using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace LFNet.Common
{
    /// <summary>
    /// 提供各种转换操作类
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// string型转换为bool型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的bool类型结果</returns>
        /// <remarks></remarks>
        public static bool StrToBool(object expression, bool defValue)
        {
            if (expression != null)
                return StrToBool(expression.ToString(), defValue);

            return defValue;
        }

        /// <summary>
        /// string型转换为bool型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的bool类型结果</returns>
        /// <remarks></remarks>
        public static bool StrToBool(string expression, bool defValue)
        {
            if (expression != null)
            {
                if (string.Compare(expression, "true", true) == 0)
                    return true;
                if (string.Compare(expression, "false", true) == 0)
                    return false;
            }
            return defValue;
        }

        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        /// <remarks></remarks>
        public static int ObjectToInt(object expression)
        {
            return ObjectToInt(expression, 0);
        }

        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int ObjectToInt(object expression, int defValue)
        {
            if (expression != null)
                return StrToInt(expression.ToString(), defValue);

            return defValue;
        }

        /// <summary>
        /// 将对象转换为Int32类型,转换失败返回0
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        public static int StrToInt(string str)
        {
            return StrToInt(str, 0);
        }

        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int StrToInt(string str, int defValue)
        {
            if (string.IsNullOrEmpty(str) || str.Trim().Length >= 11 ||
                !Regex.IsMatch(str.Trim(), @"^([-]|[0-9])[0-9]*(\.\w*)?$"))
                return defValue;

            int rv;
            if (Int32.TryParse(str, out rv))
                return rv;

            return Convert.ToInt32(StrToFloat(str, defValue));
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float StrToFloat(object strValue, float defValue)
        {
            if ((strValue == null))
                return defValue;

            return StrToFloat(strValue.ToString(), defValue);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float ObjectToFloat(object strValue, float defValue)
        {
            if ((strValue == null))
                return defValue;

            return StrToFloat(strValue.ToString(), defValue);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        /// <remarks></remarks>
        public static float ObjectToFloat(object strValue)
        {
            return ObjectToFloat(strValue.ToString(), 0);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        public static float StrToFloat(object strValue)
        {
            if ((strValue == null))
                return 0;

            return StrToFloat(strValue.ToString(), 0);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float StrToFloat(string strValue, float defValue)
        {
            if ((strValue == null) || (strValue.Length > 10))
                return defValue;

            float intValue = defValue;
            bool isFloat = Regex.IsMatch(strValue, @"^([-]|[0-9])[0-9]*(\.\w*)?$");
            if (isFloat)
                float.TryParse(strValue, out intValue);
            return intValue;
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="objectValue">需要转换的对象</param>
        /// <returns></returns>
        public static T ToGenericParameter<T>(object objectValue)
        {
            return (T) Convert.ChangeType(objectValue, typeof (T));
        }

        /// <summary>
        /// int型转换为string型
        /// </summary>
        /// <returns>转换后的string类型结果</returns>
        public static string IntToStr(int intValue)
        {
            return Convert.ToString(intValue);
        }

        /// <summary>
        /// 转换为简体中文
        /// </summary>
        public static string ToSChinese(string str)
        {
            return Strings.StrConv(str, VbStrConv.SimplifiedChinese);
        }

        /// <summary>
        /// 转换为繁体中文
        /// </summary>
        public static string ToTChinese(string str)
        {
            return Strings.StrConv(str, VbStrConv.TraditionalChinese);
        }

        /// <summary>
        /// 将数据表转换成JSON类型串
        /// </summary>
        /// <param name="dt">要转换的数据表</param>
        /// <returns></returns>
        public static StringBuilder DataTableToJSON(DataTable dt)
        {
            return DataTableToJSON(dt, true);
        }

        /// <summary>
        /// 将数据表转换成JSON类型串
        /// </summary>
        /// <param name="dt">要转换的数据表</param>
        /// <param name="dispose">数据表转换结束后是否dispose掉</param>
        /// <returns></returns>
        public static StringBuilder DataTableToJSON(DataTable dt, bool dispose)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("[\r\n");

            //数据表字段名和类型数组
            var dtField = new string[dt.Columns.Count];
            int i = 0;
            string formatStr = "{{";
            foreach (DataColumn dc in dt.Columns)
            {
                dtField[i] = dc.Caption.ToLower().Trim();
                formatStr += "'" + dc.Caption.ToLower().Trim() + "':";
                string fieldtype = dc.DataType.ToString().Trim().ToLower();
                if (fieldtype.IndexOf("int") > 0 || fieldtype.IndexOf("deci") > 0 ||
                    fieldtype.IndexOf("floa") > 0 || fieldtype.IndexOf("doub") > 0 ||
                    fieldtype.IndexOf("bool") > 0)
                {
                    formatStr += "{" + i + "}";
                }
                else
                {
                    formatStr += "'{" + i + "}'";
                }
                formatStr += ",";
                i++;
            }

            if (formatStr.EndsWith(","))
                formatStr = formatStr.Substring(0, formatStr.Length - 1); //去掉尾部","号

            formatStr += "}},";

            i = 0;
            var objectArray = new object[dtField.Length];
            foreach (DataRow dr in dt.Rows)
            {
                foreach (string fieldname in dtField)
                {
                    //对 \ , ' 符号进行转换 
                    objectArray[i] = dr[dtField[i]].ToString().Trim().Replace("\\", "\\\\").Replace("'", "\\'");
                    switch (objectArray[i].ToString())
                    {
                        case "True":
                            {
                                objectArray[i] = "true";
                                break;
                            }
                        case "False":
                            {
                                objectArray[i] = "false";
                                break;
                            }
                        default:
                            break;
                    }
                    i++;
                }
                i = 0;
                stringBuilder.Append(string.Format(formatStr, objectArray));
            }
            if (stringBuilder.ToString().EndsWith(","))
                stringBuilder.Remove(stringBuilder.Length - 1, 1); //去掉尾部","号

            if (dispose)
                dt.Dispose();

            return stringBuilder.Append("\r\n];");
        }

        /// <summary>
        /// 将全角数字转换为数字
        /// </summary>
        /// <param name="sbcCase">The SBC case.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string SbcCaseToNumberic(string sbcCase)
        {
            char[] c = sbcCase.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                byte[] b = Encoding.Unicode.GetBytes(c, i, 1);
                if (b.Length == 2)
                {
                    if (b[1] == 255)
                    {
                        b[0] = (byte) (b[0] + 32);
                        b[1] = 0;
                        c[i] = Encoding.Unicode.GetChars(b)[0];
                    }
                }
            }
            return new string(c);
        }

        /// <summary>
        /// 将字符串转换为Color
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Color ToColor(string color)
        {
            int red, green, blue;
            char[] rgb;
            color = color.TrimStart('#');
            color = Regex.Replace(color.ToLower(), "[g-zG-Z]", "");
            switch (color.Length)
            {
                case 3:
                    rgb = color.ToCharArray();
                    red = Convert.ToInt32(rgb[0] + rgb[0].ToString(), 16);
                    green = Convert.ToInt32(rgb[1] + rgb[1].ToString(), 16);
                    blue = Convert.ToInt32(rgb[2] + rgb[2].ToString(), 16);
                    return Color.FromArgb(red, green, blue);
                case 6:
                    rgb = color.ToCharArray();
                    red = Convert.ToInt32(rgb[0] + rgb[1].ToString(), 16);
                    green = Convert.ToInt32(rgb[2] + rgb[3].ToString(), 16);
                    blue = Convert.ToInt32(rgb[4] + rgb[5].ToString(), 16);
                    return Color.FromArgb(red, green, blue);
                default:
                    return Color.FromName(color);
            }
        }


        /// <summary>
        /// Returns a string representing a Hex view of a byte array. Slow.
        /// </summary>
        /// <param name="inArr"></param>
        /// <param name="iBytesPerLine"></param>
        /// <returns></returns>
        [Description("Returns a string representing a Hex view of a byte array. Slow.")]
        public static string ByteArrayToHexView(byte[] inArr, int iBytesPerLine)
        {
            return ByteArrayToHexView(inArr, iBytesPerLine, inArr.Length);
        }

        /// <summary>
        /// Returns a string representing a Hex view of a byte array. PERF: Slow.
        /// </summary>
        /// <param name="inArr"></param>
        /// <param name="iBytesPerLine"></param>
        /// <param name="iMaxByteCount"></param>
        /// <returns></returns>
        [Description("Returns a string representing a Hex view of a byte array. PERF: Slow.")]
        public static string ByteArrayToHexView(byte[] inArr, int iBytesPerLine, int iMaxByteCount)
        {
            if ((inArr == null) || (inArr.Length == 0))
            {
                return string.Empty;
            }
            if ((iBytesPerLine < 1) || (iMaxByteCount < 1))
            {
                throw new ArgumentOutOfRangeException("iBytesPerLine", "iBytesPerLine and iMaxByteCount must be >0");
            }
            iMaxByteCount = Math.Min(iMaxByteCount, inArr.Length);
            var builder = new StringBuilder(iMaxByteCount*5);
            int num = 0;
            while (num < iMaxByteCount)
            {
                int num2 = Math.Min(iBytesPerLine, iMaxByteCount - num);
                bool flag = num2 < iBytesPerLine;
                for (int i = 0; i < num2; i++)
                {
                    builder.Append(inArr[num + i].ToString("X2"));
                    builder.Append(" ");
                }
                if (flag)
                {
                    builder.Append(new string(' ', 3*(iBytesPerLine - num2)));
                }
                builder.Append(" ");
                for (int j = 0; j < num2; j++)
                {
                    if (inArr[num + j] < 32)
                    {
                        builder.Append(".");
                    }
                    else
                    {
                        builder.Append((char) inArr[num + j]);
                    }
                }
                if (flag)
                {
                    builder.Append(new string(' ', iBytesPerLine - num2));
                }
                builder.Append("\r\n");
                num += iBytesPerLine;
            }
            return builder.ToString();
        }


        /// <summary>
        /// Returns a string representing a Hex stream of a byte array. Slow.
        /// </summary>
        /// <param name="inArr"></param>
        /// <returns></returns>
        [Description("Returns a string representing a Hex stream of a byte array. Slow.")]
        public static string ByteArrayToString(byte[] inArr)
        {
            if (inArr == null)
            {
                return "null";
            }
            if (inArr.Length == 0)
            {
                return "empty";
            }
            var builder = new StringBuilder(inArr.Length*3);
            for (int i = 0; i < inArr.Length; i++)
            {
                builder.Append(inArr[i].ToString("X2") + ' ');
            }
            return builder.ToString();
        }

        /// <summary>
        /// 对象转换成二进制流
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ObjectToByte(object obj)
        {
            if (obj == null) return null;
            var stream = new MemoryStream();
            IFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, obj);
            byte[] bytes = stream.GetBuffer();
            stream.Close();
            return bytes;
        }

        /// <summary>
        /// 二进制流还原成对象
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static object ByteToObject(byte[] bytes)
        {
            if (bytes == null) return null;
            var stream = new MemoryStream(bytes);
            IFormatter bf = new BinaryFormatter();
            Object reobj = bf.Deserialize(stream);
            stream.Close();
            return reobj;
        }

        /// <summary>
        /// 将long型数值转换为Int32类型
        /// </summary>
        /// <param name="objNum"></param>
        /// <returns></returns>
        public static int SafeInt32(object objNum)
        {
            if (objNum == null)
                return 0;

            string strNum = objNum.ToString();
            if (Validator.IsNumeric(strNum))
            {
                if (strNum.Length > 9)
                {
                    return strNum.StartsWith("-") ? int.MinValue : int.MaxValue;
                }
                return Int32.Parse(strNum);
            }
            return 0;
        }

        /// <summary>
        /// 类型转换扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="convertibleValue"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this IConvertible convertibleValue)
        {

            if (null == convertibleValue)
            {
                return default(T);
            }
            if (!typeof(T).IsGenericType)
            {
                return (T)Convert.ChangeType(convertibleValue, typeof(T));
            }
            else
            {
                Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return (T)Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(typeof(T)));
                }

            }
            throw new InvalidCastException(string.Format("Invalid cast from type \"{0}\" to type \"{1}\".", convertibleValue.GetType().FullName, typeof(T).FullName));
        }

        public static T ConvertTo<T>(this object v)
        {
            Type enumType = typeof(T);
            Type type = v.GetType();
            if (enumType.Equals(type))
            {
                return (T)v;
            }
            if (enumType.IsEnum && type.Equals(typeof(string)))
            {
                return (T)Enum.Parse(enumType, v.ToString());
            }
            if (enumType.Equals(typeof(bool)))
            {
                return (T)ToBoolean(v);
            }
            return (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture);
        }
        private static object ToBoolean(object value)
        {
            bool flag;
            int num;
            if (bool.TryParse(value.ToString(), out flag))
            {
                return flag;
            }
            if (int.TryParse(value.ToString(), out num))
            {
                return Convert.ToBoolean(num);
            }
            return Convert.ToBoolean(value);
        }
    }

}