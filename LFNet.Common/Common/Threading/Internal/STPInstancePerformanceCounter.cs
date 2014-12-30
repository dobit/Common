using System;
using System.Diagnostics;

namespace LFNet.Common.Threading.Internal
{
    internal class STPInstancePerformanceCounter : IDisposable
    {
        private PerformanceCounter _pcs;

        protected STPInstancePerformanceCounter()
        {
        }

        public STPInstancePerformanceCounter(string instance, STPPerformanceCounterType spcType)
        {
            STPPerformanceCounters counters = STPPerformanceCounters.Instance;
            this._pcs = new PerformanceCounter("SmartThreadPool", counters._stpPerformanceCounters[(int) spcType].Name, instance, false);
            this._pcs.RawValue = this._pcs.RawValue;
        }

        public void Close()
        {
            if (this._pcs != null)
            {
                this._pcs.RemoveInstance();
                this._pcs.Close();
                this._pcs = null;
            }
        }

        public void Dispose()
        {
            this.Close();
            GC.SuppressFinalize(this);
        }

        ~STPInstancePerformanceCounter()
        {
            this.Close();
        }

        public virtual void Increment()
        {
            this._pcs.Increment();
        }

        public virtual void IncrementBy(long val)
        {
            this._pcs.IncrementBy(val);
        }

        public virtual void Set(long val)
        {
            this._pcs.RawValue = val;
        }
    }
}

