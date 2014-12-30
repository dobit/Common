using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>Provides a TaskScheduler that uses an I/O completion port for concurrency control.</summary>
    public sealed class IOCompletionPortTaskScheduler : TaskScheduler, IDisposable
    {
        /// <summary>The I/O completion port to use for concurrency control.</summary>
        private readonly IOCompletionPort m_iocp;
        /// <summary>Event used to wait for all threads to shutdown.</summary>
        private CountdownEvent m_remainingThreadsToShutdown;
        /// <summary>Whether the current thread is a scheduler thread.</summary>
        private ThreadLocal<bool> m_schedulerThread;
        /// <summary>The queue of tasks to be scheduled.</summary>
        private readonly ConcurrentQueue<Task> m_tasks;

        /// <summary>Initializes the IOCompletionPortTaskScheduler.</summary>
        /// <param name="maxConcurrencyLevel">The maximum number of threads in the scheduler to be executing concurrently.</param>
        /// <param name="numAvailableThreads">The number of threads to have available in the scheduler for executing tasks.</param>
        public IOCompletionPortTaskScheduler(int maxConcurrencyLevel, int numAvailableThreads)
        {
            ThreadStart start = null;
            if (maxConcurrencyLevel < 1)
            {
                throw new ArgumentNullException("maxConcurrencyLevel");
            }
            if (numAvailableThreads < 1)
            {
                throw new ArgumentNullException("numAvailableThreads");
            }
            this.m_tasks = new ConcurrentQueue<Task>();
            this.m_iocp = new IOCompletionPort(maxConcurrencyLevel);
            this.m_schedulerThread = new ThreadLocal<bool>();
            this.m_remainingThreadsToShutdown = new CountdownEvent(numAvailableThreads);
            for (int i = 0; i < numAvailableThreads; i++)
            {
                if (start == null)
                {
                    start = delegate {
                        try
                        {
                            this.m_schedulerThread.Value = true;
                            while (this.m_iocp.WaitOne())
                            {
                                Task task;
                                if (this.m_tasks.TryDequeue(out task))
                                {
                                    base.TryExecuteTask(task);
                                }
                            }
                        }
                        finally
                        {
                            this.m_remainingThreadsToShutdown.Signal();
                        }
                    };
                }
                new Thread(start) { IsBackground = true }.Start();
            }
        }

        /// <summary>Dispose of the scheduler.</summary>
        public void Dispose()
        {
            this.m_iocp.Dispose();
            this.m_remainingThreadsToShutdown.Wait();
            this.m_remainingThreadsToShutdown.Dispose();
            this.m_schedulerThread.Dispose();
        }

        /// <summary>Gets a list of all tasks scheduled to this scheduler.</summary>
        /// <returns>An enumerable of all scheduled tasks.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return this.m_tasks.ToArray();
        }

        /// <summary>Queues a task to this scheduler for execution.</summary>
        /// <param name="task">The task to be executed.</param>
        protected override void QueueTask(Task task)
        {
            this.m_tasks.Enqueue(task);
            this.m_iocp.NotifyOne();
        }

        /// <summary>Try to execute a task on the current thread.</summary>
        /// <param name="task">The task to execute.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued to this scheduler.</param>
        /// <returns>Whether the task was executed.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return (this.m_schedulerThread.Value && base.TryExecuteTask(task));
        }

        /// <summary>Provides a simple managed wrapper for an I/O completion port.</summary>
        private sealed class IOCompletionPort : IDisposable
        {
            /// <summary>Infinite timeout value to use for GetQueuedCompletedStatus.</summary>
            private uint INFINITE_TIMEOUT = uint.MaxValue;
            /// <summary>An invalid file handle value.</summary>
            private IntPtr INVALID_FILE_HANDLE = ((IntPtr) (-1));
            /// <summary>An invalid I/O completion port handle value.</summary>
            private IntPtr INVALID_IOCP_HANDLE = IntPtr.Zero;
            /// <summary>The I/O completion porth handle.</summary>
            private SafeFileHandle m_handle;

            /// <summary>Initializes the I/O completion port.</summary>
            /// <param name="maxConcurrencyLevel">The maximum concurrency level allowed by the I/O completion port.</param>
            public IOCompletionPort(int maxConcurrencyLevel)
            {
                if (maxConcurrencyLevel < 1)
                {
                    throw new ArgumentOutOfRangeException("maxConcurrencyLevel");
                }
                this.m_handle = CreateIoCompletionPort(this.INVALID_FILE_HANDLE, this.INVALID_IOCP_HANDLE, UIntPtr.Zero, (uint) maxConcurrencyLevel);
            }

            /// <summary>
            /// Creates an input/output (I/O) completion port and associates it with a specified file handle, 
            /// or creates an I/O completion port that is not yet associated with a file handle, allowing association at a later time.
            /// </summary>
            /// <param name="fileHandle">An open file handle or INVALID_HANDLE_VALUE.</param>
            /// <param name="existingCompletionPort">A handle to an existing I/O completion port or NULL.</param>
            /// <param name="completionKey">The per-handle user-defined completion key that is included in every I/O completion packet for the specified file handle.</param>
            /// <param name="numberOfConcurrentThreads">The maximum number of threads that the operating system can allow to concurrently process I/O completion packets for the I/O completion port.</param>
            /// <returns>If the function succeeds, the return value is the handle to an I/O completion port.  If the function fails, the return value is NULL.</returns>
            [DllImport("kernel32.dll", SetLastError=true)]
            private static extern SafeFileHandle CreateIoCompletionPort(IntPtr fileHandle, IntPtr existingCompletionPort, UIntPtr completionKey, uint numberOfConcurrentThreads);
            /// <summary>Clean up.</summary>
            public void Dispose()
            {
                this.m_handle.Dispose();
            }

            /// <summary>Attempts to dequeue an I/O completion packet from the specified I/O completion port.</summary>
            /// <param name="completionPort">A handle to the completion port.</param>
            /// <param name="lpNumberOfBytes">A pointer to a variable that receives the number of bytes transferred during an I/O operation that has completed.</param>
            /// <param name="lpCompletionKey">A pointer to a variable that receives the completion key value associated with the file handle whose I/O operation has completed.</param>
            /// <param name="lpOverlapped">A pointer to a variable that receives the address of the OVERLAPPED structure that was specified when the completed I/O operation was started.</param>
            /// <param name="dwMilliseconds">The number of milliseconds that the caller is willing to wait for a completion packet to appear at the completion port. </param>
            /// <returns>Returns nonzero (TRUE) if successful or zero (FALSE) otherwise.</returns>
            [DllImport("kernel32.dll", SetLastError=true)]
            private static extern bool GetQueuedCompletionStatus(IntPtr completionPort, out uint lpNumberOfBytes, out IntPtr lpCompletionKey, out IntPtr lpOverlapped, uint dwMilliseconds);
            /// <summary>Notify that I/O completion port that new work is available.</summary>
            public void NotifyOne()
            {
                if (!PostQueuedCompletionStatus(this.m_handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
                {
                    throw new Win32Exception();
                }
            }

            /// <summary>Posts an I/O completion packet to an I/O completion port.</summary>
            /// <param name="completionPort">A handle to the completion port.</param>
            /// <param name="dwNumberOfBytesTransferred">The value to be returned through the lpNumberOfBytesTransferred parameter of the GetQueuedCompletionStatus function.</param>
            /// <param name="dwCompletionKey">The value to be returned through the lpCompletionKey parameter of the GetQueuedCompletionStatus function.</param>
            /// <param name="lpOverlapped">The value to be returned through the lpOverlapped parameter of the GetQueuedCompletionStatus function.</param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
            [DllImport("kernel32.dll", SetLastError=true)]
            private static extern bool PostQueuedCompletionStatus(SafeFileHandle completionPort, IntPtr dwNumberOfBytesTransferred, IntPtr dwCompletionKey, IntPtr lpOverlapped);
            /// <summary>Waits for an item on the I/O completion port.</summary>
            /// <returns>true if an item was available; false if the completion port closed before an item could be retrieved.</returns>
            public bool WaitOne()
            {
                uint num;
                IntPtr ptr;
                IntPtr ptr2;
                if (GetQueuedCompletionStatus(this.m_handle.DangerousGetHandle(), out num, out ptr, out ptr2, this.INFINITE_TIMEOUT))
                {
                    return true;
                }
                int error = Marshal.GetLastWin32Error();
                if ((error != 0x2df) && (error != 6))
                {
                    throw new Win32Exception(error);
                }
                return false;
            }
        }
    }
}

