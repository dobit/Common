using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using LFNet.Common.Collections;
using LFNet.Common.Text;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace LFNet.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex _identifierRegex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");
        private const string _link = "<a href=\"{0}\">{1}</a>";
        private static readonly Regex _linkRegex = new Regex("\\b\r\n                (                       # Capture 1: entire matched URL\r\n                  (?:\r\n                    https?://               # http or https protocol\r\n                    |                       #   or\r\n                    www\\d{0,3}[.]           # \"www.\", \"www1.\", \"www2.\" … \"www999.\"\r\n                    |                           #   or\r\n                    [a-z0-9.\\-]+[.][a-z]{2,4}/  # looks like domain name followed by a slash\r\n                  )\r\n                  (?:                       # One or more:\r\n                    [^\\s()<>]+                  # Run of non-space, non-()<>\r\n                    |                           #   or\r\n                    \\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)  # balanced parens, up to 2 levels\r\n                  )+\r\n                  (?:                       # End with:\r\n                    \\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)  # balanced parens, up to 2 levels\r\n                    |                               #   or\r\n                    [^\\s`!()\\[\\]{};:'\".,<>?\x00ab\x00bb“”‘’]        # not a space or one of these punct chars\r\n                  )\r\n                )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const string _linkWithRel = "<a href=\"{0}\" rel=\"{1}\">{2}</a>";
        private const string _paraBreak = "\n\n";
        private static readonly Regex _properWordRegex = new Regex("([A-Z][a-z]*)|([0-9]+)");
        private static readonly string[] _salutations = new string[] { 
            "MR", "MRS", "MS", "MISS", "DR", "SIR", "MADAM", "SIR", "MONSIEUR", "MADEMOISELLE", "MADAME", "SIRE", "COL", "SENOR", "SR", "SENORA", 
            "SRA", "SENORITA", "SRTA", "HERR", "FRAU", "DHR", "HR", "FR", "SHRI", "SHRIMATI", "SIGNORE", "SIG", "SIGNORA", "SIG.RA", "PAN", "PANI", 
            "SENHOR", "SENHORA", "SENHORITA", "MENEER", "MEVROU", "MEJUFFROU"
         };
        private static readonly Regex _splitNameRegex = new Regex(@"[\W_]+");
        private static readonly Regex _whitespace = new Regex(@"\s");

        public static void AppendFormatLine(this StringBuilder sb, string format, params string[] args)
        {
            sb.AppendLine(string.Format(format, (object[]) args));
        }

        public static string AsNullIfEmpty(this string items)
        {
            if (!string.IsNullOrEmpty(items))
            {
                return items;
            }
            return null;
        }

        public static string AsNullIfWhiteSpace(this string items)
        {
            if (!items.IsNullOrWhiteSpace())
            {
                return items;
            }
            return null;
        }

        private static string BuildReplaceMultiple(ref string s, SortedDictionary<int, ReplaceKey> indexes, IDictionary<string, string> replaceMap)
        {
            StringBuilder builder = new StringBuilder();
            int startIndex = 0;
            foreach (KeyValuePair<int, ReplaceKey> pair in indexes)
            {
                builder.Append(s.Substring(startIndex, pair.Key - startIndex));
                builder.Append(replaceMap[pair.Value.Key]);
                startIndex = pair.Key + pair.Value.Length;
            }
            builder.Append(s.Substring(startIndex));
            return builder.ToString();
        }

        /// <summary>
        /// Returns true if s contains substring value.
        /// </summary>
        /// <param name="s">Initial value</param>
        /// <param name="value">Substring value</param>
        /// <returns>Boolean</returns>
        public static bool Contains(this string s, string value)
        {
            return (s.IndexOf(value) > -1);
        }

        /// <summary>
        /// Returns true if s contains substring value.
        /// </summary>
        /// <param name="s">Initial value</param>
        /// <param name="value">Substring value</param>
        /// <param name="comparison">StringComparison options.</param>
        /// <returns>Boolean</returns>
        public static bool Contains(this string s, string value, StringComparison comparison)
        {
            return (s.IndexOf(value, comparison) > -1);
        }

        /// <summary>
        /// Does string contain lowercase characters?
        /// </summary>
        /// <param name="s">The value.</param>
        /// <returns>True if contain lower case.</returns>
        public static bool ContainsLower(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return false;
            }
            return s.ToArray<char>().Any<char>(new Func<char, bool>(char.IsLower));
        }

        /// <summary>
        /// Indicates whether a string contains x occurrences of a string. 
        /// </summary>
        /// <param name="s">The string to search.</param>
        /// <param name="value">The string to search for.</param>
        /// <returns>
        /// <c>true</c> if the string contains at least two occurrences of {value}; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsMultiple(this string s, string value)
        {
            return s.ContainsMultiple(value, 2);
        }

        /// <summary>
        /// Indicates whether a string contains x occurrences of a string. 
        /// </summary>
        /// <param name="s">The string to search.</param>
        /// <param name="value">The string to search for.</param>
        /// <param name="count">The number of occurrences to search for.</param>
        /// <returns>
        /// <c>true</c> if the string contains at least {count} occurrences of {value}; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsMultiple(this string s, string value, int count)
        {
            if (count == 0)
            {
                return true;
            }
            int index = s.IndexOf(value);
            return ((index > -1) && s.Substring(index + 1).ContainsMultiple(value, --count));
        }

        /// <summary>
        /// Does string contain uppercase characters?
        /// </summary>
        /// <param name="s">The value.</param>
        /// <returns>True if contain upper case.</returns>
        public static bool ContainsUpper(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return false;
            }
            return s.ToArray<char>().Any<char>(new Func<char, bool>(char.IsUpper));
        }

        /// <summary>
        /// Encodes [[URL]] and [[Text][URL]] links to HTML.
        /// </summary>
        /// <param name="s">Text to encode</param>
        /// <param name="sb">StringBuilder to write results</param>
        /// <param name="rel">If specified, links will have the rel attribute set to this value
        /// attribute</param>
        private static void EncodeLinks(string s, StringBuilder sb, string rel)
        {
            int startIndex = 0;
            while (startIndex < s.Length)
            {
                int num2 = startIndex;
                startIndex = s.IndexOf("[[", startIndex);
                if (startIndex < 0)
                {
                    startIndex = s.Length;
                }
                sb.Append(s.Substring(num2, startIndex - num2));
                if (startIndex < s.Length)
                {
                    string str2;
                    num2 = startIndex + 2;
                    startIndex = s.IndexOf("]]", num2);
                    if (startIndex < 0)
                    {
                        startIndex = s.Length;
                    }
                    string str = s.Substring(num2, startIndex - num2);
                    int index = str.IndexOf("][");
                    if (index >= 0)
                    {
                        str2 = str.Substring(index + 2);
                        str = str.Substring(0, index);
                    }
                    else
                    {
                        str2 = str;
                    }
                    if (string.IsNullOrEmpty(rel))
                    {
                        sb.Append(string.Format("<a href=\"{0}\">{1}</a>", str2, str));
                    }
                    else
                    {
                        sb.Append(string.Format("<a href=\"{0}\" rel=\"{1}\">{2}</a>", str2, rel, str));
                    }
                    startIndex += 2;
                }
            }
        }

        /// <summary>
        /// Encodes a single paragraph to HTML.
        /// </summary>
        /// <param name="s">Text to encode</param>
        /// <param name="sb">StringBuilder to write results</param>
        /// <param name="rel">If specified, links will have the rel attribute set to this value
        /// attribute</param>
        private static void EncodeParagraph(string s, StringBuilder sb, string rel = null)
        {
            sb.AppendLine("<p>");
            s = HttpUtility.HtmlEncode(s);
            s = s.Replace("\n", "<br />\r\n");
            if (!string.IsNullOrEmpty(rel))
            {
                s = _linkRegex.Replace(s, string.Format("<a href=\"{0}\" rel=\"{1}\">{2}</a>", "$1", rel, "$1"));
            }
            else
            {
                s = _linkRegex.Replace(s, string.Format("<a href=\"{0}\">{1}</a>", "$1", "$1"));
            }
            EncodeLinks(s, sb, rel);
            sb.AppendLine("\r\n</p>");
        }

        private static void FindMultipleRegex(ref string s, string find, IDictionary<int, ReplaceKey> indexes)
        {
            MatchCollection matchs = new Regex(find, RegexOptions.IgnoreCase).Matches(s);
            for (int i = 0; i < matchs.Count; i++)
            {
                ReplaceKey key = new ReplaceKey {
                    Key = find,
                    Length = matchs[i].Length
                };
                indexes.Add(matchs[i].Index, key);
            }
        }

        private static void FindMultipleString(ref string s, string find, IDictionary<int, ReplaceKey> indexes)
        {
            int index;
            int startIndex = 0;
            do
            {
                index = s.IndexOf(find, startIndex);
                if (index >= 0)
                {
                    ReplaceKey key = new ReplaceKey {
                        Key = find,
                        Length = find.Length
                    };
                    indexes.Add(index, key);
                    startIndex = index + find.Length;
                }
            }
            while (index >= 0);
        }

        /// <summary>
        /// Applies a format to the item
        /// </summary>
        /// <param name="item">Item to format</param>
        /// <param name="format">Format string</param>
        /// <returns>Formatted string</returns>
        public static string FormatAs(this object item, string format)
        {
            format.Require<string>("format").NotNullOrEmpty();
            return string.Format(format, item);
        }

        /// <summary>
        /// Uses the string as a format.
        /// </summary>
        /// <param name="format">A String reference</param>
        /// <param name="source">Object that should be formatted</param>
        /// <returns>Formatted string</returns>
        public static string FormatName(this string format, object source)
        {
            format.Require<string>("format").NotNullOrEmpty();
            return NameFormatter.Format(format, source);
        }

        /// <summary>
        /// Applies a format to the item
        /// </summary>
        /// <param name="item">Item to format</param>
        /// <param name="format">Format string</param>
        /// <returns>Formatted string</returns>
        public static string FormatNameAs(this object item, string format)
        {
            format.Require<string>("format").NotNullOrEmpty();
            return NameFormatter.Format(format, item);
        }

        /// <summary>
        /// Uses the string as a format
        /// </summary>
        /// <param name="format">A String reference</param>
        /// <param name="args">Object parameters that should be formatted</param>
        /// <returns>Formatted string</returns>
        public static string FormatWith(this string format, params object[] args)
        {
            format.Require<string>("format").NotNullOrEmpty();
            return string.Format(format, args);
        }

        public static string GetDomainOfUrl(string url)
        {
            int index = url.IndexOf("://");
            index = (index == -1) ? 0 : (index + 3);
            int num2 = url.IndexOf('/', index);
            if (num2 != -1)
            {
                return url.Substring(0, num2 + 1);
            }
            return url;
        }

        /// <summary>
        /// Calculates a hashcode for the string that is guaranteed to be stable across .NET versions.
        /// </summary>
        /// <param name="value">The string to hash.</param>
        /// <returns>The hashcode</returns>
        public static int GetStableHashCode(this string value)
        {
            int num = 0;
            int num2 = 0;
            while (num2 < (value.Length - 1))
            {
                num = ((num << 5) - num) + value[num2];
                num = ((num << 5) - num) + value[num2 + 1];
                num2 += 2;
            }
            if (num2 < value.Length)
            {
                num = ((num << 5) - num) + value[num2];
            }
            return num;
        }

        /// <summary>
        /// Is the string all lower case characters?
        /// </summary>
        /// <param name="s">The value.</param>
        /// <returns>True if all lower case.</returns>
        public static bool IsAllLowerCase(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return false;
            }
            return !s.ContainsUpper();
        }

        /// <summary>
        /// Is the string all upper case characters?
        /// </summary>
        /// <param name="s">The value.</param>
        /// <returns>True if all upper case.</returns>
        public static bool IsAllUpperCase(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return false;
            }
            return !s.ContainsLower();
        }

        /// <summary>
        /// Determines if the string looks like JSON content.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsJson(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            if (!value.StartsWith("{"))
            {
                return value.StartsWith("[");
            }
            return true;
        }

        /// <summary>
        /// Do any of the strings contain both uppercase and lowercase characters?
        /// </summary>
        /// <param name="values">String values.</param>
        /// <returns>True if any contain mixed cases.</returns>
        public static bool IsMixedCase(this IEnumerable<string> values)
        {
            foreach (string str in values)
            {
                if (str.IsMixedCase())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Does string contain both uppercase and lowercase characters?
        /// </summary>
        /// <param name="s">The value.</param>
        /// <returns>True if contain mixed case.</returns>
        public static bool IsMixedCase(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return false;
            }
            bool flag = false;
            bool flag2 = false;
            foreach (char ch in s)
            {
                if (char.IsUpper(ch))
                {
                    flag = true;
                }
                if (char.IsLower(ch))
                {
                    flag2 = true;
                }
            }
            return (flag2 && flag);
        }

        /// <summary>
        /// Indicates whether the specified String object is null or an empty string
        /// </summary>
        /// <param name="item">A String reference</param>
        /// <returns>
        /// <c>true</c> if is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this string item)
        {
            return string.IsNullOrEmpty(item);
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters
        /// </summary>
        /// <param name="item">A String reference</param>
        /// <returns>
        /// <c>true</c> if is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrWhiteSpace(this string item)
        {
            if (item != null)
            {
                return item.All<char>(new Func<char, bool>(char.IsWhiteSpace));
            }
            return true;
        }

        public static bool IsSalutation(this string value)
        {
            value = value.ToASCII();
            value = value.Trim();
            value = value.TrimEnd(new char[] { '.' });
            return _salutations.Any<string>(s => (s.ToUpper() == value));
        }

        /// <summary>
        /// Checks to see if a string is a valid .NET identifier string.
        /// </summary>
        /// <param name="value">String identifier to check.</param>
        /// <returns>Returns true if value is a valid identifier</returns>
        public static bool IsValidIdentifier(this string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }
            if (_identifierRegex.IsMatch(value))
            {
                return false;
            }
            return (char.IsLetter(value[0]) || ((value[0] == '_') && (value.Length > 1)));
        }

        /// <summary>
        /// Checks to see if a string is a valid .NET namespace.
        /// </summary>
        /// <param name="value">String identifier to check.</param>
        /// <returns>Returns true if value is a valid namespace.</returns>
        public static bool IsValidNamespace(this string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }
            return !value.Split(new char[] { '.' }).Any<string>(v => !v.IsValidIdentifier());
        }

        public static string NormalizeLineEndings(this string text, string lineEnding = null)
        {
            if (string.IsNullOrEmpty(lineEnding))
            {
                lineEnding = Environment.NewLine;
            }
            text = text.Replace("\r\n", "\n");
            if (lineEnding != "\n")
            {
                text = text.Replace("\r\n", lineEnding);
            }
            return text;
        }

        public static Dictionary<string, string> ParseConfigValues(this string value)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string str in value.Split(new char[] { ';' }))
            {
                string[] strArray2 = str.Split(new char[] { '=' });
                if (strArray2.Length == 2)
                {
                    dictionary.Add(strArray2[0].Trim(), strArray2[1].Trim());
                }
                else
                {
                    dictionary.Add(str.Trim(), null);
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Parses a person's full name from a single string.
        /// </summary>
        /// <param name="fullName">The person's full name.</param>
        public static PersonNameInfo ParsePersonName(this string fullName)
        {
            PersonNameInfo info = new PersonNameInfo();
            string[] separator = new string[] { " " };
            string[] source = fullName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            switch (source.Length)
            {
                case 1:
                    info.FirstName = source[0].ToTitleCase();
                    return info;

                case 2:
                    if (!source[0].IsSalutation())
                    {
                        if (source[0].EndsWith(","))
                        {
                            info.LastName = source[0].TrimEnd(new char[] { ',' }).ToTitleCase();
                            info.FirstName = source[1].ToTitleCase();
                            return info;
                        }
                        info.FirstName = source[0].ToTitleCase();
                        info.LastName = source[1].ToTitleCase();
                        return info;
                    }
                    info.Salutation = source[0].ToTitleCase();
                    info.LastName = source[1].ToTitleCase();
                    return info;

                case 3:
                    if (!source[0].IsSalutation())
                    {
                        if (source[0].EndsWith(","))
                        {
                            info.LastName = source[0].TrimEnd(new char[] { ',' }).ToTitleCase();
                            info.FirstName = source[1].ToTitleCase();
                            info.MiddleName = source[2].ToTitleCase();
                            return info;
                        }
                        info.FirstName = source[0].ToTitleCase();
                        info.MiddleName = source[1].ToTitleCase();
                        info.LastName = source[2].ToTitleCase();
                        return info;
                    }
                    info.Salutation = source[0].ToTitleCase();
                    info.FirstName = source[1].ToTitleCase();
                    info.LastName = source[2].ToTitleCase();
                    return info;

                case 4:
                    if (!source[0].IsSalutation())
                    {
                        info.FirstName = source[0].ToTitleCase();
                        info.MiddleName = source[1].ToTitleCase();
                        info.LastName = source[2].ToTitleCase();
                        info.Suffix = source[3].ToTitleCase();
                        return info;
                    }
                    info.Salutation = source[0].ToTitleCase();
                    info.FirstName = source[1].ToTitleCase();
                    info.MiddleName = source[2].ToTitleCase();
                    info.LastName = source[3].ToTitleCase();
                    return info;

                case 0:
                    throw new ArgumentException("Full name parameter must contain at least one part.");
            }
            if (source[0].EndsWith(".") && source[0].IsSalutation())
            {
                info.Salutation = source[0].ToTitleCase();
                info.FirstName = source[1].ToTitleCase();
                info.MiddleName = source[2].ToTitleCase();
                info.LastName = source[3].ToTitleCase();
                info.Suffix = string.Join(" ", (from p in source.AsIndexedEnumerable<string>()
                    where p.Index > 3
                    select p.Value).ToArray<string>());
                return info;
            }
            info.FirstName = source[0].ToTitleCase();
            info.MiddleName = source[1].ToTitleCase();
            info.LastName = source[2].ToTitleCase();
            info.Suffix = string.Join(" ", (from p in source.AsIndexedEnumerable<string>()
                where p.Index > 2
                select p.Value).ToArray<string>());
            return info;
        }

        /// <summary>
        /// Removes all whitespace from a string.
        /// </summary>
        /// <param name="s">Initial string.</param>
        /// <returns>String with no whitespace.</returns>
        public static string RemoveWhiteSpace(this string s)
        {
            return _whitespace.Replace(s, string.Empty);
        }

        public static string ReplaceFirst(this string s, string find, string replace)
        {
            int index = s.IndexOf(find);
            if (index >= 0)
            {
                string str = s.Substring(0, index);
                string str2 = s.Substring(index + find.Length);
                return (str + replace + str2);
            }
            return s;
        }

        public static string ReplaceMultiple(this string s, IDictionary<string, string> replaceMap)
        {
            return s.ReplaceMultiple(replaceMap, false);
        }

        public static string ReplaceMultiple(this string s, IDictionary<string, string> replaceMap, bool isRegexFind)
        {
            SortedDictionary<int, ReplaceKey> indexes = new SortedDictionary<int, ReplaceKey>();
            foreach (KeyValuePair<string, string> pair in replaceMap)
            {
                if (isRegexFind)
                {
                    FindMultipleRegex(ref s, pair.Key, indexes);
                }
                else
                {
                    FindMultipleString(ref s, pair.Key, indexes);
                }
            }
            if (indexes.Count <= 0)
            {
                return s;
            }
            return BuildReplaceMultiple(ref s, indexes, replaceMap);
        }

        /// <summary>
        /// Replicates the given string.
        /// </summary>
        /// <param name="value">Text to replicate</param>
        /// <param name="count">Times to replicate</param>
        /// <returns>The replicated string</returns>
        public static string Replicate(this string value, int count)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                builder.Append(value);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Formats a string without throwing a FormatException.
        /// </summary>
        /// <param name="format">A String reference</param>
        /// <param name="args">Object parameters that should be formatted</param>
        /// <returns>Formatted string if no error is thrown, else reutrns the format parameter.</returns>
        public static string SafeFormat(this string format, params object[] args)
        {
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        public static string[] SplitAndTrim(this string s, params char[] separator)
        {
            if (s.IsNullOrEmpty())
            {
                return new string[0];
            }
            string[] strArray = s.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strArray.Length; i++)
            {
                strArray[i] = strArray[i].Trim();
            }
            return strArray;
        }

        public static string[] SplitAndTrim(this string s, params string[] separator)
        {
            if (s.IsNullOrEmpty())
            {
                return new string[0];
            }
            string[] strArray = ((separator == null) || (separator.Length == 0)) ? s.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries) : s.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strArray.Length; i++)
            {
                strArray[i] = strArray[i].Trim();
            }
            return strArray;
        }

        /// <summary>
        /// Strips NewLines and Tabs
        /// </summary>
        /// <param name="s">The string to strip.</param>
        /// <returns>Stripped string.</returns>
        public static string StripInvisible(this string s)
        {
            return s.Replace("\r\n", " ").Replace('\n', ' ').Replace('\t', ' ');
        }

        /// <summary>
        /// Convert UTF8 string to ASCII.
        /// </summary>
        /// <param name="s">The UTF8 string.</param>
        /// <returns>The ASCII string.</returns>
        public static string ToASCII(this string s)
        {
            Encoding dstEncoding = Encoding.GetEncoding(Encoding.ASCII.EncodingName, new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback());
            byte[] bytes = Encoding.Convert(Encoding.UTF8, dstEncoding, Encoding.UTF8.GetBytes(s));
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Converts a string to use camelCase.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The to camel case. </returns>
        public static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            string str = value.ToPascalCase();
            if (str.Length > 2)
            {
                return (char.ToLower(str[0]) + str.Substring(1));
            }
            return str.ToLower();
        }

        public static string ToCommandLineArgument(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            if (!path.Contains(" "))
            {
                return path;
            }
            return string.Format("\"{0}\"", path);
        }

        /// <summary>
        /// Converts a string to a valid C# identifier string.
        /// </summary>
        /// <param name="value">Text to convert.</param>
        /// <returns>The valid identifier</returns>
        public static string ToCSharpIdentifier(this string value)
        {
            string str = _identifierRegex.Replace(value, string.Empty);
            return new CSharpCodeProvider().CreateEscapedIdentifier(str);
        }

        /// <summary>
        /// Converts a string to an C# escaped literal string.
        /// </summary>
        /// <param name="value">Text to escape</param>
        /// <returns>The escaped string</returns>
        public static string ToCSharpLiteral(this string value)
        {
            StringWriter writer = new StringWriter();
            new CSharpCodeProvider().GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, null);
            return writer.GetStringBuilder().ToString();
        }

        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <param name="values">The IEnumerable string values to convert.</param>
        /// <returns>A delimited string of the values.</returns>
        public static string ToDelimitedString(this IEnumerable<string> values)
        {
            return values.ToDelimitedString(",");
        }

        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <param name="values">The IEnumerable string values to convert.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>A delimited string of the values.</returns>
        public static string ToDelimitedString(this IEnumerable<string> values, string delimiter)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in values)
            {
                if (builder.Length > 0)
                {
                    builder.Append(delimiter);
                }
                builder.Append(str);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <typeparam name="T">
        /// The type of objects to delimit.
        /// </typeparam>
        /// <param name="values">
        /// The IEnumerable string values to convert.
        /// </param>
        /// <param name="delimiter">
        /// The delimiter.
        /// </param>
        /// <returns>
        /// A delimited string of the values.
        /// </returns>
        public static string ToDelimitedString<T>(this IEnumerable<T> values, string delimiter)
        {
            StringBuilder builder = new StringBuilder();
            foreach (T local in values)
            {
                if (builder.Length > 0)
                {
                    builder.Append(delimiter);
                }
                builder.Append(local.ToString());
            }
            return builder.ToString();
        }

        public static string ToFileExtension(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            string str = Path.GetExtension(path) ?? string.Empty;
            if (str.Length <= 0)
            {
                return string.Empty;
            }
            return str.ToLowerInvariant();
        }

        /// <summary>
        /// Returns a copy of this string converted to HTML markup.
        /// </summary>
        public static string ToHtml(this string s)
        {
            return s.ToHtml(null);
        }

        /// <summary>
        /// Returns a copy of this string converted to HTML markup.
        /// </summary>
        /// <param name="rel">If specified, links will have the rel attribute set to this value
        /// attribute</param>
        public static string ToHtml(this string s, string rel)
        {
            s = s.NormalizeLineEndings("\n");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i += "\n\n".Length)
            {
                int startIndex = i;
                i = s.IndexOf("\n\n", startIndex);
                if (i < 0)
                {
                    i = s.Length;
                }
                string str = s.Substring(startIndex, i - startIndex).Trim();
                if (str.Length > 0)
                {
                    EncodeParagraph(str, sb, rel);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a string to a valid .NET identifier string.
        /// </summary>
        /// <param name="value">Text to convert.</param>
        /// <returns>The valid identifier</returns>
        public static string ToIdentifier(this string value)
        {
            string str = _identifierRegex.Replace(value, string.Empty);
            if (str.StartsWith("__"))
            {
                str = "_" + str;
            }
            return str;
        }

        /// <summary>
        /// Converts a string to use PascalCase.
        /// </summary>
        /// <param name="value">Text to convert</param>
        /// <returns>The string</returns>
        public static string ToPascalCase(this string value)
        {
            return value.ToPascalCase(_splitNameRegex);
        }

        /// <summary>
        /// Converts a string to use PascalCase.
        /// </summary>
        /// <param name="value">Text to convert</param>
        /// <param name="splitRegex">Regular Expression to split words on.</param>
        /// <returns>The string</returns>
        public static string ToPascalCase(this string value, Regex splitRegex)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            bool flag = value.IsMixedCase();
            string[] strArray = splitRegex.Split(value);
            StringBuilder builder = new StringBuilder();
            if (strArray.Length > 1)
            {
                foreach (string str in strArray)
                {
                    if (str.Length > 1)
                    {
                        builder.Append(char.ToUpper(str[0]));
                        builder.Append(flag ? str.Substring(1) : str.Substring(1).ToLower());
                    }
                    else
                    {
                        builder.Append(str);
                    }
                }
            }
            else if (value.Length > 1)
            {
                builder.Append(char.ToUpper(value[0]));
                builder.Append(flag ? value.Substring(1) : value.Substring(1).ToLower());
            }
            else
            {
                builder.Append(value.ToUpper());
            }
            return builder.ToString();
        }

        /// <summary>
        /// Takes a NameIdentifier and spaces it out into words "Name Identifier".
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string</returns>
        public static string ToSpacedWords(this string value)
        {
            string[] strArray = value.ToWords();
            StringBuilder builder = new StringBuilder();
            foreach (string str in strArray)
            {
                builder.Append(str);
                builder.Append(' ');
            }
            return builder.ToString().Trim();
        }

        /// <summary>
        /// Converts a string to use PascalCase.
        /// </summary>
        /// <param name="value">Text to convert</param>
        /// <returns>The string</returns>
        public static string ToTitleCase(this string value)
        {
            if (value.IsMixedCase())
            {
                return value;
            }
            return (char.ToUpper(value[0]) + value.Substring(1).ToLower());
        }

        /// <summary>
        /// Converts a string to a valid C# identifier string.
        /// </summary>
        /// <param name="value">Text to convert.</param>
        /// <returns>The valid identifier</returns>
        public static string ToVbIdentifier(this string value)
        {
            string str = _identifierRegex.Replace(value, string.Empty);
            return new VBCodeProvider().CreateEscapedIdentifier(str);
        }

        /// <summary>
        /// Converts a string to an VB escaped literal string.
        /// </summary>
        /// <param name="value">Text to escape</param>
        /// <returns>The escaped string</returns>
        public static string ToVbLiteral(this string value)
        {
            StringWriter writer = new StringWriter();
            new VBCodeProvider().GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, null);
            return writer.GetStringBuilder().ToString();
        }

        /// <summary>
        /// Takes a NameIdentifier and spaces it out into words "Name Identifier".
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string</returns>
        public static string[] ToWords(this string value)
        {
            List<string> list = new List<string>();
            value = value.ToPascalCase();
            foreach (Match match in _properWordRegex.Matches(value))
            {
                if (!match.Value.IsNullOrWhiteSpace())
                {
                    list.Add(match.Value);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Truncates the specified text.
        /// </summary>
        /// <param name="text">The text to truncate.</param>
        /// <param name="keep">The number of characters to keep.</param>
        /// <returns>A truncate string.</returns>
        public static string Truncate(this string text, int keep)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            string str = text.NormalizeLineEndings(null);
            if (str.Length <= keep)
            {
                return str;
            }
            return (str.Substring(0, keep - 3) + "...");
        }

        public static string Truncate(this string text, int length, string ellipsis, bool keepFullWordAtEnd)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            if (text.Length < length)
            {
                return text;
            }
            text = text.Substring(0, length);
            if (keepFullWordAtEnd && (text.LastIndexOf(' ') > 0))
            {
                text = text.Substring(0, text.LastIndexOf(' '));
            }
            return string.Format("{0}{1}", text, ellipsis);
        }

        public static string TruncatePath(this string path, int length)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            if (path.Length <= length)
            {
                return path;
            }
            int num = "...".Length;
            if (length <= num)
            {
                return "...";
            }
            bool flag = true;
            string str = "";
            string str2 = "";
            int index = 0;
            int num3 = 0;
            string[] strArray = path.Split(new char[] { Path.DirectorySeparatorChar });
            for (int i = 0; i < strArray.Length; i++)
            {
                if (flag)
                {
                    string str3 = string.Format("{0}{1}", strArray[index], Path.DirectorySeparatorChar);
                    if ((((str.Length + str2.Length) + str3.Length) + num) > length)
                    {
                        break;
                    }
                    str = str + str3;
                    if (str3 != Path.DirectorySeparatorChar.ToString())
                    {
                        flag = false;
                    }
                    index++;
                }
                else
                {
                    int num5 = (strArray.Length - num3) - 1;
                    string str4 = string.Format("{0}{1}", Path.DirectorySeparatorChar, strArray[num5]);
                    if ((((str.Length + str2.Length) + str4.Length) + num) > length)
                    {
                        break;
                    }
                    str2 = str4 + str2;
                    if (str4 != Path.DirectorySeparatorChar.ToString())
                    {
                        flag = true;
                    }
                    num3++;
                }
            }
            if (str2 == string.Empty)
            {
                str2 = strArray[strArray.Length - 1];
                str2 = str2.Substring(((str2.Length + num) + str.Length) - length, (length - num) - str.Length);
            }
            return string.Format("{0}{1}{2}", str, "...", str2);
        }

        private class ReplaceKey
        {
            public string Key { get; set; }

            public int Length { get; set; }
        }
    }
}

