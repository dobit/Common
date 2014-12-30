using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using LFNet.Common.Reflection;

namespace LFNet.Common.Visitor
{
	public abstract class DynamicVisitor<TBase>
	{
		private class VisitorMethodInfo
		{
			public Type ItemType
			{
				get;
				private set;
			}
			public bool HasVisitChildrenFlag
			{
				get;
				private set;
			}
			public LateBoundMethod Method
			{
				get;
				private set;
			}
			public VisitorMethodInfo(Type itemType, LateBoundMethod method, bool hasVisitChildrenFlag = false)
			{
				this.ItemType = itemType;
				this.Method = method;
				this.HasVisitChildrenFlag = hasVisitChildrenFlag;
			}
		}
		private static readonly ConcurrentDictionary<Type, List<DynamicVisitor<TBase>.VisitorMethodInfo>> _typeVisitorMethodsCache = new ConcurrentDictionary<Type, List<DynamicVisitor<TBase>.VisitorMethodInfo>>();
		private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, List<DynamicVisitor<TBase>.VisitorMethodInfo>>> _itemTypeVisitorsCache = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, List<DynamicVisitor<TBase>.VisitorMethodInfo>>>();
		private readonly ConcurrentDictionary<Type, List<DynamicVisitor<TBase>.VisitorMethodInfo>> _itemTypeVisitors = new ConcurrentDictionary<Type, List<DynamicVisitor<TBase>.VisitorMethodInfo>>();
		private readonly List<DynamicVisitor<TBase>.VisitorMethodInfo> _typeVisitorMethods;
		/// <summary>
		/// Determines if the visitor should call the visitors of any registered delegates that are base types of the being visited.
		/// </summary>
		public bool ShouldCallBaseTypeVisitors
		{
			get;
			set;
		}
		/// <summary>
		/// Determines if the visiting should stop after one successful visit.
		/// </summary>
		public bool ShouldCallMultipleVisitors
		{
			get;
			set;
		}
		protected DynamicVisitor()
		{
			this.ShouldCallBaseTypeVisitors = false;
			this.ShouldCallMultipleVisitors = false;
			this._itemTypeVisitors = DynamicVisitor<TBase>._itemTypeVisitorsCache.GetOrAdd(base.GetType(), new ConcurrentDictionary<Type, List<DynamicVisitor<TBase>.VisitorMethodInfo>>());
			this._typeVisitorMethods = DynamicVisitor<TBase>._typeVisitorMethodsCache.GetOrAdd(base.GetType(), delegate(Type t)
			{
				List<DynamicVisitor<TBase>.VisitorMethodInfo> list = new List<DynamicVisitor<TBase>.VisitorMethodInfo>();
				foreach (MethodInfo current in 
					from m in t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic)
					where m.Name == "Visit"
					select m)
				{
					ParameterInfo[] parameters = current.GetParameters();
					if (parameters.Length == 1)
					{
						list.Add(new DynamicVisitor<TBase>.VisitorMethodInfo(parameters[0].ParameterType, DelegateFactory.CreateMethod(current), false));
					}
					if (parameters.Length == 2 && parameters[1].ParameterType == typeof(CancelEventArgs))
					{
						list.Add(new DynamicVisitor<TBase>.VisitorMethodInfo(parameters[0].ParameterType, DelegateFactory.CreateMethod(current), true));
					}
				}
				return list;
			});
		}
		protected abstract IEnumerable<TBase> GetChildren(TBase item);
		public void Visit<TSub>(TSub item) where TSub : TBase
		{
			CancelEventArgs cancelEventArgs = new CancelEventArgs();
			Type type = item.GetType();
			IEnumerable<DynamicVisitor<TBase>.VisitorMethodInfo> typeVisitors = this.GetTypeVisitors(type);
			foreach (DynamicVisitor<TBase>.VisitorMethodInfo current in typeVisitors)
			{
				if (this.ShouldCallBaseTypeVisitors || !(current.ItemType != type))
				{
					this.InvokeVisitor(current, item, cancelEventArgs);
					if (!this.ShouldCallMultipleVisitors || cancelEventArgs.Cancel)
					{
						break;
					}
				}
			}
			if (!cancelEventArgs.Cancel)
			{
				foreach (TBase current2 in this.GetChildren((TBase)((object)item)))
				{
					this.Visit<TBase>(current2);
				}
			}
		}
		private IEnumerable<DynamicVisitor<TBase>.VisitorMethodInfo> GetTypeVisitors(Type type)
		{
			return this._itemTypeVisitors.GetOrAdd(type, delegate(Type t)
			{
				List<DynamicVisitor<TBase>.VisitorMethodInfo> list = new List<DynamicVisitor<TBase>.VisitorMethodInfo>();
				foreach (DynamicVisitor<TBase>.VisitorMethodInfo current in this._typeVisitorMethods)
				{
					if (current.ItemType == type)
					{
						list.Add(current);
					}
					else
					{
						if (current.ItemType.IsAssignableFrom(t))
						{
							list.Add(current);
						}
					}
				}
				return list;
			});
		}
		private void InvokeVisitor(DynamicVisitor<TBase>.VisitorMethodInfo methodInfo, object item, CancelEventArgs visitChildren)
		{
			if (methodInfo.HasVisitChildrenFlag)
			{
				methodInfo.Method(this, new object[]
				{
					item,
					visitChildren
				});
				return;
			}
			methodInfo.Method(this, new object[]
			{
				item
			});
		}
	}
}
