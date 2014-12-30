using System;
using System.Collections;

namespace LFNet.Common.Threading.Internal
{
	/// <summary>
	/// PriorityQueue class
	/// This class is not thread safe because we use external lock
	/// </summary>
	public sealed class PriorityQueue : IEnumerable
	{
		/// <summary>
		/// The class the implements the enumerator
		/// </summary>
		private class PriorityQueueEnumerator : IEnumerator
		{
			private PriorityQueue _priorityQueue;
			private int _version;
			private int _queueIndex;
			private IEnumerator _enumerator;
			public object Current
			{
				get
				{
					return this._enumerator.Current;
				}
			}
			public PriorityQueueEnumerator(PriorityQueue priorityQueue)
			{
				this._priorityQueue = priorityQueue;
				this._version = this._priorityQueue._version;
				this._queueIndex = this._priorityQueue.GetNextNonEmptyQueue(-1);
				if (this._queueIndex >= 0)
				{
					this._enumerator = this._priorityQueue._queues[this._queueIndex].GetEnumerator();
					return;
				}
				this._enumerator = null;
			}
			public void Reset()
			{
				this._version = this._priorityQueue._version;
				this._queueIndex = this._priorityQueue.GetNextNonEmptyQueue(-1);
				if (this._queueIndex >= 0)
				{
					this._enumerator = this._priorityQueue._queues[this._queueIndex].GetEnumerator();
					return;
				}
				this._enumerator = null;
			}
			public bool MoveNext()
			{
				if (this._enumerator == null)
				{
					return false;
				}
				if (this._version != this._priorityQueue._version)
				{
					throw new InvalidOperationException("The collection has been modified");
				}
				if (this._enumerator.MoveNext())
				{
					return true;
				}
				this._queueIndex = this._priorityQueue.GetNextNonEmptyQueue(this._queueIndex);
				if (-1 == this._queueIndex)
				{
					return false;
				}
				this._enumerator = this._priorityQueue._queues[this._queueIndex].GetEnumerator();
				this._enumerator.MoveNext();
				return true;
			}
		}
		/// <summary>
		/// The number of queues, there is one for each type of priority
		/// </summary>
		private const int _queuesCount = 5;
		/// <summary>
		/// Work items queues. There is one for each type of priority
		/// </summary>
		private Queue[] _queues = new Queue[5];
		/// <summary>
		/// The total number of work items within the queues 
		/// </summary>
		private int _workItemsCount;
		/// <summary>
		/// Use with IEnumerable interface
		/// </summary>
		private int _version;
		/// <summary>
		/// The number of work items 
		/// </summary>
		public int Count
		{
			get
			{
				return this._workItemsCount;
			}
		}
		public PriorityQueue()
		{
			for (int i = 0; i < this._queues.Length; i++)
			{
				this._queues[i] = new Queue();
			}
		}
		/// <summary>
		/// Enqueue a work item.
		/// </summary>
		/// <param name="workItem">A work item</param>
		public void Enqueue(IHasWorkItemPriority workItem)
		{
			//int num = (WorkItemPriority)5 - workItem.WorkItemPriority - WorkItemPriority.BelowNormal;
            int num = 5 - workItem.WorkItemPriority - WorkItemPriority.BelowNormal;
			this._queues[num].Enqueue(workItem);
			this._workItemsCount++;
			this._version++;
		}
		/// <summary>
		/// Dequeque a work item.
		/// </summary>
		/// <returns>Returns the next work item</returns>
		public IHasWorkItemPriority Dequeue()
		{
			IHasWorkItemPriority result = null;
			if (this._workItemsCount > 0)
			{
				int nextNonEmptyQueue = this.GetNextNonEmptyQueue(-1);
				result = (this._queues[nextNonEmptyQueue].Dequeue() as IHasWorkItemPriority);
				this._workItemsCount--;
				this._version++;
			}
			return result;
		}
		/// <summary>
		/// Find the next non empty queue starting at queue queueIndex+1
		/// </summary>
		/// <param name="queueIndex">The index-1 to start from</param>
		/// <returns>
		/// The index of the next non empty queue or -1 if all the queues are empty
		/// </returns>
		private int GetNextNonEmptyQueue(int queueIndex)
		{
			for (int i = queueIndex + 1; i < 5; i++)
			{
				if (this._queues[i].Count > 0)
				{
					return i;
				}
			}
			return -1;
		}
		/// <summary>
		/// Clear all the work items 
		/// </summary>
		public void Clear()
		{
			if (this._workItemsCount > 0)
			{
				Queue[] queues = this._queues;
				for (int i = 0; i < queues.Length; i++)
				{
					Queue queue = queues[i];
					queue.Clear();
				}
				this._workItemsCount = 0;
				this._version++;
			}
		}
		/// <summary>
		/// Returns an enumerator to iterate over the work items
		/// </summary>
		/// <returns>Returns an enumerator</returns>
		public IEnumerator GetEnumerator()
		{
			return new PriorityQueue.PriorityQueueEnumerator(this);
		}
	}
}
