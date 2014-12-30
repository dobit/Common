namespace LFNet.Common.Threading
{
    /// <summary>
    /// Summary description for STPStartInfo.
    /// </summary>
    public class STPStartInfo : WIGStartInfo
    {
        /// <summary>
        /// Idle timeout in milliseconds.
        /// If a thread is idle for _idleTimeout milliseconds then 
        /// it may quit.
        /// </summary>
        private int _idleTimeout;
        /// <summary>
        /// The upper limit of threads in the pool.
        /// </summary>
        private int _maxWorkerThreads;
        /// <summary>
        /// The lower limit of threads in the pool.
        /// </summary>
        private int _minWorkerThreads;
        /// <summary>
        /// If this field is not null then the performance counters are enabled
        /// and use the string as the name of the instance.
        /// </summary>
        private string _pcInstanceName;
        /// <summary>
        /// The priority of the threads in the pool
        /// </summary>
        private global::System.Threading.ThreadPriority _threadPriority;

        public STPStartInfo()
        {
            this._idleTimeout = 0xea60;
            this._minWorkerThreads = 0;
            this._maxWorkerThreads = 0x19;
            this._threadPriority = global::System.Threading.ThreadPriority.Normal;
            this._pcInstanceName = SmartThreadPool.DefaultPerformanceCounterInstanceName;
        }

        public STPStartInfo(STPStartInfo stpStartInfo) : base(stpStartInfo)
        {
            this._idleTimeout = stpStartInfo._idleTimeout;
            this._minWorkerThreads = stpStartInfo._minWorkerThreads;
            this._maxWorkerThreads = stpStartInfo._maxWorkerThreads;
            this._threadPriority = stpStartInfo._threadPriority;
            this._pcInstanceName = stpStartInfo._pcInstanceName;
        }

        public int IdleTimeout
        {
            get
            {
                return this._idleTimeout;
            }
            set
            {
                this._idleTimeout = value;
            }
        }

        public int MaxWorkerThreads
        {
            get
            {
                return this._maxWorkerThreads;
            }
            set
            {
                this._maxWorkerThreads = value;
            }
        }

        public int MinWorkerThreads
        {
            get
            {
                return this._minWorkerThreads;
            }
            set
            {
                this._minWorkerThreads = value;
            }
        }

        public string PerformanceCounterInstanceName
        {
            get
            {
                return this._pcInstanceName;
            }
            set
            {
                this._pcInstanceName = value;
            }
        }

        public global::System.Threading.ThreadPriority ThreadPriority
        {
            get
            {
                return this._threadPriority;
            }
            set
            {
                this._threadPriority = value;
            }
        }
    }
}

