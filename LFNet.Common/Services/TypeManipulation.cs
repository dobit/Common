using System;
using System.ComponentModel;

namespace LFNet.Services
{
    /// <summary>
    /// Some handy type conversion routines.
    /// </summary>
    class TypeManipulation
    {
        /// <summary>
        /// Does its best to convert whatever the value is into the destination
        /// type. Null in yields null out for value types and the default(T)
        /// for value types (this may change.)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns>An object of the destination type.</returns>
        public static object ChangeToCompatibleType(object value, Type destinationType)
        {
            Enforce.ArgumentNotNull(destinationType, "destinationType");

            if (value == null)
            {
                if (destinationType.IsValueType)
                    return Activator.CreateInstance(destinationType);

                return null;
            }

            if (destinationType.IsAssignableFrom(value.GetType()))
                return value;

            return TypeDescriptor.GetConverter(destinationType).ConvertFrom(value);
        }
    }
}