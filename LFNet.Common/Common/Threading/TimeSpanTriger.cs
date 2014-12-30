using System;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// 时间间隔触发器
    /// </summary>
    public static class TimeSpanTriger
    {
        private const int MinSleep = 200;
        /// <summary>
        /// 队列集合
        /// </summary>
        private static List<ActionInfo> actionInfos=new List<ActionInfo>();

        /// <summary>
        /// 休眠
        /// </summary>
        private static int _sleep = MinSleep;
        

        /// <summary>
        /// 添加一个操作
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"> </param>
        /// <param name="interval">间隔 最小间隔200ms</param>
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
        /// 添加一个操作并马上执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval">间隔 最小间隔200ms</param>
        public static void AddToRun(Action<object> action, object state, TimeSpan interval)
        {
            Add(action, state,interval);
            System.Threading.ThreadPool.QueueUserWorkItem((object o)=>action(o), state);
        }

        /// <summary>
        /// 添加一个操作
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval">间隔 最小间隔200ms</param>
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
        /// 添加一个操作并马上执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval">间隔 最小间隔200ms</param>
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
                        System.Threading.Thread.Sleep(MinSleep);//休眠一秒
                        t += MinSleep;
                        if(t>=_sleep) break;
                    }
                    
                }
            }
            else
            {
                System.Threading.Thread.Sleep(_sleep);//休眠一段时间
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
            /// 设置一个回调
            /// </summary>
            public Action Action { get; set; }

            /// <summary>
            /// 对象回调
            /// </summary>
            public Action<object> ObjectAction { get; set; } 
            /// <summary>
            /// 传递的状态
            /// </summary>
            public object State { get; set; }

            /// <summary>
            /// 执行间隔
            /// </summary>
            public TimeSpan Interval { get; set; }
           

            /// <summary>
            /// 是否要等之前的完成了再执行
            /// </summary>
            public bool WaitBeforeFinished { get; set; }

            /// <summary>
            /// 状态
            /// </summary>
            public ActionStatus Status { get; set; }


            /// <summary>
            /// 下次执行时间
            /// </summary>
            public DateTime NextRunTime
            {
                get { return LastRunTime + Interval; }
                //set { _nextRunTime = value; }
            }

            /// <summary>
            /// 最后运行的时间
            /// </summary>
            public DateTime LastRunTime { get; set; }
        }

        private enum  ActionStatus
        {
            /// <summary>
            /// 停状态
            /// </summary>
            Stop,
           
            /// <summary>
            /// 执行中
            /// </summary>
            Runing
        }
    }
}