using System;
using System.Collections.Generic;
using System.Threading;
using LFNet.Common.Logs;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// 后台任务类
    /// </summary>
    public class BackgroundTasks
    {
        private class TaskEntity
        {
            public TaskEntity(Action<object> func, object data)
            {
                this.Function = func;
                this.Data = data;
            }

            public Action<object> Function;
            public object Data;
        }

        private static Queue<TaskEntity> list = new Queue<TaskEntity>();
        private static bool _isRuning;

        private static void RunTask()
        {
            do
            {
                TaskEntity entity;
                lock (list)
                {
                    entity = list.Dequeue();
                }
                try
                {
                    entity.Function(entity.Data);
                }
                catch (Exception ex)
                {
                    LogUtil.Log(ex);
                }
            } while (list.Count > 0);
            _isRuning = false;
        }

        public static void Add(Action<object> func, object data)
        {
            lock (list)
            {
                list.Enqueue(new TaskEntity(func, data));
                if (!_isRuning)
                {
                    _isRuning = true;
                    ThreadPool.QueueUserWorkItem(o => RunTask());
                }
            }

        }

    }
}