using System;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// ʱ����������
    /// </summary>
    public static class TimeSpanTriger
    {
        private const int MinSleep = 200;
        /// <summary>
        /// ���м���
        /// </summary>
        private static List<ActionInfo> actionInfos=new List<ActionInfo>();

        /// <summary>
        /// ����
        /// </summary>
        private static int _sleep = MinSleep;
        

        /// <summary>
        /// ���һ������
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"> </param>
        /// <param name="interval">��� ��С���200ms</param>
        public static void Add(Action<object> action,object state,TimeSpan interval)
        {
            ActionInfo actionInfo=new ActionInfo();
            actionInfo.ObjectAction = action;
            actionInfo.LastRunTime = DateTime.Now;
            actionInfo.Interval = interval;
            actionInfo.State = state;
            lock (actionInfos)
            {
                actionInfos.Add(actionInfo);
            }
            if(actionInfos.Count==1)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(Run, null);
            }
            
            _sleep =Math.Max(MinSleep,Math.Min(interval.Milliseconds, _sleep));
        }
        /// <summary>
        /// ���һ������������ִ��
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval">��� ��С���200ms</param>
        public static void AddToRun(Action<object> action, object state, TimeSpan interval)
        {
            Add(action, state,interval);
            System.Threading.ThreadPool.QueueUserWorkItem((object o)=>action(o), state);
        }

        /// <summary>
        /// ���һ������
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval">��� ��С���200ms</param>
        public static void Add(Action action,  TimeSpan interval)
        {
            ActionInfo actionInfo = new ActionInfo();
            actionInfo.Action = action;
            actionInfo.LastRunTime = DateTime.Now;
            actionInfo.Interval = interval;
            lock (actionInfos)
            {
                actionInfos.Add(actionInfo);
            }
            if (actionInfos.Count == 1)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(Run, null);
            }

            _sleep = Math.Max(MinSleep, Math.Min(interval.Milliseconds, _sleep));
        }

        /// <summary>
        /// ���һ������������ִ��
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval">��� ��С���200ms</param>
        public static void AddToRun(Action action,  TimeSpan interval)
        {
            Add(action, interval);
            System.Threading.ThreadPool.QueueUserWorkItem((object o)=>action(), null);
        }

        public static void Remove(Action callBack)
        {
            var index=  actionInfos.FindIndex(p => p.Action == callBack);
            if(index>-1)
            {
                lock (actionInfos)
                {
                    actionInfos.RemoveAt(index);
                }
                _sleep =Math.Max( actionInfos.Min(p => p.Interval).Milliseconds,MinSleep);
            }
            else
            {
                throw new Exception("action Not Find");
            }
        }

        public static void Remove(Action<object> objectAction)
        {
            var index = actionInfos.FindIndex(p => p.ObjectAction == objectAction);
            if (index > -1)
            {
                lock (actionInfos)
                {
                    actionInfos.RemoveAt(index);
                }
                _sleep = Math.Max(actionInfos.Min(p => p.Interval).Milliseconds, MinSleep);
            }
            else
            {
                throw new Exception("action Not Find");
            }
        }

        private static void Run(object obj)
        {
            List<ActionInfo> checkStateActions=new List<ActionInfo>();
            foreach (ActionInfo actionInfo in GetToRunActions())
            {
                if (actionInfo.Status==ActionStatus.Runing && actionInfo.WaitBeforeFinished)
                {
                    checkStateActions.Add(actionInfo);
                }else
                {
                    //willRemoves.Add(actionInfo);
                    actionInfo.Status = ActionStatus.Runing;
                    actionInfo.LastRunTime = DateTime.Now;
                    System.Threading.ThreadPool.QueueUserWorkItem(WaitCallBack, actionInfo);
                    
                }
                
            }
            if (checkStateActions.Count>0)
            {
                int t = 0;
                while (true)
                {
                    var action=  checkStateActions.Find(p => p.Status == ActionStatus.Stop);
                    if(action!=null)
                    {
                        break;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(MinSleep);//����һ��
                        t += MinSleep;
                        if(t>=_sleep) break;
                    }
                    
                }
            }
            else
            {
                System.Threading.Thread.Sleep(_sleep);//����һ��ʱ��
            }
            System.Threading.ThreadPool.QueueUserWorkItem(Run, null);
        }

        private static void WaitCallBack(object obj)
        {
            ActionInfo actionInfo = obj as ActionInfo;
            try
            {
                if(actionInfo.Action!=null)
                {
                    actionInfo.Action();
                }
                else
                {
                    actionInfo.ObjectAction(actionInfo.State);
                }
                
            }
            catch (Exception ex)
            {

            }
            actionInfo.Status = ActionStatus.Stop;
            
        }

        private static List<ActionInfo> GetToRunActions()
        {
            DateTime now = DateTime.Now;
            return actionInfos.FindAll(p => p.NextRunTime <= now);
           
        }

        private class  ActionInfo
        {
            /// <summary>
            /// ����һ���ص�
            /// </summary>
            public Action Action { get; set; }

            /// <summary>
            /// ����ص�
            /// </summary>
            public Action<object> ObjectAction { get; set; } 
            /// <summary>
            /// ���ݵ�״̬
            /// </summary>
            public object State { get; set; }

            /// <summary>
            /// ִ�м��
            /// </summary>
            public TimeSpan Interval { get; set; }
           

            /// <summary>
            /// �Ƿ�Ҫ��֮ǰ���������ִ��
            /// </summary>
            public bool WaitBeforeFinished { get; set; }

            /// <summary>
            /// ״̬
            /// </summary>
            public ActionStatus Status { get; set; }


            /// <summary>
            /// �´�ִ��ʱ��
            /// </summary>
            public DateTime NextRunTime
            {
                get { return LastRunTime + Interval; }
                //set { _nextRunTime = value; }
            }

            /// <summary>
            /// ������е�ʱ��
            /// </summary>
            public DateTime LastRunTime { get; set; }
        }

        private enum  ActionStatus
        {
            /// <summary>
            /// ͣ״̬
            /// </summary>
            Stop,
           
            /// <summary>
            /// ִ����
            /// </summary>
            Runing
        }
    }
}