using System;
using System.Globalization;
using System.IO;

namespace LFNet.Common.IO
{
    [Serializable]
    public class SafeTextWriter : TextWriter
    {
        private readonly TextWriter _writer;

        public SafeTextWriter(TextWriter writer) : base(CultureInfo.InvariantCulture)
        {
            this._writer = writer;
        }

        protected virtual void LogError(Exception ex)
        {
        }

        public override void Write(string value)
        {
            try
            {
                if (this._writer != null)
                {
                    this._writer.Write(value);
                }
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            try
            {
                if (this._writer != null)
                {
                    this._writer.Write(buffer, index, count);
                }
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }

        public override void WriteLine()
        {
            try
            {
                if (this._writer != null)
                {
                    this._writer.WriteLine();
                }
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }

        public override void WriteLine(object value)
        {
            try
            {
                if (this._writer != null)
                {
                    this._writer.WriteLine(value);
                }
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }

        public override void WriteLine(string value)
        {
            try
            {
                if (this._writer != null)
                {
                    this._writer.WriteLine(value);
                }
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }

        public override global::System.Text.Encoding Encoding
        {
            get
            {
                if (this._writer == null)
                {
                    return global::System.Text.Encoding.Default;
                }
                return this._writer.Encoding;
            }
        }
    }
}

