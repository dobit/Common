using System;
using System.Diagnostics;
using System.Threading;

namespace LFNet.Threading
{
    /// <summary>
    /// 等待超时
    /// </summary>
    public class WaitAction
    {
        private readonly Action _action;
        private readonly int _timeOut;

        public WaitAction(Action action,int timeOut)
        {
            Debug.Assert(action != null, "action != null");
            _action = action;
            _timeOut = timeOut;
        }

        public void Run()
        {
            Thread thread = new Thread(() => _action()) {IsBackground = true};
            thread.Start();
            int i = 0;
            while (thread.IsAlive && i < _timeOut)
            {
                Thread.Sleep(200);
                i += 200;
            }
            if (thread.IsAlive && i >= _timeOut)
            {
                thread.Abort();
                throw new Exception("执行任务超时，{0}" + _action.Method);
            }
        }
    }
}