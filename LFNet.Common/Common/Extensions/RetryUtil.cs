using System;
using System.Threading;

namespace LFNet.Common.Extensions
{
    public static class RetryUtil
    {
        /// <summary>
        /// 重试
        /// </summary>
        /// <param name="action"></param>
        /// <param name="attempts"></param>
        /// <param name="retryTimeout"></param>
        public static void Retry(this Action action, int attempts = 3, int retryTimeout = 100)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            do
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    if (attempts <= 0)
                    {
                        throw;
                    }
                    Thread.Sleep(retryTimeout);
                }
            }
            while (attempts-- > 0);
        }
    }
}

