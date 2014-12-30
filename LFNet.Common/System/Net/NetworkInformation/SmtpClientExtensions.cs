using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Net.NetworkInformation
{
    /// <summary>Extension methods for working with SmtpClient asynchronously.</summary>
    public static class SmtpClientExtensions
    {
        /// <summary>Sends an e-mail message asynchronously.</summary>
        /// <param name="smtpClient">The client.</param>
        /// <param name="message">A MailMessage that contains the message to send.</param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A Task that represents the asynchronous send.</returns>
        public static Task SendTask(this SmtpClient smtpClient, MailMessage message, object userToken)
        {
            return SendTaskCore(smtpClient, userToken, delegate (TaskCompletionSource<object> tcs) {
                smtpClient.SendAsync(message, tcs);
            });
        }

        /// <summary>Sends an e-mail message asynchronously.</summary>
        /// <param name="smtpClient">The client.</param>
        /// <param name="from">A String that contains the address information of the message sender.</param>
        /// <param name="recipients">A String that contains the address that the message is sent to.</param>
        /// <param name="subject">A String that contains the subject line for the message.</param>
        /// <param name="body">A String that contains the message body.</param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A Task that represents the asynchronous send.</returns>
        public static Task SendTask(this SmtpClient smtpClient, string from, string recipients, string subject, string body, object userToken)
        {
            return SendTaskCore(smtpClient, userToken, delegate (TaskCompletionSource<object> tcs) {
                smtpClient.SendAsync(from, recipients, subject, body, tcs);
            });
        }

        /// <summary>The core implementation of SendTask.</summary>
        /// <param name="smtpClient">The client.</param>
        /// <param name="userToken">The user-supplied state.</param>
        /// <param name="sendAsync">
        /// A delegate that initiates the asynchronous send.
        /// The provided TaskCompletionSource must be passed as the user-supplied state to the actual SmtpClient.SendAsync method.
        /// </param>
        /// <returns></returns>
        private static Task SendTaskCore(SmtpClient smtpClient, object userToken, Action<TaskCompletionSource<object>> sendAsync)
        {
            if (smtpClient == null)
            {
                throw new ArgumentNullException("smtpClient");
            }
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(userToken);
            SendCompletedEventHandler handler = null;
            handler = delegate (object sender, AsyncCompletedEventArgs e) {
                EAPCommon.HandleCompletion<object>(tcs, e, () => null, delegate {
                    smtpClient.SendCompleted -= handler;
                });
            };
            smtpClient.SendCompleted += handler;
            try
            {
                sendAsync(tcs);
            }
            catch (Exception exception)
            {
                smtpClient.SendCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }
    }
}

