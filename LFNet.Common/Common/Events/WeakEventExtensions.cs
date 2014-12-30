using System;
using System.ComponentModel;

namespace LFNet.Common.Events
{
    /// <summary>
    /// Utilities for the weak event method
    /// </summary>
    public static class WeakEventExtensions
    {
        public static void CheckArgs(Delegate eventHandler, Delegate unregister)
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException("eventHandler");
            }
            if (eventHandler.Method.IsStatic || (eventHandler.Target == null))
            {
                throw new ArgumentException("Only instance methods are supported.", "eventHandler");
            }
        }

        public static object GetWeakHandler(Type generalType, Type[] genericTypes, Type[] constructorArgTypes, object[] constructorArgs)
        {
            return generalType.MakeGenericType(genericTypes).GetConstructor(constructorArgTypes).Invoke(constructorArgs);
        }

        /// <summary>
        /// Makes a property change handler weak
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="unregister">The unregister.</param>
        /// <returns></returns>
        public static PropertyChangedEventHandler MakeWeak(this PropertyChangedEventHandler eventHandler, UnregisterDelegate<PropertyChangedEventHandler> unregister)
        {
            CheckArgs(eventHandler, unregister);
            Type generalType = typeof(WeakPropertyChangeHandler<>);
            Type[] genericTypes = new Type[] { eventHandler.Method.DeclaringType };
            Type[] constructorArgTypes = new Type[] { typeof(PropertyChangedEventHandler), typeof(UnregisterDelegate<PropertyChangedEventHandler>) };
            object[] constructorArgs = new object[] { eventHandler, unregister };
            return ((IWeakPropertyChangedEventHandler) GetWeakHandler(generalType, genericTypes, constructorArgTypes, constructorArgs)).Handler;
        }

        /// <summary>
        /// Makes a generic handler weak
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="unregister">The unregister.</param>
        /// <returns></returns>
        public static EventHandler<E> MakeWeak<E>(this EventHandler<E> eventHandler, UnregisterDelegate<EventHandler<E>> unregister) where E: EventArgs
        {
            CheckArgs(eventHandler, unregister);
            Type generalType = typeof(WeakEventHandler<,>);
            Type[] genericTypes = new Type[] { eventHandler.Method.DeclaringType, typeof(E) };
            Type[] constructorArgTypes = new Type[] { typeof(EventHandler<E>), typeof(UnregisterDelegate<EventHandler<E>>) };
            object[] constructorArgs = new object[] { eventHandler, unregister };
            return ((IWeakEventHandler<E>) GetWeakHandler(generalType, genericTypes, constructorArgTypes, constructorArgs)).Handler;
        }
    }
}

