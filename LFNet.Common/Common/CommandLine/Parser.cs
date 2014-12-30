using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace LFNet.Common.CommandLine
{
    /// <summary>
    /// Parser for command line arguments.
    /// 
    /// The parser specification is inferred from the instance fields of the object
    /// specified as the destination of the parse.
    /// Valid argument types are: int, uint, string, bool, enums
    /// Also argument types of Array of the above types are also valid.
    /// 
    /// Error checking options can be controlled by adding a ArgumentAttribute
    /// to the instance fields of the destination object.
    /// 
    /// At most one field may be marked with the DefaultArgumentAttribute
    /// indicating that arguments without a '-' or '/' prefix will be parsed as that argument.
    /// 
    /// If not specified then the parser will infer default options for parsing each
    /// instance field. The default long name of the argument is the field name. The
    /// default short name is the first character of the long name. Long names and explicitly
    /// specified short names must be unique. Default short names will be used provided that
    /// the default short name does not conflict with a long name or an explicitly
    /// specified short name.
    /// 
    /// Arguments which are array types are collection arguments. Collection
    /// arguments can be specified multiple times.
    /// </summary>
    public sealed class Parser
    {
        private Hashtable argumentMap;
        private ArrayList arguments;
        private Argument defaultArgument;
        /// <summary>
        /// The System Defined new line string.
        /// </summary>
        public const string NewLine = "\r\n";
        private ErrorReporter reporter;
        private const int spaceBeforeParam = 2;

        /// <summary>
        /// Don't ever call this.
        /// </summary>
        private Parser()
        {
        }

        /// <summary>
        /// Creates a new command line argument parser.
        /// </summary>
        /// <param name="argumentSpecification"> The type of object to  parse. </param>
        /// <param name="reporter"> The destination for parse errors. </param>
        public Parser(Type argumentSpecification, ErrorReporter reporter)
        {
            this.reporter = reporter;
            this.arguments = new ArrayList();
            this.argumentMap = new Hashtable();
            foreach (FieldInfo info in argumentSpecification.GetFields())
            {
                if ((!info.IsStatic && !info.IsInitOnly) && !info.IsLiteral)
                {
                    ArgumentAttribute attribute = GetAttribute(info);
                    if (attribute is DefaultArgumentAttribute)
                    {
                        this.defaultArgument = new Argument(attribute, info, reporter);
                    }
                    else
                    {
                        this.arguments.Add(new Argument(attribute, info, reporter));
                    }
                }
            }
            foreach (Argument argument in this.arguments)
            {
                this.argumentMap[argument.LongName] = argument;
                if (argument.ExplicitShortName)
                {
                    if ((argument.ShortName != null) && (argument.ShortName.Length > 0))
                    {
                        this.argumentMap[argument.ShortName] = argument;
                    }
                    else
                    {
                        argument.ClearShortName();
                    }
                }
            }
            foreach (Argument argument2 in this.arguments)
            {
                if (!argument2.ExplicitShortName)
                {
                    if (((argument2.ShortName != null) && (argument2.ShortName.Length > 0)) && !this.argumentMap.ContainsKey(argument2.ShortName))
                    {
                        this.argumentMap[argument2.ShortName] = argument2;
                    }
                    else
                    {
                        argument2.ClearShortName();
                    }
                }
            }
        }

        private static void AddNewLine(string newLine, StringBuilder builder, ref int currentColumn)
        {
            builder.Append(newLine);
            currentColumn = 0;
        }

        /// <summary>
        /// Returns a Usage string for command line argument parsing.
        /// Use ArgumentAttributes to control parsing behavior.
        /// Formats the output to the width of the current console window.
        /// </summary>
        /// <param name="argumentType"> The type of the arguments to display usage for. </param>
        /// <returns> Printable string containing a user friendly description of command line arguments. </returns>
        public static string ArgumentsUsage(Type argumentType)
        {
            int windowWidth = Console.WindowWidth;
            if (windowWidth == 0)
            {
                windowWidth = 80;
            }
            return ArgumentsUsage(argumentType, windowWidth);
        }

        /// <summary>
        /// Returns a Usage string for command line argument parsing.
        /// Use ArgumentAttributes to control parsing behavior.
        /// </summary>
        /// <param name="argumentType"> The type of the arguments to display usage for. </param>
        /// <param name="columns"> The number of columns to format the output to. </param>
        /// <returns> Printable string containing a user friendly description of command line arguments. </returns>
        public static string ArgumentsUsage(Type argumentType, int columns)
        {
            return new Parser(argumentType, null).GetUsageString(columns);
        }

        private static object DefaultValue(ArgumentAttribute attribute, FieldInfo field)
        {
            if ((attribute != null) && attribute.HasDefaultValue)
            {
                return attribute.DefaultValue;
            }
            return null;
        }

        private static Type ElementType(FieldInfo field)
        {
            if (IsCollectionType(field.FieldType))
            {
                return field.FieldType.GetElementType();
            }
            return null;
        }

        private static bool ExplicitShortName(ArgumentAttribute attribute)
        {
            return ((attribute != null) && !attribute.DefaultShortName);
        }

        private static ArgumentType Flags(ArgumentAttribute attribute, FieldInfo field)
        {
            if (attribute != null)
            {
                return attribute.Type;
            }
            if (IsCollectionType(field.FieldType))
            {
                return ArgumentType.MultipleUnique;
            }
            return ArgumentType.AtMostOnce;
        }

        private ArgumentHelpStrings[] GetAllHelpStrings()
        {
            ArgumentHelpStrings[] stringsArray = new ArgumentHelpStrings[this.NumberOfParametersToDisplay() - 1];
            int index = 0;
            if (this.defaultArgument != null)
            {
                stringsArray[index++] = GetHelpStrings(this.defaultArgument);
            }
            foreach (Argument argument in this.arguments)
            {
                stringsArray[index] = GetHelpStrings(argument);
                index++;
            }
            return stringsArray;
        }

        private static ArgumentAttribute GetAttribute(FieldInfo field)
        {
            object[] customAttributes = field.GetCustomAttributes(typeof(ArgumentAttribute), false);
            if (customAttributes.Length == 1)
            {
                return (ArgumentAttribute) customAttributes[0];
            }
            return null;
        }

        private static ArgumentHelpStrings GetHelpStrings(Argument arg)
        {
            return new ArgumentHelpStrings(arg.SyntaxHelp, arg.FullHelpText);
        }

        /// <summary>
        /// A user firendly usage string describing the command line argument syntax.
        /// </summary>
        public string GetUsageString(int screenWidth)
        {
            int num2;
            ArgumentHelpStrings[] allHelpStrings = this.GetAllHelpStrings();
            int num = 0;
            foreach (ArgumentHelpStrings strings in allHelpStrings)
            {
                num = Math.Max(num, strings.syntax.Length);
            }
            int num3 = num + 2;
            screenWidth = Math.Max(screenWidth, 15);
            if (screenWidth < (num3 + 10))
            {
                num2 = 5;
            }
            else
            {
                num2 = num3;
            }
            StringBuilder builder = new StringBuilder();
            foreach (ArgumentHelpStrings strings2 in allHelpStrings)
            {
                int length = strings2.syntax.Length;
                builder.Append(strings2.syntax);
                int currentColumn = length;
                if (length >= num2)
                {
                    builder.Append("\n");
                    currentColumn = 0;
                }
                int num6 = screenWidth - num2;
                int startIndex = 0;
                while (startIndex < strings2.help.Length)
                {
                    builder.Append(' ', num2 - currentColumn);
                    currentColumn = num2;
                    int num8 = startIndex + num6;
                    if (num8 >= strings2.help.Length)
                    {
                        num8 = strings2.help.Length;
                    }
                    else
                    {
                        num8 = strings2.help.LastIndexOf(' ', num8 - 1, Math.Min(num8 - startIndex, num6));
                        if (num8 <= startIndex)
                        {
                            num8 = startIndex + num6;
                        }
                    }
                    builder.Append(strings2.help, startIndex, num8 - startIndex);
                    startIndex = num8;
                    AddNewLine("\n", builder, ref currentColumn);
                    while ((startIndex < strings2.help.Length) && (strings2.help[startIndex] == ' '))
                    {
                        startIndex++;
                    }
                }
                if (strings2.help.Length == 0)
                {
                    builder.Append("\n");
                }
            }
            return builder.ToString();
        }

        private static bool HasHelpText(ArgumentAttribute attribute)
        {
            return ((attribute != null) && attribute.HasHelpText);
        }

        private static string HelpText(ArgumentAttribute attribute, FieldInfo field)
        {
            if (attribute == null)
            {
                return null;
            }
            return attribute.HelpText;
        }

        /// <summary>
        /// Searches a StringBuilder for a character
        /// </summary>
        /// <param name="text"> The text to search. </param>
        /// <param name="value"> The character value to search for. </param>
        /// <param name="startIndex"> The index to stat searching at. </param>
        /// <returns> The index of the first occurrence of value or -1 if it is not found. </returns>
        public static int IndexOf(StringBuilder text, char value, int startIndex)
        {
            for (int i = startIndex; i < text.Length; i++)
            {
                if (text[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool IsCollectionType(Type type)
        {
            return type.IsArray;
        }

        private static bool IsValidElementType(Type type)
        {
            if (type == null)
            {
                return false;
            }
            if ((!(type == typeof(int)) && !(type == typeof(uint))) && (!(type == typeof(string)) && !(type == typeof(bool))))
            {
                return type.IsEnum;
            }
            return true;
        }

        /// <summary>
        /// Searches a StringBuilder for a character in reverse
        /// </summary>
        /// <param name="text"> The text to search. </param>
        /// <param name="value"> The character to search for. </param>
        /// <param name="startIndex"> The index to start the search at. </param>
        /// <returns>The index of the last occurrence of value in text or -1 if it is not found. </returns>
        public static int LastIndexOf(StringBuilder text, char value, int startIndex)
        {
            for (int i = Math.Min(startIndex, text.Length - 1); i >= 0; i--)
            {
                if (text[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool LexFileArguments(string fileName, out string[] arguments)
        {
            string str = null;
            try
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    str = new StreamReader(stream).ReadToEnd();
                }
            }
            catch (Exception exception)
            {
                this.reporter(string.Format("Error: Can't open command line argument file '{0}' : '{1}'", fileName, exception.Message));
                arguments = null;
                return false;
            }
            bool flag = false;
            ArrayList list = new ArrayList();
            StringBuilder builder = new StringBuilder();
            bool flag2 = false;
            int num = 0;
            try
            {
                while (true)
                {
                    while (char.IsWhiteSpace(str[num]))
                    {
                        num++;
                    }
                    if (str[num] == '#')
                    {
                        num++;
                        while (str[num] != '\n')
                        {
                            num++;
                        }
                    }
                    else
                    {
                        do
                        {
                            if (str[num] == '\\')
                            {
                                int repeatCount = 1;
                                num++;
                                while ((num == str.Length) && (str[num] == '\\'))
                                {
                                    repeatCount++;
                                }
                                if ((num == str.Length) || (str[num] != '"'))
                                {
                                    builder.Append('\\', repeatCount);
                                }
                                else
                                {
                                    builder.Append('\\', repeatCount >> 1);
                                    if ((repeatCount & 1) != 0)
                                    {
                                        builder.Append('"');
                                    }
                                    else
                                    {
                                        flag2 = !flag2;
                                    }
                                }
                            }
                            else if (str[num] == '"')
                            {
                                flag2 = !flag2;
                                num++;
                            }
                            else
                            {
                                builder.Append(str[num]);
                                num++;
                            }
                        }
                        while (!char.IsWhiteSpace(str[num]) || flag2);
                        list.Add(builder.ToString());
                        builder.Length = 0;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                if (flag2)
                {
                    this.reporter(string.Format("Error: Unbalanced '\"' in command line argument file '{0}'", fileName));
                    flag = true;
                }
                else if (builder.Length > 0)
                {
                    list.Add(builder.ToString());
                }
            }
            arguments = (string[]) list.ToArray(typeof(string));
            return flag;
        }

        private static string LongName(ArgumentAttribute attribute, FieldInfo field)
        {
            if ((attribute != null) && !attribute.DefaultLongName)
            {
                return attribute.LongName;
            }
            return field.Name;
        }

        private static void NullErrorReporter(string message)
        {
        }

        private int NumberOfParametersToDisplay()
        {
            int num = this.arguments.Count + 1;
            if (this.HasDefaultArgument)
            {
                num++;
            }
            return num;
        }

        /// <summary>
        /// Parses an argument list.
        /// </summary>
        /// <param name="args"> The arguments to parse. </param>
        /// <param name="destination"> The destination of the parsed arguments. </param>
        /// <returns> true if no parse errors were encountered. </returns>
        public bool Parse(string[] args, object destination)
        {
            bool flag = this.ParseArgumentList(args, destination);
            foreach (Argument argument in this.arguments)
            {
                flag |= argument.Finish(destination);
            }
            if (this.defaultArgument != null)
            {
                flag |= this.defaultArgument.Finish(destination);
            }
            return !flag;
        }

        /// <summary>
        /// Parses an argument list into an object
        /// </summary>
        /// <param name="args"></param>
        /// <param name="destination"></param>
        /// <returns> true if an error occurred </returns>
        private bool ParseArgumentList(string[] args, object destination)
        {
            bool flag = false;
            if (args != null)
            {
                foreach (string str in args)
                {
                    string str2;
                    string str3;
                    Argument argument;
                    if (str.Length <= 0)
                    {
                        continue;
                    }
                    switch (str[0])
                    {
                        case '-':
                        case '/':
                        {
                            int num = str.IndexOfAny(new char[] { ':', '+', '-' }, 1);
                            str2 = str.Substring(1, (num == -1) ? (str.Length - 1) : (num - 1));
                            if ((str2.Length + 1) != str.Length)
                            {
                                break;
                            }
                            str3 = null;
                            goto Label_00DD;
                        }
                        case '@':
                        {
                            string[] strArray;
                            flag |= this.LexFileArguments(str.Substring(1), out strArray);
                            flag |= this.ParseArgumentList(strArray, destination);
                            continue;
                        }
                        default:
                            goto Label_0131;
                    }
                    if ((str.Length > (1 + str2.Length)) && (str[1 + str2.Length] == ':'))
                    {
                        str3 = str.Substring(str2.Length + 2);
                    }
                    else
                    {
                        str3 = str.Substring(str2.Length + 1);
                    }
                Label_00DD:
                    argument = (Argument) this.argumentMap[str2];
                    if (argument == null)
                    {
                        this.ReportUnrecognizedArgument(str);
                        flag = true;
                    }
                    else
                    {
                        flag |= !argument.SetValue(str3, destination);
                    }
                    continue;
                Label_0131:
                    if (this.defaultArgument != null)
                    {
                        flag |= !this.defaultArgument.SetValue(str, destination);
                    }
                    else
                    {
                        this.ReportUnrecognizedArgument(str);
                        flag = true;
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// Parses Command Line Arguments. 
        /// Errors are output on Console.Error.
        /// Use ArgumentAttributes to control parsing behavior.
        /// </summary>
        /// <param name="arguments"> The actual arguments. </param>
        /// <param name="destination"> The resulting parsed arguments. </param>
        /// <returns> true if no errors were detected. </returns>
        public static bool ParseArguments(string[] arguments, object destination)
        {
            TextWriter error = Console.Error;
            return ParseArguments(arguments, destination, new ErrorReporter(error.WriteLine));
        }

        /// <summary>
        /// Parses Command Line Arguments. 
        /// Use ArgumentAttributes to control parsing behavior.
        /// </summary>
        /// <param name="arguments"> The actual arguments. </param>
        /// <param name="destination"> The resulting parsed arguments. </param>
        /// <param name="reporter"> The destination for parse errors. </param>
        /// <returns> true if no errors were detected. </returns>
        public static bool ParseArguments(string[] arguments, object destination, ErrorReporter reporter)
        {
            Parser parser = new Parser(destination.GetType(), reporter);
            return parser.Parse(arguments, destination);
        }

        /// <summary>
        /// Parses Command Line Arguments. Displays usage message to Console.Out
        /// if /?, /help or invalid arguments are encountered.
        /// Errors are output on Console.Error.
        /// Use ArgumentAttributes to control parsing behavior.
        /// </summary>
        /// <param name="arguments"> The actual arguments. </param>
        /// <param name="destination"> The resulting parsed arguments. </param>
        /// <returns> true if no errors were detected. </returns>
        public static bool ParseArgumentsWithUsage(string[] arguments, object destination)
        {
            if (!ParseHelp(arguments) && ParseArguments(arguments, destination))
            {
                return true;
            }
            Console.Write(ArgumentsUsage(destination.GetType()));
            return false;
        }

        /// <summary>
        /// Checks if a set of arguments asks for help.
        /// </summary>
        /// <param name="args"> Args to check for help. </param>
        /// <returns> Returns true if args contains /? or /help. </returns>
        public static bool ParseHelp(string[] args)
        {
            Parser parser = new Parser(typeof(HelpArgument), new ErrorReporter(Parser.NullErrorReporter));
            HelpArgument destination = new HelpArgument();
            parser.Parse(args, destination);
            return destination.help;
        }

        private void ReportUnrecognizedArgument(string argument)
        {
            this.reporter(string.Format("Unrecognized command line argument '{0}'", argument));
        }

        private static string ShortName(ArgumentAttribute attribute, FieldInfo field)
        {
            if (attribute is DefaultArgumentAttribute)
            {
                return null;
            }
            if (!ExplicitShortName(attribute))
            {
                return LongName(attribute, field).Substring(0, 1);
            }
            return attribute.ShortName;
        }

        /// <summary>
        /// Does this parser have a default argument.
        /// </summary>
        /// <value> Does this parser have a default argument. </value>
        public bool HasDefaultArgument
        {
            get
            {
                return (this.defaultArgument != null);
            }
        }

        public class Argument
        {
            private ArrayList collectionValues;
            private object defaultValue;
            private global::System.Type elementType;
            private bool explicitShortName;
            private FieldInfo field;
            private ArgumentType flags;
            private bool hasHelpText;
            private string helpText;
            private bool isDefault;
            private bool isToggle;
            private string longName;
            private ErrorReporter reporter;
            private bool seenValue;
            private string shortName;

            public Argument(ArgumentAttribute attribute, FieldInfo field, ErrorReporter reporter)
            {
                this.longName = Parser.LongName(attribute, field);
                this.explicitShortName = Parser.ExplicitShortName(attribute);
                this.shortName = Parser.ShortName(attribute, field);
                this.hasHelpText = Parser.HasHelpText(attribute);
                this.helpText = Parser.HelpText(attribute, field);
                this.defaultValue = Parser.DefaultValue(attribute, field);
                this.elementType = Parser.ElementType(field);
                this.flags = Parser.Flags(attribute, field);
                this.field = field;
                this.seenValue = false;
                this.reporter = reporter;
                this.isDefault = (attribute != null) && (attribute is DefaultArgumentAttribute);
                this.isToggle = attribute.IsToggle;
                if (this.IsCollection)
                {
                    this.collectionValues = new ArrayList();
                }
            }

            private void AppendValue(StringBuilder builder, object value)
            {
                if (((value is string) || (value is int)) || ((value is uint) || value.GetType().IsEnum))
                {
                    builder.Append(value.ToString());
                }
                else if (value is bool)
                {
                    builder.Append(((bool) value) ? "+" : "-");
                }
                else
                {
                    bool flag = true;
                    foreach (object obj2 in (Array) value)
                    {
                        if (!flag)
                        {
                            builder.Append(", ");
                        }
                        this.AppendValue(builder, obj2);
                        flag = false;
                    }
                }
            }

            public void ClearShortName()
            {
                this.shortName = null;
            }

            public bool Finish(object destination)
            {
                if (!this.SeenValue && this.HasDefaultValue)
                {
                    this.field.SetValue(destination, this.DefaultValue);
                }
                if (this.IsCollection)
                {
                    this.field.SetValue(destination, this.collectionValues.ToArray(this.elementType));
                }
                return this.ReportMissingRequiredArgument();
            }

            private bool ParseValue(global::System.Type type, string stringData, out object value)
            {
                if (((stringData != null) || (type == typeof(bool))) && ((stringData == null) || (stringData.Length > 0)))
                {
                    try
                    {
                        if (type == typeof(string))
                        {
                            value = stringData;
                            return true;
                        }
                        if (type == typeof(bool))
                        {
                            if ((stringData == null) || (stringData == "+"))
                            {
                                value = true;
                                return true;
                            }
                            if (stringData == "-")
                            {
                                value = false;
                                return true;
                            }
                        }
                        else
                        {
                            if (type == typeof(int))
                            {
                                value = int.Parse(stringData);
                                return true;
                            }
                            if (type == typeof(uint))
                            {
                                value = int.Parse(stringData);
                                return true;
                            }
                            value = Enum.Parse(type, stringData, true);
                            return true;
                        }
                    }
                    catch
                    {
                    }
                }
                this.ReportBadArgumentValue(stringData);
                value = null;
                return false;
            }

            private void ReportBadArgumentValue(string value)
            {
                this.reporter(string.Format("'{0}' is not a valid value for the '{1}' command line option", value, this.LongName));
            }

            private void ReportDuplicateArgumentValue(string value)
            {
                this.reporter(string.Format("Duplicate '{0}' argument '{1}'", this.LongName, value));
            }

            private bool ReportMissingRequiredArgument()
            {
                if (!this.IsRequired || this.SeenValue)
                {
                    return false;
                }
                if (this.IsDefault)
                {
                    this.reporter(string.Format("Missing required argument '<{0}>'.", this.LongName));
                }
                else
                {
                    this.reporter(string.Format("Missing required argument '/{0}'.", this.LongName));
                }
                return true;
            }

            public bool SetValue(string value, object destination)
            {
                object obj2;
                if (this.SeenValue && !this.AllowMultiple)
                {
                    this.reporter(string.Format("Duplicate '{0}' argument", this.LongName));
                    return false;
                }
                this.seenValue = true;
                if (!this.ParseValue(this.ValueType, value, out obj2))
                {
                    return false;
                }
                if (this.IsCollection)
                {
                    if (this.Unique && this.collectionValues.Contains(obj2))
                    {
                        this.ReportDuplicateArgumentValue(value);
                        return false;
                    }
                    this.collectionValues.Add(obj2);
                }
                else
                {
                    this.field.SetValue(destination, obj2);
                }
                return true;
            }

            public bool AllowMultiple
            {
                get
                {
                    return (ArgumentType.AtMostOnce != (this.flags & ArgumentType.LastOccurenceWins));
                }
            }

            public object DefaultValue
            {
                get
                {
                    return this.defaultValue;
                }
            }

            public bool ExplicitShortName
            {
                get
                {
                    return this.explicitShortName;
                }
            }

            public string FullHelpText
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    if (this.HasHelpText)
                    {
                        builder.Append(this.HelpText);
                    }
                    if (this.HasDefaultValue)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(" ");
                        }
                        builder.Append("Default value:'");
                        this.AppendValue(builder, this.DefaultValue);
                        builder.Append('\'');
                    }
                    if (this.HasShortName)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(" ");
                        }
                        builder.Append("(short form /");
                        builder.Append(this.ShortName);
                        builder.Append(")");
                    }
                    return builder.ToString();
                }
            }

            public bool HasDefaultValue
            {
                get
                {
                    return (null != this.defaultValue);
                }
            }

            public bool HasHelpText
            {
                get
                {
                    return this.hasHelpText;
                }
            }

            public bool HasShortName
            {
                get
                {
                    return (this.shortName != null);
                }
            }

            public string HelpText
            {
                get
                {
                    return this.helpText;
                }
            }

            public bool IsCollection
            {
                get
                {
                    return Parser.IsCollectionType(this.Type);
                }
            }

            public bool IsDefault
            {
                get
                {
                    return this.isDefault;
                }
            }

            public bool IsRequired
            {
                get
                {
                    return (ArgumentType.AtMostOnce != (this.flags & ArgumentType.Required));
                }
            }

            public string LongName
            {
                get
                {
                    return this.longName;
                }
            }

            public bool SeenValue
            {
                get
                {
                    return this.seenValue;
                }
            }

            public string ShortName
            {
                get
                {
                    return this.shortName;
                }
            }

            public string SyntaxHelp
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    if (this.IsDefault)
                    {
                        builder.Append("<");
                        builder.Append(this.LongName);
                        builder.Append(">");
                    }
                    else
                    {
                        builder.Append("/");
                        builder.Append(this.LongName);
                        global::System.Type valueType = this.ValueType;
                        if (valueType == typeof(int))
                        {
                            builder.Append(":<int>");
                        }
                        else if (valueType == typeof(uint))
                        {
                            builder.Append(":<uint>");
                        }
                        else if (valueType == typeof(bool))
                        {
                            if (this.isToggle)
                            {
                                builder.Append("[+|-]");
                            }
                        }
                        else if (valueType == typeof(string))
                        {
                            builder.Append(":<string>");
                        }
                        else if (valueType.IsEnum)
                        {
                            builder.Append(":{");
                            bool flag = true;
                            foreach (FieldInfo info in valueType.GetFields())
                            {
                                if (info.IsStatic)
                                {
                                    if (flag)
                                    {
                                        flag = false;
                                    }
                                    else
                                    {
                                        builder.Append('|');
                                    }
                                    builder.Append(info.Name);
                                }
                            }
                            builder.Append('}');
                        }
                    }
                    return builder.ToString();
                }
            }

            public global::System.Type Type
            {
                get
                {
                    return this.field.FieldType;
                }
            }

            public bool Unique
            {
                get
                {
                    return (ArgumentType.AtMostOnce != (this.flags & ArgumentType.Unique));
                }
            }

            public global::System.Type ValueType
            {
                get
                {
                    if (!this.IsCollection)
                    {
                        return this.Type;
                    }
                    return this.elementType;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ArgumentHelpStrings
        {
            public string syntax;
            public string help;
            public ArgumentHelpStrings(string syntax, string help)
            {
                this.syntax = syntax;
                this.help = help;
            }
        }

        private class HelpArgument
        {
            [Argument(ArgumentType.AtMostOnce, ShortName="?")]
            public bool help;
        }
    }
}

