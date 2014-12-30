using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

namespace LFNet.Common.Text
{
    /// <summary>
    /// Named string formatter.
    /// </summary>
    public static class NameFormatter
    {
        /// <summary>
        /// Formats the specified input string.
        /// </summary>
        /// <param name="format">The input format string.</param>
        /// <param name="source">The source to replace the formant names with.</param>
        /// <returns>A formatted string.</returns>
        public static string Format(string format, object source)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            StringBuilder builder = new StringBuilder(format.Length * 2);
            using (StringReader reader = new StringReader(format))
            {
                StringBuilder builder2 = new StringBuilder();
                int num = -1;
                State outsideExpression = State.OutsideExpression;
            Label_002E:
                switch (outsideExpression)
                {
                    case State.OutsideExpression:
                        switch (reader.Read())
                        {
                            case 0x7b:
                                goto Label_007D;

                            case 0x7d:
                                goto Label_0085;
                        }
                        goto Label_008D;

                    case State.OnOpenBracket:
                        switch (reader.Read())
                        {
                            case -1:
                                throw new FormatException();

                            case 0x7b:
                                goto Label_00B8;
                        }
                        goto Label_00C9;

                    case State.InsideExpression:
                        num = reader.Read();
                        switch (num)
                        {
                            case -1:
                                throw new FormatException();

                            case 0x7d:
                                goto Label_00F4;
                        }
                        goto Label_0113;

                    case State.OnCloseBracket:
                        if (reader.Read() != 0x7d)
                        {
                            throw new FormatException();
                        }
                        builder.Append('}');
                        outsideExpression = State.OutsideExpression;
                        goto Label_014D;

                    default:
                        throw new InvalidOperationException("Invalid state.");
                }
                outsideExpression = State.End;
                goto Label_014D;
            Label_007D:
                outsideExpression = State.OnOpenBracket;
                goto Label_014D;
            Label_0085:
                outsideExpression = State.OnCloseBracket;
                goto Label_014D;
            Label_008D:
                builder.Append((char) num);
                goto Label_014D;
            Label_00B8:
                builder.Append('{');
                outsideExpression = State.OutsideExpression;
                goto Label_014D;
            Label_00C9:
                builder2.Append((char) num);
                outsideExpression = State.InsideExpression;
                goto Label_014D;
            Label_00F4:
                builder.Append(OutExpression(source, builder2.ToString()));
                builder2.Length = 0;
                outsideExpression = State.OutsideExpression;
                goto Label_014D;
            Label_0113:
                builder2.Append((char) num);
            Label_014D:
                if (outsideExpression != State.End)
                {
                    goto Label_002E;
                }
            }
            return builder.ToString();
        }

        private static string OutExpression(object source, string expression)
        {
            string str2;
            string str = "";
            int index = expression.IndexOf(':');
            if (index > 0)
            {
                str = expression.Substring(index + 1);
                expression = expression.Substring(0, index);
            }
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    return (DataBinder.Eval(source, expression) ?? "").ToString();
                }
                str2 = DataBinder.Eval(source, expression, "{0:" + str + "}") ?? "";
            }
            catch (HttpException)
            {
                throw new FormatException();
            }
            return str2;
        }

        private enum State
        {
            OutsideExpression,
            OnOpenBracket,
            InsideExpression,
            OnCloseBracket,
            End
        }
    }
}

