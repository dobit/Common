using System.Diagnostics;
using System.IO;
using System.Text;

namespace LFNet.Common.Diagnostics
{
    public class TraceTextWriter : TextWriter
    {
        private string _category;
        private global::System.Text.Encoding _encoding;

        public TraceTextWriter()
        {
            this._category = string.Empty;
        }

        public TraceTextWriter(string category)
        {
            this._category = string.Empty;
            this._category = category;
        }

        public override void Write(char value)
        {
            if (string.IsNullOrEmpty(this._category))
            {
                Trace.Write(value);
            }
            else
            {
                Trace.Write(value, this._category);
            }
        }

        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(this._category))
            {
                Trace.Write(value);
            }
            else
            {
                Trace.Write(value, this._category);
            }
        }

        public override global::System.Text.Encoding Encoding
        {
            get
            {
                if (this._encoding == null)
                {
                    this._encoding = new UnicodeEncoding(false, false);
                }
                return this._encoding;
            }
        }
    }
}

