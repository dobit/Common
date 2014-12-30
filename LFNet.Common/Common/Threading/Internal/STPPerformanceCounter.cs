using System.Diagnostics;

namespace LFNet.Common.Threading.Internal
{
    /// <summary>
    /// Summary description for STPPerformanceCounter.
    /// </summary>
    internal class STPPerformanceCounter
    {
        protected string _counterHelp;
        protected string _counterName;
        private PerformanceCounterType _pcType;

        public STPPerformanceCounter(string counterName, string counterHelp, PerformanceCounterType pcType)
        {
            this._counterName = counterName;
            this._counterHelp = counterHelp;
            this._pcType = pcType;
        }

        public void AddCounterToCollection(CounterCreationDataCollection counterData)
        {
            CounterCreationData data = new CounterCreationData(this._counterName, this._counterHelp, this._pcType);
            counterData.Add(data);
        }

        public string Name
        {
            get
            {
                return this._counterName;
            }
        }
    }
}

