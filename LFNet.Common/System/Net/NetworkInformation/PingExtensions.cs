using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Net.NetworkInformation
{
    /// <summary>Extension methods for working with Ping asynchronously.</summary>
    public static class PingExtensions
    {
        /// <summary>
        /// Asynchronously attempts to send an Internet Control Message Protocol (ICMP) echo message.
        /// </summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="address">An IPAddress that identifies the computer that is the destination for the ICMP echo message.</param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<PingReply> SendTask(this Ping ping, IPAddress address, object userToken)
        {
            return SendTaskCore(ping, userToken, delegate (TaskCompletionSource<PingReply> tcs) {
                ping.SendAsync(address, tcs);
            });
        }

        /// <summary>
        /// Asynchronously attempts to send an Internet Control Message Protocol (ICMP) echo message.
        /// </summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="hostNameOrAddress">
        /// A String that identifies the computer that is the destination for the ICMP echo message. 
        /// The value specified for this parameter can be a host name or a string representation of an IP address.
        /// </param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<PingReply> SendTask(this Ping ping, string hostNameOrAddress, object userToken)
        {
            return SendTaskCore(ping, userToken, delegate (TaskCompletionSource<PingReply> tcs) {
                ping.SendAsync(hostNameOrAddress, tcs);
            });
        }

        /// <summary>
        /// Asynchronously attempts to send an Internet Control Message Protocol (ICMP) echo message.
        /// </summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="address">An IPAddress that identifies the computer that is the destination for the ICMP echo message.</param>
        /// <param name="timeout">
        /// An Int32 value that specifies the maximum number of milliseconds (after sending the echo message) 
        /// to wait for the ICMP echo reply message.
        /// </param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<PingReply> SendTask(this Ping ping, IPAddress address, int timeout, object userToken)
        {
            return SendTaskCore(ping, userToken, delegate (TaskCompletionSource<PingReply> tcs) {
                ping.SendAsync(address, timeout, tcs);
            });
        }

        /// <summary>
        /// Asynchronously attempts to send an Internet Control Message Protocol (ICMP) echo message.
        /// </summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="hostNameOrAddress">
        /// A String that identifies the computer that is the destination for the ICMP echo message. 
        /// The value specified for this parameter can be a host name or a string representation of an IP address.
        /// </param>
        /// <param name="timeout">
        /// An Int32 value that specifies the maximum number of milliseconds (after sending the echo message) 
        /// to wait for the ICMP echo reply message.
        /// </param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<PingReply> SendTask(this Ping ping, string hostNameOrAddress, int timeout, object userToken)
        {
            return SendTaskCore(ping, userToken, delegate (TaskCompletionSource<PingReply> tcs) {
                ping.SendAsync(hostNameOrAddress, timeout, tcs);
            });
        }

        /// <summary>
        /// Asynchronously attempts to send an Internet Control Message Protocol (ICMP) echo message.
        /// </summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="address">An IPAddress that identifies the computer that is the destination for the ICMP echo message.</param>
        /// <param name="timeout">
        /// An Int32 value that specifies the maximum number of milliseconds (after sending the echo message) 
        /// to wait for the ICMP echo reply message.
        /// </param>
        /// <param name="buffer">
        /// A Byte array that contains data to be sent with the ICMP echo message and returned 
        /// in the ICMP echo reply message. The array cannot contain more than 65,500 bytes.
        /// </param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<PingReply> SendTask(this Ping ping, IPAddress address, int timeout, byte[] buffer, object userToken)
        {
            return SendTaskCore(ping, userToken, delegate (TaskCompletionSource<PingReply> tcs) {
                ping.SendAsync(address, timeout, buffer, tcs);
            });
        }

        /// <summary>
        /// Asynchronously attempts to send an Internet Control Message Protocol (ICMP) echo message.
        /// </summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="hostNameOrAddress">
        /// A String that identifies the computer that is the destination for the ICMP echo message. 
        /// The value specified for this parameter can be a host name or a string representation of an IP address.
        /// </param>
        /// <param name="timeout">
        /// An Int32 value that specifies the maximum number of milliseconds (after sending the echo message) 
        /// to wait for the ICMP echo reply message.
        /// </param>
        /// <param name="buffer">
        /// A Byte array that contains data to be sent with the ICMP echo message and returned 
        /// in the ICMP echo reply message. The array cannot contain more than 65,500 bytes.
        /// </param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<PingReply> SendTask(this Ping ping, string hostNameOrAddress, int timeout, byte[] buffer, object userToken)
        {
            return SendTaskCore(ping, userToken, delegate (TaskCompletionSource<PingReply> tcs) {
                ping.SendAsync(hostNameOrAddress, timeout, buffer, tcs);
            });
        }

        /// <summary>
        /// Asynchronously attempts to send an Internet Control Message Protocol (ICMP) echo message.
        /// </summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="address">An IPAddress that identifies the computer that is the destination for the ICMP echo message.</param>
        /// <param name="timeout">
        /// An Int32 value that specifies the maximum number of milliseconds (after sending the echo message) 
        /// to wait for the ICMP echo reply message.
        /// </param>
        /// <param name="buffer">
        /// A Byte array that contains data to be sent with the ICMP echo message and returned 
        /// in the ICMP echo reply message. The array cannot contain more than 65,500 bytes.
        /// </param>
        /// <param name="options">A PingOptions object used to control fragmentation and Time-to-Live values for the ICMP echo message packet.</param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<PingReply> SendTask(this Ping ping, IPAddress address, int timeout, byte[] buffer, PingOptions options, object userToken)
        {
            return SendTaskCore(ping, userToken, delegate (TaskCompletionSource<PingReply> tcs) {
                ping.SendAsync(address, timeout, buffer, options, tcs);
            });
        }

        /// <summary>
        /// Asynchronously attempts to send an Internet Control Message Protocol (ICMP) echo message.
        /// </summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="hostNameOrAddress">
        /// A String that identifies the computer that is the destination for the ICMP echo message. 
        /// The value specified for this parameter can be a host name or a string representation of an IP address.
        /// </param>
        /// <param name="timeout">
        /// An Int32 value that specifies the maximum number of milliseconds (after sending the echo message) 
        /// to wait for the ICMP echo reply message.
        /// </param>
        /// <param name="buffer">
        /// A Byte array that contains data to be sent with the ICMP echo message and returned 
        /// in the ICMP echo reply message. The array cannot contain more than 65,500 bytes.
        /// </param>
        /// <param name="options">A PingOptions object used to control fragmentation and Time-to-Live values for the ICMP echo message packet.</param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<PingReply> SendTask(this Ping ping, string hostNameOrAddress, int timeout, byte[] buffer, PingOptions options, object userToken)
        {
            return SendTaskCore(ping, userToken, delegate (TaskCompletionSource<PingReply> tcs) {
                ping.SendAsync(hostNameOrAddress, timeout, buffer, options, tcs);
            });
        }

        /// <summary>The core implementation of SendTask.</summary>
        /// <param name="ping">The Ping.</param>
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param>
        /// <param name="sendAsync">
        /// A delegate that initiates the asynchronous send.
        /// The provided TaskCompletionSource must be passed as the user-supplied state to the actual Ping.SendAsync method.
        /// </param>
        /// <returns></returns>
        private static Task<PingReply> SendTaskCore(Ping ping, object userToken, Action<TaskCompletionSource<PingReply>> sendAsync)
        {
            if (ping == null)
            {
                throw new ArgumentNullException("ping");
            }
            TaskCompletionSource<PingReply> tcs = new TaskCompletionSource<PingReply>(userToken);
            PingCompletedEventHandler handler = null;
            handler = delegate (object sender, PingCompletedEventArgs e) {
                EAPCommon.HandleCompletion<PingReply>(tcs, e, () => e.Reply, delegate {
                    ping.PingCompleted -= handler;
                });
            };
            ping.PingCompleted += handler;
            try
            {
                sendAsync(tcs);
            }
            catch (Exception exception)
            {
                ping.PingCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }
    }
}

