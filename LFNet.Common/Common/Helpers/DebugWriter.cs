using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace LFNet.Common.Helpers
{
    /// <summary>
    /// Implements a <see cref="T:System.IO.TextWriter" /> for writing information to the debugger log.
    /// </summary>
    /// <seealso cref="M:System.Diagnostics.Debugger.Log(System.Int32,System.String,System.String)" />
    public class DebugWriter : TextWriter
    {
        private readonly string category;
        private static UnicodeEncoding encoding;
        private bool isOpen;
        private readonly int level;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Helpers.DebugWriter" /> class.
        /// </summary>
        public DebugWriter() : this(0, Debugger.DefaultCategory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Helpers.DebugWriter" /> class with the specified level and category.
        /// </summary>
        /// <param name="level">A description of the importance of the messages.</param>
        /// <param name="category">The category of the messages.</param>
        public DebugWriter(int level, string category) : this(level, category, CultureInfo.CurrentCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Helpers.DebugWriter" /> class with the specified level, category and format provider.
        /// </summary>
        /// <param name="level">A description of the importance of the messages.</param>
        /// <param name="category">The category of the messages.</param>
        /// <param name="formatProvider">An <see cref="T:System.IFormatProvider" /> object that controls formatting.</param>
        public DebugWriter(int level, string category, IFormatProvider formatProvider) : base(formatProvider)
        {
            this.level = level;
            this.category = category;
            this.isOpen = true;
        }

        protected override void Dispose(bool disposing)
        {
            this.isOpen = false;
            base.Dispose(disposing);
        }

        public override void Write(char value)
        {
            if (!this.isOpen)
            {
                throw new ObjectDisposedException(null);
            }
            Debugger.Log(this.level, this.category, value.ToString());
        }

        public override void Write(string value)
        {
            if (!this.isOpen)
            {
                throw new ObjectDisposedException(null);
            }
            if (value != null)
            {
                Debugger.Log(this.level, this.category, value);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (!this.isOpen)
            {
                throw new ObjectDisposedException(null);
            }
            if (((buffer == null) || (index < 0)) || ((count < 0) || ((buffer.Length - index) < count)))
            {
                base.Write(buffer, index, count);
            }
            Debugger.Log(this.level, this.category, new string(buffer, index, count));
        }

        public string Category
        {
            get
            {
                return this.category;
            }
        }

        public override global::System.Text.Encoding Encoding
        {
            get
            {
                if (encoding == null)
                {
                    encoding = new UnicodeEncoding(false, false);
                }
                return encoding;
            }
        }

        public int Level
        {
            get
            {
                return this.level;
            }
        }
    }
}

