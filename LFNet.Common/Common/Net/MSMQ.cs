using System;
using System.Collections.Generic;
using System.Text;
using System.Messaging;
using System.Configuration;

namespace MSMQTest.Model
{
    /// <summary>
    /// 消息队列管理器 
    /// </summary>
    public class MSMQManager<T> : IDisposable
    {
        #region 字段与属性
        private MessageQueue _msmq = null;
        private string _path;

        #endregion

        /// <summary>
        /// 创建队列
        /// </summary>
        /// <param name="transactional">是否启用事务</param>
        /// <returns></returns>
        public bool Create(bool transactional)
        {
            if (MessageQueue.Exists(@".\private$\"+ConfigurationManager.AppSettings["MSMQName"] ?? "CSMSMQ"))
            {
                return true;
            }
            else
            {
                if (MessageQueue.Create(@".\private$\"+(ConfigurationManager.AppSettings["MSMQName"] ?? "CSMSMQ"), transactional) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 实例化消息队列
        /// </summary>
        /// <param name="isLocalComputer">是否为本机</param>
        public MSMQManager(bool isLocalComputer)
        {
            if (isLocalComputer)
            {
                _path = @".\private$\"+(ConfigurationManager.AppSettings["MSMQName"] ?? "CSMSMQ");
            }
            else
            {
                _path = @"FormatName:DIRECT=TCP:192.168.1.125\private$\"+(ConfigurationManager.AppSettings["MSMQName"] ?? "CSMSMQ");
            }

            _msmq = new MessageQueue(_path);
        }


        /// <summary>
        /// 发送消息队列
        /// </summary>
        /// <param name="msmqIndex">消息队列实体</param>
        /// <returns></returns>
        public void Send(T msmqIndex)
        {
            _msmq.Send(new Message(msmqIndex, new BinaryMessageFormatter()));
        }

        /// <summary>
        /// 接收消息队列,删除队列
        /// </summary>
        /// <returns></returns>
        public T ReceiveAndRemove()
        {
            T msmqIndex = default(T);
            _msmq.Formatter = new BinaryMessageFormatter();
            Message msg = null;
            try
            {
                msg = _msmq.Receive(new TimeSpan(0, 0, 1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (msg != null)
            {
                msmqIndex = (T)msg.Body;
            }
            return msmqIndex;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_msmq != null)
            {
                _msmq.Close();
                _msmq.Dispose();
                _msmq = null;
            }
        }

        #endregion
    }
}
