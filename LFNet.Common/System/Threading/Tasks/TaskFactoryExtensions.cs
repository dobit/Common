using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>Extensions for TaskFactory.</summary>
    /// <summary>Extensions for TaskFactory.</summary>
    /// <summary>Extensions for TaskFactory.</summary>
    public static class TaskFactoryExtensions
    {
        /// <summary>Converts TaskCreationOptions into TaskContinuationOptions.</summary>
        /// <param name="creationOptions"></param>
        /// <returns></returns>
        private static TaskContinuationOptions ContinuationOptionsFromCreationOptions(TaskCreationOptions creationOptions)
        {
            return (((((TaskContinuationOptions) creationOptions) & TaskContinuationOptions.AttachedToParent) | (((TaskContinuationOptions) creationOptions) & TaskContinuationOptions.PreferFairness)) | (((TaskContinuationOptions) creationOptions) & TaskContinuationOptions.LongRunning));
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(this TaskFactory factory, Action action)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return new Task(action, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory factory, Func<TResult> function)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return new Task<TResult>(function, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory<TResult> factory, Func<TResult> function)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return new Task<TResult>(function, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(this TaskFactory factory, Action action, TaskCreationOptions creationOptions)
        {
            return new Task(action, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(this TaskFactory factory, Action<object> action, object state)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return new Task(action, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory factory, Func<TResult> function, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory factory, Func<object, TResult> function, object state)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return new Task<TResult>(function, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory<TResult> factory, Func<TResult> function, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory<TResult> factory, Func<object, TResult> function, object state)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return new Task<TResult>(function, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(this TaskFactory factory, Action<object> action, object state, TaskCreationOptions creationOptions)
        {
            return new Task(action, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory factory, Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory<TResult> factory, Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task that will be completed when the specified WaitHandle is signaled.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="waitHandle">The WaitHandle.</param>
        /// <returns>The created Task.</returns>
        public static Task FromAsync(this TaskFactory factory, WaitHandle waitHandle)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (waitHandle == null)
            {
                throw new ArgumentNullException("waitHandle");
            }
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, delegate {
                tcs.TrySetResult(null);
            }, null, -1, true);
            Task<object> task = tcs.Task;
            task.ContinueWith<bool>(_ => rwh.Unregister(null), TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        /// <summary>Creates a Task that has completed in the Canceled state with the specified CancellationToken.</summary>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="cancellationToken">The CancellationToken with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task FromCancellation(this TaskFactory factory, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                throw new ArgumentOutOfRangeException("cancellationToken");
            }
            return new Task(delegate {
            }, cancellationToken);
        }

        /// <summary>Creates a Task that has completed in the Canceled state with the specified CancellationToken.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="cancellationToken">The CancellationToken with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromCancellation<TResult>(this TaskFactory factory, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                throw new ArgumentOutOfRangeException("cancellationToken");
            }
            return new Task<TResult>(DelegateCache<TResult>.DefaultResult, cancellationToken);
        }

        /// <summary>Creates a Task that has completed in the Canceled state with the specified CancellationToken.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="cancellationToken">The CancellationToken with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromCancellation<TResult>(this TaskFactory<TResult> factory, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                throw new ArgumentOutOfRangeException("cancellationToken");
            }
            return new Task<TResult>(DelegateCache<TResult>.DefaultResult, cancellationToken);
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task FromException(this TaskFactory factory, Exception exception)
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>(factory.CreationOptions);
            source.SetException(exception);
            return source.Task;
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromException<TResult>(this TaskFactory factory, Exception exception)
        {
            TaskCompletionSource<TResult> source = new TaskCompletionSource<TResult>(factory.CreationOptions);
            source.SetException(exception);
            return source.Task;
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromException<TResult>(this TaskFactory<TResult> factory, Exception exception)
        {
            TaskCompletionSource<TResult> source = new TaskCompletionSource<TResult>(factory.CreationOptions);
            source.SetException(exception);
            return source.Task;
        }

        /// <summary>Creates a Task that has completed in the RanToCompletion state with the specified result.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="result">The result with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromResult<TResult>(this TaskFactory factory, TResult result)
        {
            TaskCompletionSource<TResult> source = new TaskCompletionSource<TResult>(factory.CreationOptions);
            source.SetResult(result);
            return source.Task;
        }

        /// <summary>Creates a Task that has completed in the RanToCompletion state with the specified result.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="result">The result with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromResult<TResult>(this TaskFactory<TResult> factory, TResult result)
        {
            TaskCompletionSource<TResult> source = new TaskCompletionSource<TResult>(factory.CreationOptions);
            source.SetResult(result);
            return source.Task;
        }

        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        public static TaskScheduler GetTargetScheduler(this TaskFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return (factory.Scheduler ?? TaskScheduler.Current);
        }

        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        public static TaskScheduler GetTargetScheduler<TResult>(this TaskFactory<TResult> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (factory.Scheduler == null)
            {
                return TaskScheduler.Current;
            }
            return factory.Scheduler;
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.Iterate(source, null, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.Iterate(source, state, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, CancellationToken cancellationToken)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.Iterate(source, null, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, TaskCreationOptions creationOptions)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.Iterate(source, null, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, TaskScheduler scheduler)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.Iterate(source, null, factory.CancellationToken, factory.CreationOptions, scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state, CancellationToken cancellationToken)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.Iterate(source, state, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state, TaskCreationOptions creationOptions)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.Iterate(source, state, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state, TaskScheduler scheduler)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.Iterate(source, state, factory.CancellationToken, factory.CreationOptions, scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.Iterate(source, null, cancellationToken, creationOptions, scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (source == null)
            {
                throw new ArgumentNullException("asyncIterator");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            IEnumerator<object> enumerator = source.GetEnumerator();
            if (enumerator == null)
            {
                throw new InvalidOperationException("Invalid enumerable - GetEnumerator returned null");
            }
            TaskCompletionSource<object> trs = new TaskCompletionSource<object>(state, creationOptions);
            trs.Task.ContinueWith(delegate (Task<object> _) {
                enumerator.Dispose();
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            Action<Task> recursiveBody = null;
            recursiveBody = delegate (Task antecedent) {
                Action action = null;
                try
                {
                    if (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is Task)
                        {
                            Task task = (Task) current;
                            task.IgnoreExceptions();
                            task.ContinueWith(recursiveBody).IgnoreExceptions();
                        }
                        else if (current is TaskScheduler)
                        {
                            if (action == null)
                            {
                                action = () => recursiveBody(null);
                            }
                            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, (TaskScheduler) current).IgnoreExceptions();
                        }
                        else
                        {
                            trs.TrySetException(new InvalidOperationException("Task or TaskScheduler object expected in Iterate"));
                        }
                    }
                    else
                    {
                        trs.TrySetResult(null);
                    }
                }
                catch (Exception exception)
                {
                    OperationCanceledException exception2 = exception as OperationCanceledException;
                    if ((exception2 != null) && (exception2.CancellationToken == cancellationToken))
                    {
                        trs.TrySetCanceled();
                    }
                    else
                    {
                        trs.TrySetException(exception);
                    }
                }
            };
            factory.StartNew(delegate {
                recursiveBody(null);
            }, CancellationToken.None, TaskCreationOptions.None, scheduler).IgnoreExceptions();
            return trs.Task;
        }

        /// <summary>Creates a Task that will complete after the specified delay.</summary>
        /// <param name="factory">The TaskFactory.</param>
        /// <param name="millisecondsDelay">The delay after which the Task should transition to RanToCompletion.</param>
        /// <returns>A Task that will be completed after the specified duration.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay)
        {
            return factory.StartNewDelayed(millisecondsDelay, CancellationToken.None);
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action action)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed(millisecondsDelay, action, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates a Task that will complete after the specified delay.</summary>
        /// <param name="factory">The TaskFactory.</param>
        /// <param name="millisecondsDelay">The delay after which the Task should transition to RanToCompletion.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the timed task.</param>
        /// <returns>A Task that will be completed after the specified duration and that's cancelable with the specified token.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, CancellationToken cancellationToken)
        {
            Action callback = null;
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (millisecondsDelay < 0)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay");
            }
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(factory.CreationOptions);
            CancellationTokenRegistration ctr = new CancellationTokenRegistration();
            Timer timer = new Timer(delegate (object self) {
                ctr.Dispose();
                ((Timer) self).Dispose();
                tcs.TrySetResult(null);
            });
            if (cancellationToken.CanBeCanceled)
            {
                if (callback == null)
                {
                    callback = delegate {
                        timer.Dispose();
                        tcs.TrySetCanceled();
                    };
                }
                ctr = cancellationToken.Register(callback);
            }
            timer.Change(millisecondsDelay, -1);
            return tcs.Task;
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay, Func<TResult> function)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action action, CancellationToken cancellationToken)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed(millisecondsDelay, action, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action action, TaskCreationOptions creationOptions)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed(millisecondsDelay, action, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action<object> action, object state)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed(millisecondsDelay, action, state, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay, Func<TResult> function, CancellationToken cancellationToken)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay, Func<TResult> function, TaskCreationOptions creationOptions)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, factory.CancellationToken, creationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay, Func<object, TResult> function, object state)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, state, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action<object> action, object state, CancellationToken cancellationToken)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed(millisecondsDelay, action, state, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action<object> action, object state, TaskCreationOptions creationOptions)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed(millisecondsDelay, action, state, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay, Func<object, TResult> function, object state, CancellationToken cancellationToken)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, state, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay, Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, state, factory.CancellationToken, creationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (millisecondsDelay < 0)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            return factory.StartNewDelayed(millisecondsDelay, cancellationToken).ContinueWith(delegate (Task _) {
                action();
            }, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay, Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (millisecondsDelay < 0)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay");
            }
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            TaskCompletionSource<object> state = new TaskCompletionSource<object>();
            Timer timer = new Timer(delegate (object obj) {
                ((TaskCompletionSource<object>) obj).SetResult(null);
            }, state, millisecondsDelay, -1);
            return state.Task.ContinueWith<TResult>(delegate (Task<object> _) {
                timer.Dispose();
                return function();
            }, cancellationToken, ContinuationOptionsFromCreationOptions(creationOptions), scheduler);
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (millisecondsDelay < 0)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            TaskCompletionSource<object> result = new TaskCompletionSource<object>(state);
            factory.StartNewDelayed(millisecondsDelay, cancellationToken).ContinueWith(delegate (Task t) {
                if (t.IsCanceled)
                {
                    result.TrySetCanceled();
                }
                else
                {
                    try
                    {
                        action(state);
                        result.TrySetResult(null);
                    }
                    catch (Exception exception)
                    {
                        result.TrySetException(exception);
                    }
                }
            }, scheduler);
            return result.Task;
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay, Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (millisecondsDelay < 0)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay");
            }
            if (function == null)
            {
                throw new ArgumentNullException("action");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            TaskCompletionSource<TResult> result = new TaskCompletionSource<TResult>(state);
            Timer timer = null;
            Task<TResult> task = new Task<TResult>(function, state, creationOptions);
            task.ContinueWith(delegate (Task<TResult> t) {
                result.SetFromTask<TResult>(t);
                timer.Dispose();
            }, cancellationToken, ContinuationOptionsFromCreationOptions(creationOptions) | TaskContinuationOptions.ExecuteSynchronously, scheduler);
            timer = new Timer(delegate (object obj) {
                ((Task) obj).Start(scheduler);
            }, task, millisecondsDelay, -1);
            return result.Task;
        }

        /// <summary>Creates a generic TaskFactory from a non-generic one.</summary>
        /// <typeparam name="TResult">Specifies the type of Task results for the Tasks created by the new TaskFactory.</typeparam>
        /// <param name="factory">The TaskFactory to serve as a template.</param>
        /// <returns>The created TaskFactory.</returns>
        public static TaskFactory<TResult> ToGeneric<TResult>(this TaskFactory factory)
        {
            return new TaskFactory<TResult>(factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>Creates a generic TaskFactory from a non-generic one.</summary>
        /// <typeparam name="TResult">Specifies the type of Task results for the Tasks created by the new TaskFactory.</typeparam>
        /// <param name="factory">The TaskFactory to serve as a template.</param>
        /// <returns>The created TaskFactory.</returns>
        public static TaskFactory ToNonGeneric<TResult>(this TaskFactory<TResult> factory)
        {
            return new TaskFactory(factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>Asynchronously executes a sequence of tasks, maintaining a list of all tasks processed.</summary>
        /// <param name="factory">The TaskFactory to use to create the task.</param>
        /// <param name="functions">
        /// The functions that generate the tasks through which to iterate sequentially.
        /// Iteration will cease if a task faults.
        /// </param>
        /// <returns>A Task that will return the list of tracked tasks iterated.</returns>
        public static Task<IList<Task>> TrackedSequence(this TaskFactory factory, params Func<Task>[] functions)
        {
            TaskCompletionSource<IList<Task>> tcs = new TaskCompletionSource<IList<Task>>();
            factory.Iterate(TrackedSequenceInternal(functions, tcs));
            return tcs.Task;
        }

        /// <summary>Creates the enumerable to iterate through with Iterate.</summary>
        /// <param name="functions">
        /// The functions that generate the tasks through which to iterate sequentially.
        /// Iteration will cease if a task faults.
        /// </param>
        /// <param name="tcs">The TaskCompletionSource to resolve with the asynchronous results.</param>
        /// <returns>The enumerable through which to iterate.</returns>
        private static IEnumerable<Task> TrackedSequenceInternal(IEnumerable<Func<Task>> functions, TaskCompletionSource<IList<Task>> tcs)
        {
            List<Task> result = new List<Task>();
            foreach (Func<Task> iteratorVariable1 in functions)
            {
                Task item = null;
                try
                {
                    item = iteratorVariable1();
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
                if (item == null)
                {
                    break;
                }
                result.Add(item);
                yield return item;
                if (item.IsFaulted)
                {
                    break;
                }
            }
            tcs.TrySetResult(result);
        }

        /// <summary>
        /// Creates a continuation Task that will compplete upon
        /// the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the array of completed tasks.</returns>
        public static Task<Task[]> WhenAll(this TaskFactory factory, params Task[] tasks)
        {
            return factory.ContinueWhenAll<Task[]>(tasks, completedTasks => completedTasks);
        }

        /// <summary>
        /// Creates a continuation Task that will compplete upon
        /// the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the array of completed tasks.</returns>
        public static Task<Task<TAntecedentResult>[]> WhenAll<TAntecedentResult>(this TaskFactory factory, params Task<TAntecedentResult>[] tasks)
        {
            return factory.ContinueWhenAll<TAntecedentResult, Task<TAntecedentResult>[]>(tasks, completedTasks => completedTasks);
        }

        /// <summary>
        /// Creates a continuation Task that will complete upon
        /// the completion of any one of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the completed task.</returns>
        public static Task<Task> WhenAny(this TaskFactory factory, params Task[] tasks)
        {
            return factory.ContinueWhenAny<Task>(tasks, completedTask => completedTask);
        }

        /// <summary>
        /// Creates a continuation Task that will complete upon
        /// the completion of any one of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the completed task.</returns>
        public static Task<Task<TAntecedentResult>> WhenAny<TAntecedentResult>(this TaskFactory factory, params Task<TAntecedentResult>[] tasks)
        {
            return factory.ContinueWhenAny<TAntecedentResult, Task<TAntecedentResult>>(tasks, completedTask => completedTask);
        }


        /// <summary>A cache of delegates.</summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        private class DelegateCache<TResult>
        {
            /// <summary>Function that returns default(TResult).</summary>
            internal static readonly Func<TResult> DefaultResult;

            static DelegateCache()
            {
                TaskFactoryExtensions.DelegateCache<TResult>.DefaultResult = () => default(TResult);
            }
        }
    }
}

