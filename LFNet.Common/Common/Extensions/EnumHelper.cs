using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace LFNet.Common.Extensions
{
    /// <summary>
    /// A class to help with Enum Flags.
    /// </summary>
    public static class EnumHelper
    {
        private static T ConvertFlag<T>(ulong maskInt)
        {
            Type enumType = typeof(T);
            if (enumType.IsEnum)
            {
                return (T) Enum.ToObject(enumType, maskInt);
            }
            return (T) Convert.ChangeType(maskInt, enumType, Thread.CurrentThread.CurrentUICulture);
        }

        /// <summary>
        /// Retrieve the description on the enum, e.g.
        /// [Description("Bright Pink")]
        /// BrightPink = 2,
        /// Then when you pass in the enum, it will retrieve the description
        /// </summary>
        /// <param name="en">The Enumeration</param>
        /// <returns>A string representing the friendly name</returns>
        public static string GetDescription(Enum en)
        {
            MemberInfo[] member = en.GetType().GetMember(en.ToString());
            if (member.Length > 0)
            {
                object[] customAttributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (customAttributes.Length > 0)
                {
                    return ((DescriptionAttribute) customAttributes[0]).Description;
                }
            }
            return en.ToString();
        }

        /// <summary>
        /// Gets the default defined value of an enum.
        /// </summary>
        /// <param name="type">The enum.</param>
        /// <returns>If the value cannot be determined, 0 will be returned.</returns>
        public static object GetEnumDefaultValue(Type type)
        {
            if ((type != null) && type.IsEnum)
            {
                object obj2;
                if (TryGetEnumDefaultValue<int>(type, out obj2))
                {
                    return obj2;
                }
                if (TryGetEnumDefaultValue<byte>(type, out obj2))
                {
                    return obj2;
                }
                if (TryGetEnumDefaultValue<short>(type, out obj2))
                {
                    return obj2;
                }
                if (TryGetEnumDefaultValue<long>(type, out obj2))
                {
                    return obj2;
                }
                if (TryGetEnumDefaultValue<sbyte>(type, out obj2))
                {
                    return obj2;
                }
                if (TryGetEnumDefaultValue<ushort>(type, out obj2))
                {
                    return obj2;
                }
                if (TryGetEnumDefaultValue<uint>(type, out obj2))
                {
                    return obj2;
                }
                if (TryGetEnumDefaultValue<ulong>(type, out obj2))
                {
                    return obj2;
                }
            }
            return 0;
        }

        public static List<string> GetValues<T>() where T: struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            List<string> list = new List<string>();
            foreach (T local in Enum.GetValues(typeof(T)))
            {
                list.Add(local.ToString());
            }
            return list;
        }

        /// <summary>
        /// Determines whether any flag is on for the specified mask.
        /// </summary>
        /// <typeparam name="T">The flag type.</typeparam>
        /// <param name="mask">The mask to check if the flag is on.</param>
        /// <param name="flag">The flag to check for in the mask.</param>
        /// <returns>
        /// <c>true</c> if any flag is on for the specified mask; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAnyFlagOn<T>(this Enum mask, T flag) where T: struct, IComparable, IFormattable, IConvertible
        {
            ulong num = Convert.ToUInt64(flag);
            return ((Convert.ToUInt64(mask) & num) != 0L);
        }

        /// <summary>
        /// Determines whether the flag is on for the specified mask.
        /// </summary>
        /// <typeparam name="T">The flag type.</typeparam>
        /// <param name="mask">The mask to check if the flag is on.</param>
        /// <param name="flag">The flag to check for in the mask.</param>
        /// <returns>
        /// <c>true</c> if the flag is on for the specified mask; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFlagOn<T>(this Enum mask, T flag) where T: struct, IComparable, IFormattable, IConvertible
        {
            ulong num = Convert.ToUInt64(flag);
            return ((Convert.ToUInt64(mask) & num) == num);
        }

        /// <summary>
        /// Sets the flag off in the specified mask.
        /// </summary>
        /// <typeparam name="T">The flag type.</typeparam>
        /// <param name="mask">The mask to set flag off.</param>
        /// <param name="flag">The flag to set.</param>
        /// <returns>The mask with the flag set to off.</returns>
        public static T SetFlagOff<T>(this Enum mask, T flag) where T: struct, IComparable, IFormattable, IConvertible
        {
            ulong num = Convert.ToUInt64(flag);
            ulong maskInt = Convert.ToUInt64(mask) & ~num;
            return ConvertFlag<T>(maskInt);
        }

        /// <summary>
        /// Sets the flag on in the specified mask.
        /// </summary>
        /// <typeparam name="T">The flag type.</typeparam>
        /// <param name="mask">The mask to set flag on.</param>
        /// <param name="flag">The flag to set.</param>
        /// <returns>The mask with the flag set to on.</returns>
        public static T SetFlagOn<T>(this Enum mask, T flag) where T: struct, IComparable, IFormattable, IConvertible
        {
            ulong num = Convert.ToUInt64(flag);
            ulong maskInt = Convert.ToUInt64(mask) | num;
            return ConvertFlag<T>(maskInt);
        }

        /// <summary>
        /// Toggles the flag in the specified mask.
        /// </summary>
        /// <typeparam name="T">The flag type.</typeparam>
        /// <param name="mask">The mask to toggle the flag against.</param>
        /// <param name="flag">The flag to toggle.</param>
        /// <returns>The mask with the flag set in the opposite position then it was.</returns>
        public static T ToggleFlag<T>(this Enum mask, T flag) where T: struct, IComparable, IFormattable, IConvertible
        {
            ulong num = Convert.ToUInt64(flag);
            ulong maskInt = Convert.ToUInt64(mask) ^ num;
            return ConvertFlag<T>(maskInt);
        }

        /// <summary>
        /// Gets the string hex of the enum.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="enum">The enum to get the string hex from.</param>
        /// <returns></returns>
        public static string ToStringHex<T>(this Enum @enum) where T: struct, IComparable, IFormattable, IConvertible
        {
            return string.Format("{0:x8}", @enum);
        }

        /// <summary>
        /// Will try and parse an enum and it's default type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>True if the enum value is defined.</returns>
        public static bool TryEnumIsDefined(Type type, object value)
        {
            if (((type == null) || (value == null)) || !type.IsEnum)
            {
                return false;
            }
            return ((type == value.GetType()) || (TryEnumIsDefined<int>(type, value) || (TryEnumIsDefined<string>(type, value) || (TryEnumIsDefined<byte>(type, value) || (TryEnumIsDefined<short>(type, value) || (TryEnumIsDefined<long>(type, value) || (TryEnumIsDefined<sbyte>(type, value) || (TryEnumIsDefined<ushort>(type, value) || (TryEnumIsDefined<uint>(type, value) || TryEnumIsDefined<ulong>(type, value))))))))));
        }

        private static bool TryEnumIsDefined<T>(Type type, object value)
        {
            try
            {
                if ((value is T) && Enum.IsDefined(type, (T) value))
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// Attempts to get the default value of an enum.
        /// </summary>
        /// <typeparam name="T">The System Type.</typeparam>
        /// <param name="type"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static bool TryGetEnumDefaultValue<T>(Type type, out object defaultValue)
        {
            defaultValue = null;
            try
            {
                defaultValue = (T) type.GetField(Enum.GetValues(type).GetValue(0).ToString()).GetValue(null);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// Tries to get an enum from a string.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value.</param>
        /// <param name="input">The input string.</param>
        /// <param name="returnValue">The return enum value.</param>
        /// <returns>
        /// <c>true</c> if the string was able to be parsed to an enum; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParseEnum<T>(this Enum value, string input, out T returnValue) where T: struct, IComparable, IFormattable, IConvertible
        {
            returnValue = default(T);
            if (!input.IsNullOrEmpty())
            {
                Type enumType = typeof(T);
                if (enumType.IsEnum && Enum.IsDefined(enumType, input))
                {
                    returnValue = (T) Enum.Parse(enumType, input, true);
                    return true;
                }
            }
            return false;
        }
    }
}

