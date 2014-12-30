using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>Provides a work-stealing scheduler.</summary>
    public class WorkStealingTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly int m_concurrencyLevel;
        private readonly Queue<Task> m_queue = new Queue<Task>();
        private WorkStealingQueue<Task>[] m_wsQueues = new WorkStealingQueue<Task>[Environment.ProcessorCount];
        private Lazy<Thread[]> m_threads;
        private int m_threadsWaiting;
        private bool m_shutdown;
        [ThreadStatic]
        private static WorkStealingQueue<Task> m_wsq;
        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
        public override int MaximumConcurrencyLevel
        {
            get
            {
                return this.m_concurrencyLevel;
            }
        }
        /// <summary>Initializes a new instance of the WorkStealingTaskScheduler class.</summary>
        /// <remarks>This constructors defaults to using twice as many threads as there are processors.</remarks>
        public WorkStealingTaskScheduler()
            : this(Environment.ProcessorCount * 2)
        {
        }
        /// <summary>Initializes a new instance of the WorkStealingTaskScheduler class.</summary>
        /// <param name="concurrencyLevel">The number of threads to use in the scheduler.</param>
        public WorkStealingTaskScheduler(int concurrencyLevel)
        {
            if (concurrencyLevel <= 0)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel");
            }
            this.m_concurrencyLevel = concurrencyLevel;
            this.m_threads = new Lazy<Thread[]>(delegate
            {
                Thread[] array = new Thread[this.m_concurrencyLevel];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new Thread(new ThreadStart(this.DispatchLoop))
                    {
                        IsBackground = true
                    };
                    array[i].Start();
                }
                return array;
            });
        }
        /// <summary>Queues a task to the scheduler.</summary>
        /// <param name="task">The task to be scheduled.</param>
        protected override void QueueTask(Task task)
        {
            this.m_threads.Force<Thread[]>();
            if ((task.CreationOptions & TaskCreationOptions.LongRunning) != TaskCreationOptions.None)
            {
                new Thread(delegate(object state)
                {
                    base.TryExecuteTask((Task)state);
                })
                {
                    IsBackground = true
                }.Start(task);
                return;
            }
            WorkStealingQueue<Task> wsq = WorkStealingTaskScheduler.m_wsq;
            if (wsq != null && (task.CreationOptions & TaskCreationOptions.PreferFairness) == TaskCreationOptions.None)
            {
                wsq.LocalPush(task);
                if (this.m_threadsWaiting <= 0)
                {
                    return;
                }
                lock (this.m_queue)
                {
                    Monitor.Pulse(this.m_queue);
                    return;
                }
            }
            lock (this.m_queue)
            {
                this.m_queue.Enqueue(task);
                if (this.m_threadsWaiting > 0)
                {
                    Monitor.Pulse(this.m_queue);
                }
            }
        }
        /// <summary>Executes a task on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Ignored.</param>
        /// <returns>Whether the task could be executed.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return base.TryExecuteTask(task);
        }
        /// <summary>Gets all of the tasks currently scheduled to this scheduler.</summary>
        /// <returns>An enumerable containing all of the scheduled tasks.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            List<Task> list = new List<Task>();
            bool flag = false;
            try
            {
                Monitor.TryEnter(this.m_queue, ref flag);
                if (!flag)
                {
                    throw new NotSupportedException();
                }
                list.AddRange(this.m_queue.ToArray());
            }
            finally
            {
                if (flag)
                {
                    Monitor.Exit(this.m_queue);
                }
            }
            WorkStealingQueue<Task>[] wsQueues = this.m_wsQueues;
            for (int i = 0; i < wsQueues.Length; i++)
            {
                WorkStealingQueue<Task> workStealingQueue = wsQueues[i];
                if (workStealingQueue != null)
                {
                    list.AddRange(workStealingQueue.ToArray());
                }
            }
            return list;
        }
        /// <summary>Adds a work-stealing queue to the set of queues.</summary>
        /// <param name="wsq">The queue to be added.</param>
        private void AddWsq(WorkStealingQueue<Task> wsq)
        {
            lock (this.m_wsQueues)
            {
                int i;
                for (i = 0; i < this.m_wsQueues.Length; i++)
                {
                    if (this.m_wsQueues[i] == null)
                    {
                        this.m_wsQueues[i] = wsq;
                        return;
                    }
                }
                WorkStealingQueue<Task>[] array = new WorkStealingQueue<Task>[i * 2];
                Array.Copy(this.m_wsQueues, array, i);
                array[i] = wsq;
                this.m_wsQueues = array;
            }
        }
        /// <summary>Remove a work-stealing queue from the set of queues.</summary>
        /// <param name="wsq">The work-stealing queue to remove.</param>
        private void RemoveWsq(WorkStealingQueue<Task> wsq)
        {
            lock (this.m_wsQueues)
            {
                for (int i = 0; i < this.m_wsQueues.Length; i++)
                {
                    if (this.m_wsQueues[i] == wsq)
                    {
                        this.m_wsQueues[i] = null;
                    }
                }
            }
        }
        /// <summary>
        /// The dispatch loop run by each thread in the scheduler.
        /// </summary>
        private void DispatchLoop()
        {
            WorkStealingQueue<Task> workStealingQueue = new WorkStealingQueue<Task>();
            WorkStealingTaskScheduler.m_wsq = workStealingQueue;
            this.AddWsq(workStealingQueue);
            try
            {
                while (true)
                {
                    Task task = null;
                    if (!workStealingQueue.LocalPop(ref task))
                    {
                        bool flag = false;
                        while (true)
                        {
                            lock (this.m_queue)
                            {
                                if (this.m_shutdown)
                                {
                                    return;
                                }
                                if (this.m_queue.Count != 0)
                                {
                                    task = this.m_queue.Dequeue();
                                    break;
                                }
                                if (flag)
                                {
                                    this.m_threadsWaiting++;
                                    try
                                    {
                                        Monitor.Wait(this.m_queue);
                                    }
                                    finally
                                    {
                                        this.m_threadsWaiting--;
                                    }
                                    if (this.m_shutdown)
                                    {
                                        return;
                                    }
                                    flag = false;
                                    continue;
                                }
                            }
                            WorkStealingQueue<Task>[] wsQueues = this.m_wsQueues;
                            int i;
                            for (i = 0; i < wsQueues.Length; i++)
                            {
                                WorkStealingQueue<Task> workStealingQueue2 = wsQueues[i];
                                if (workStealingQueue2 != null && workStealingQueue2 != workStealingQueue && workStealingQueue2.TrySteal(ref task))
                                {
                                    break;
                                }
                            }
                            if (i != wsQueues.Length)
                            {
                                break;
                            }
                            flag = true;
                        }
                    }
                    base.TryExecuteTask(task);
                }
            }
            finally
            {
                this.RemoveWsq(workStealingQueue);
            }
        }
        /// <summary>Signal the scheduler to shutdown and wait for all threads to finish.</summary>
        public void Dispose()
        {
            this.m_shutdown = true;
            if (this.m_queue != null && this.m_threads.IsValueCreated)
            {
                Thread[] value = this.m_threads.Value;
                lock (this.m_queue)
                {
                    Monitor.PulseAll(this.m_queue);
                }
                for (int i = 0; i < value.Length; i++)
                {
                    value[i].Join();
                }
            }
        }
    }
}
