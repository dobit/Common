using System;
using System.Reflection;

namespace LFNet.Common
{
    /// <summary>
    /// 空默认值
    /// </summary>
    public class Null
    {
        /// <summary>
        /// Gets the null short.
        /// </summary>
        /// <remarks></remarks>
        public static short NullShort
        {
            get { return -1; }
        }

        /// <summary>
        /// Gets the null integer.
        /// </summary>
        /// <remarks></remarks>
        public static int NullInteger
        {
            get { return -1; }
        }

        /// <summary>
        /// Gets the null byte.
        /// </summary>
        /// <remarks></remarks>
        public static byte NullByte
        {
            get { return 255; }
        }

        /// <summary>
        /// Gets the null single.
        /// </summary>
        /// <remarks></remarks>
        public static float NullSingle
        {
            get { return float.MinValue; }
        }

        /// <summary>
        /// Gets the null double.
        /// </summary>
        /// <remarks></remarks>
        public static double NullDouble
        {
            get { return double.MinValue; }
        }

        /// <summary>
        /// Gets the null decimal.
        /// </summary>
        /// <remarks></remarks>
        public static decimal NullDecimal
        {
            get { return decimal.MinValue; }
        }

        /// <summary>
        /// Gets the null date.
        /// </summary>
        /// <remarks></remarks>
        public static DateTime NullDate
        {
            get { return DateTime.MinValue; }
        }

        /// <summary>
        /// Gets the null string.
        /// </summary>
        /// <remarks></remarks>
        public static string NullString
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets a value indicating whether [null boolean].
        /// </summary>
        /// <remarks></remarks>
        public static bool NullBoolean
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the null GUID.
        /// </summary>
        /// <remarks></remarks>
        public static Guid NullGuid
        {
            get { return Guid.Empty; }
        }

        /// <summary>
        /// Sets the null.
        /// </summary>
        /// <param name="objValue">The obj value.</param>
        /// <param name="objField">The obj field.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object SetNull(object objValue, object objField)
        {
            object returnValue;
            if (objValue == DBNull.Value)
            {
                if (objField is short)
                {
                    returnValue = NullShort;
                }
                else if (objField is byte)
                {
                    returnValue = NullByte;
                }
                else if (objField is int)
                {
                    returnValue = NullInteger;
                }
                else if (objField is float)
                {
                    returnValue = NullSingle;
                }
                else if (objField is double)
                {
                    returnValue = NullDouble;
                }
                else if (objField is decimal)
                {
                    returnValue = NullDecimal;
                }
                else if (objField is DateTime)
                {
                    returnValue = NullDate;
                }
                else if (objField is string)
                {
                    returnValue = NullString;
                }
                else if (objField is bool)
                {
                    returnValue = NullBoolean;
                }
                else if (objField is Guid)
                {
                    returnValue = NullGuid;
                }
                else
                {
                    returnValue = null;
                }
            }
            else
            {
                returnValue = objValue;
            }
            return returnValue;
        }

        /// <summary>
        /// Sets the null.
        /// </summary>
        /// <param name="objPropertyInfo">The obj property info.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object SetNull(PropertyInfo objPropertyInfo)
        {
            object returnValue;
            switch (objPropertyInfo.PropertyType.ToString())
            {
                case "System.Int16":
                    returnValue = NullShort;
                    break;
                case "System.Int32":
                case "System.Int64":
                    returnValue = NullInteger;
                    break;
                case "system.Byte":
                    returnValue = NullByte;
                    break;
                case "System.Single":
                    returnValue = NullSingle;
                    break;
                case "System.Double":
                    returnValue = NullDouble;
                    break;
                case "System.Decimal":
                    returnValue = NullDecimal;
                    break;
                case "System.DateTime":
                    returnValue = NullDate;
                    break;
                case "System.String":
                case "System.Char":
                    returnValue = NullString;
                    break;
                case "System.Boolean":
                    returnValue = NullBoolean;
                    break;
                case "System.Guid":
                    returnValue = NullGuid;
                    break;
                default:
                    Type pType = objPropertyInfo.PropertyType;
                    if (pType.BaseType.Equals(typeof (Enum)))
                    {
                        Array objEnumValues = Enum.GetValues(pType);
                        Array.Sort(objEnumValues);
                        returnValue = Enum.ToObject(pType, objEnumValues.GetValue(0));
                    }
                    else
                    {
                        returnValue = null;
                    }
                    break;
            }
            return returnValue;
        }

        /// <summary>
        /// Sets the null boolean.
        /// </summary>
        /// <param name="objValue">The obj value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool SetNullBoolean(object objValue)
        {
            bool retValue = NullBoolean;
            if (objValue != DBNull.Value)
            {
                retValue = Convert.ToBoolean(objValue);
            }
            return retValue;
        }

        /// <summary>
        /// Sets the null date time.
        /// </summary>
        /// <param name="objValue">The obj value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DateTime SetNullDateTime(object objValue)
        {
            DateTime retValue = NullDate;
            if (objValue != DBNull.Value)
            {
                retValue = Convert.ToDateTime(objValue);
            }
            return retValue;
        }

        /// <summary>
        /// Sets the null integer.
        /// </summary>
        /// <param name="objValue">The obj value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int SetNullInteger(object objValue)
        {
            int retValue = NullInteger;
            if (objValue != DBNull.Value)
            {
                retValue = Convert.ToInt32(objValue);
            }
            return retValue;
        }

        /// <summary>
        /// Sets the null single.
        /// </summary>
        /// <param name="objValue">The obj value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static float SetNullSingle(object objValue)
        {
            float retValue = NullSingle;
            if (objValue != DBNull.Value)
            {
                retValue = Convert.ToSingle(objValue);
            }
            return retValue;
        }

        /// <summary>
        /// Sets the null string.
        /// </summary>
        /// <param name="objValue">The obj value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string SetNullString(object objValue)
        {
            string retValue = NullString;
            if (objValue != DBNull.Value)
            {
                retValue = Convert.ToString(objValue);
            }
            return retValue;
        }

        /// <summary>
        /// Sets the null GUID.
        /// </summary>
        /// <param name="objValue">The obj value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Guid SetNullGuid(object objValue)
        {
            Guid retValue = Guid.Empty;
            if ((objValue != DBNull.Value) & !string.IsNullOrEmpty(objValue.ToString()))
            {
                retValue = new Guid(objValue.ToString());
            }
            return retValue;
        }

        /// <summary>
        /// Gets the null.
        /// </summary>
        /// <param name="objField">The obj field.</param>
        /// <param name="objDBNull">The obj DBNull.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object GetNull(object objField, object objDBNull)
        {
            object returnValue = objField;
            if (objField == null)
            {
                returnValue = objDBNull;
            }
            else if (objField is byte)
            {
                if (Convert.ToByte(objField) == NullByte)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is short)
            {
                if (Convert.ToInt16(objField) == NullShort)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is int)
            {
                if (Convert.ToInt32(objField) == NullInteger)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is float)
            {
                if (Convert.ToSingle(objField) == NullSingle)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is double)
            {
                if (Convert.ToDouble(objField) == NullDouble)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is decimal)
            {
                if (Convert.ToDecimal(objField) == NullDecimal)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is DateTime)
            {
                if (Convert.ToDateTime(objField).Date == NullDate.Date)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is string)
            {
                if (objField == null)
                {
                    returnValue = objDBNull;
                }
                else
                {
                    if (objField.ToString() == NullString)
                    {
                        returnValue = objDBNull;
                    }
                }
            }
            else if (objField is bool)
            {
                if (Convert.ToBoolean(objField) == NullBoolean)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is Guid)
            {
                if (((Guid) objField).Equals(NullGuid))
                {
                    returnValue = objDBNull;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Determines whether the specified obj field is null.
        /// </summary>
        /// <param name="objField">The obj field.</param>
        /// <returns><c>true</c> if the specified obj field is null; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public static bool IsNull(object objField)
        {
            bool isNull;
            if (objField != null)
            {
                if (objField is int)
                {
                    isNull = objField.Equals(NullInteger);
                }
                else if (objField is short)
                {
                    isNull = objField.Equals(NullShort);
                }
                else if (objField is byte)
                {
                    isNull = objField.Equals(NullByte);
                }
                else if (objField is float)
                {
                    isNull = objField.Equals(NullSingle);
                }
                else if (objField is double)
                {
                    isNull = objField.Equals(NullDouble);
                }
                else if (objField is decimal)
                {
                    isNull = objField.Equals(NullDecimal);
                }
                else if (objField is DateTime)
                {
                    var objDate = (DateTime) objField;
                    isNull = objDate.Date.Equals(NullDate.Date);
                }
                else if (objField is string)
                {
                    isNull = objField.Equals(NullString);
                }
                else if (objField is bool)
                {
                    isNull = objField.Equals(NullBoolean);
                }
                else if (objField is Guid)
                {
                    isNull = objField.Equals(NullGuid);
                }
                else
                {
                    isNull = false;
                }
            }
            else
            {
                isNull = true;
            }
            return isNull;
        }
    }
}