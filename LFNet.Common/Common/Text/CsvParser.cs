using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace LFNet.Common.Text
{
    public class CsvParser
    {
        private static string GetNextColumnHeader(DataTable table)
        {
            string str;
            int num = 1;
            do
            {
                str = "Column" + num++;
            }
            while (table.Columns.Contains(str));
            return str;
        }

        public static DataTable Parse(TextReader stream)
        {
            return Parse(stream, false);
        }

        public static DataTable Parse(string data)
        {
            return Parse(new StringReader(data));
        }

        public static DataTable Parse(TextReader stream, bool headers)
        {
            DataTable table = new DataTable();
            CsvStream stream2 = new CsvStream(stream);
            string[] nextRow = stream2.GetNextRow();
            if (nextRow == null)
            {
                return null;
            }
            if (headers)
            {
                foreach (string str in nextRow)
                {
                    if (!string.IsNullOrEmpty(str) && !table.Columns.Contains(str))
                    {
                        table.Columns.Add(str, typeof(string));
                    }
                    else
                    {
                        table.Columns.Add(GetNextColumnHeader(table), typeof(string));
                    }
                }
                nextRow = stream2.GetNextRow();
            }
            while (nextRow != null)
            {
                while (nextRow.Length > table.Columns.Count)
                {
                    table.Columns.Add(GetNextColumnHeader(table), typeof(string));
                }
                table.Rows.Add(nextRow);
                nextRow = stream2.GetNextRow();
            }
            return table;
        }

        public static DataTable Parse(string data, bool headers)
        {
            return Parse(new StringReader(data), headers);
        }

        public static string[] ParseValues(string data)
        {
            List<string> list = new List<string>();
            DataTable table = Parse(new StringReader(data), false);
            if (table.Rows.Count > 0)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    list.Add(table.Rows[0][i].ToString());
                }
            }
            return list.ToArray();
        }

        private class CsvStream
        {
            private char[] buffer = new char[0x1000];
            private bool EOL;
            private bool EOS;
            private int length;
            private int pos;
            private readonly TextReader stream;

            public CsvStream(TextReader s)
            {
                this.stream = s;
            }

            private char GetNextChar(bool eat)
            {
                if (this.pos >= this.length)
                {
                    this.length = this.stream.ReadBlock(this.buffer, 0, this.buffer.Length);
                    if (this.length == 0)
                    {
                        this.EOS = true;
                        return '\0';
                    }
                    this.pos = 0;
                }
                if (eat)
                {
                    return this.buffer[this.pos++];
                }
                return this.buffer[this.pos];
            }

            private string GetNextItem()
            {
                if (this.EOL)
                {
                    this.EOL = false;
                    return null;
                }
                bool flag = false;
                bool flag2 = true;
                bool flag3 = false;
                StringBuilder builder = new StringBuilder();
                while (true)
                {
                    char nextChar = this.GetNextChar(true);
                    if (this.EOS)
                    {
                        if (builder.Length <= 0)
                        {
                            return null;
                        }
                        return builder.ToString();
                    }
                    if ((flag3 || !flag) && (nextChar == ','))
                    {
                        return builder.ToString();
                    }
                    if (((flag2 || flag3) || !flag) && ((nextChar == '\n') || (nextChar == '\r')))
                    {
                        this.EOL = true;
                        if ((nextChar == '\r') && (this.GetNextChar(false) == '\n'))
                        {
                            this.GetNextChar(true);
                        }
                        return builder.ToString();
                    }
                    if (!flag2 || (nextChar != ' '))
                    {
                        if (flag2 && (nextChar == '"'))
                        {
                            flag = true;
                            flag2 = false;
                        }
                        else if (flag2)
                        {
                            flag2 = false;
                            builder.Append(nextChar);
                        }
                        else if ((nextChar == '"') && flag)
                        {
                            if (this.GetNextChar(false) == '"')
                            {
                                builder.Append(this.GetNextChar(true));
                            }
                            else
                            {
                                flag3 = true;
                            }
                        }
                        else
                        {
                            builder.Append(nextChar);
                        }
                    }
                }
            }

            public string[] GetNextRow()
            {
                ArrayList list = new ArrayList();
                while (true)
                {
                    string nextItem = this.GetNextItem();
                    if (nextItem == null)
                    {
                        if (list.Count != 0)
                        {
                            return (string[]) list.ToArray(typeof(string));
                        }
                        return null;
                    }
                    list.Add(nextItem);
                }
            }
        }
    }
}

