using System;

namespace LFNet.Common.Win32
{
    /// <summary>
    /// A class representing the SendMailComplate event.
    /// </summary>
    public class SendMailCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.SendMailCompleteEventArgs" /> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public SendMailCompleteEventArgs(int errorCode)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>The error code.</value>
        public int ErrorCode { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage
        {
            get
            {
                return Mapi.GetError(this.ErrorCode);
            }
        }
    }
}

