using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace LFNet.Common.Extensions
{
    public static class PropertyChangeExtensions
    {
        /// <summary>
        /// Sets the property value if the value is different fromt he existing value. 
        /// The PropertyChanging and PropertyChanged events are raised if the value is updated.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="changedHandler">The handler for the PropertyChanged event.</param>
        /// <param name="changingHandler">The handler for the PropertyChanging event.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="oldValueExpression">The old value expression.</param>
        /// <param name="setter">The setter delegate.</param>
        /// <returns>The new value.</returns>
        /// <example>The following is an example of a Name property.
        /// <code>
        /// <![CDATA[
        /// private string _name;
        /// public string Name
        /// {
        /// get { return _name; }
        /// set { PropertyChanged.SetValue(PropertyChanging, value, () => Name, v => _name = v); }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static T SetValue<T>(this PropertyChangedEventHandler changedHandler, PropertyChangingEventHandler changingHandler, T newValue, Expression<Func<T>> oldValueExpression, Action<T> setter)
        {
            T objA = oldValueExpression.Compile()();
            if (!object.Equals(objA, default(T)) || !object.Equals(newValue, default(T)))
            {
                if (object.Equals(objA, newValue))
                {
                    return newValue;
                }
                MemberExpression body = oldValueExpression.Body as MemberExpression;
                string name = body.Member.Name;
                ConstantExpression expression = body.Expression as ConstantExpression;
                object sender = expression.Value;
                PropertyChangingEventHandler handler = changingHandler;
                if (handler != null)
                {
                    handler(sender, new PropertyChangingEventArgs(name));
                }
                setter(newValue);
                PropertyChangedEventHandler handler2 = changedHandler;
                if (handler2 != null)
                {
                    handler2(sender, new PropertyChangedEventArgs(name));
                }
            }
            return newValue;
        }

        /// <summary>
        /// Sets the property value if the value is different fromt he existing value. 
        /// The PropertyChanging and PropertyChanged events are raised if the value is updated.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="changedHandler">The handler for the PropertyChanged event.</param>
        /// <param name="changingHandler">The handler for the PropertyChanging event.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="oldValueExpression">The old value expression.</param>
        /// <param name="setter">The setter delegate.</param>
        /// <returns>The new value.</returns>
        public static T SetValue<T>(this PropertyChangingEventHandler changingHandler, PropertyChangedEventHandler changedHandler, T newValue, Expression<Func<T>> oldValueExpression, Action<T> setter)
        {
            return changedHandler.SetValue<T>(changingHandler, newValue, oldValueExpression, setter);
        }
    }
}

