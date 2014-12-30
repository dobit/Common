using System;
using System.Threading;

namespace LFNet.Common.Win32
{
    /// <summary>
    /// Represents an email message to be sent through MAPI.
    /// </summary>
    public class MailDialog
    {
        private readonly ManualResetEvent _manualResetEvent;

        /// <summary>
        /// Occurs when the MAPI send mail call is complete.
        /// </summary>
        public event EventHandler<SendMailCompleteEventArgs> SendMailComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.MailDialog" /> class.
        /// </summary>
        public MailDialog()
        {
            this._manualResetEvent = new ManualResetEvent(false);
            this.Attachments = new AttachmentCollection();
            this.Recipients = new RecipientCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.MailDialog" /> class with the specified subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        public MailDialog(string subject) : this()
        {
            this.Subject = subject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.MailDialog" /> class with the specified subject and body.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        public MailDialog(string subject, string body) : this()
        {
            this.Subject = subject;
            this.Body = body;
        }

        /// <summary>
        /// Called when send mail is complete.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        protected void OnSendMailComplete(int errorCode)
        {
            if (this.SendMailComplete != null)
            {
                SendMailCompleteEventArgs e = new SendMailCompleteEventArgs(errorCode);
                this.SendMailComplete(this, e);
            }
        }

        /// <summary>
        /// Displays the mail message dialog asynchronously.
        /// </summary>
        public void ShowAsync()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(this.ShowMail)) {
                IsBackground = true
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(this._manualResetEvent);
            this._manualResetEvent.WaitOne();
            this._manualResetEvent.Reset();
        }

        /// <summary>
        /// Displays the mail message dialog. The call is block until the dialog is closed.
        /// </summary>
        /// <returns>The error code from the mapi call.</returns>
        public int ShowDialog()
        {
            return this.ShowMail((EventWaitHandle) null);
        }

        private void ShowMail(object parameter)
        {
            ManualResetEvent event2 = parameter as ManualResetEvent;
            int errorCode = this.ShowMail((EventWaitHandle) event2);
            if (errorCode > 1)
            {
                Mapi.GetError(errorCode);
            }
        }

        private int ShowMail(EventWaitHandle waitHandle)
        {
            int num;
            Mapi.MapiMessage message = new Mapi.MapiMessage();
            using (RecipientCollection.RecipientCollectionHandle handle = this.Recipients.GetHandle())
            {
                using (AttachmentCollection.AttachmentCollectionHandle handle2 = this.Attachments.GetHandle())
                {
                    message.Subject = this.Subject;
                    message.NoteText = this.Body;
                    message.Recipients = (IntPtr) handle;
                    message.RecipientCount = this.Recipients.Count;
                    message.Files = (IntPtr) handle2;
                    message.FileCount = this.Attachments.Count;
                    if (waitHandle != null)
                    {
                        waitHandle.Set();
                    }
                    num = Mapi.MAPISendMail(IntPtr.Zero, IntPtr.Zero, message, 8, 0);
                }
            }
            this.OnSendMailComplete(num);
            return num;
        }

        /// <summary>
        /// Gets the attachment list for this mail message.
        /// </summary>
        public AttachmentCollection Attachments { get; private set; }

        /// <summary>
        /// Gets or sets the body of this mail message.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets the recipient list for this mail message.
        /// </summary>
        public RecipientCollection Recipients { get; private set; }

        /// <summary>
        /// Gets or sets the subject of this mail message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Specifies the valid RecipientTypes for a Recipient.
        /// </summary>
        public enum RecipientType
        {
            /// <summary>
            /// Recipient will be in the BCC list.
            /// </summary>
            BCC = 3,
            /// <summary>
            /// Recipient will be in the CC list.
            /// </summary>
            CC = 2,
            /// <summary>
            /// Recipient will be in the TO list.
            /// </summary>
            To = 1
        }
    }
}

