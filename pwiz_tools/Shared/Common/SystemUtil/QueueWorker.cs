﻿/*
 * Original author: Don Marsh <donmarsh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2015 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace pwiz.Common.SystemUtil
{
    public class QueueWorker<TItem> where TItem : class
    {
        private readonly Func<int, TItem> _produce; 
        private readonly Action<TItem, int> _consume;
        private Thread[] _produceThreads;
        private Thread[] _consumeThreads;
        private BlockingCollection<TItem> _queue;
        private CountdownEvent _threadExit;
        private int _itemsWaiting;
        private readonly object _exceptionLock = new object();

        /// <summary>
        /// Construct a QueueWorker with optional "produce" and "consume" callback functions.
        /// </summary>
        /// <param name="produce">Function to produce a work item.</param>
        /// <param name="consume">Action to consume a work item.</param>
        public QueueWorker(Func<int, TItem> produce = null, Action<TItem, int> consume = null)
        {
            _produce = produce;
            _consume = consume;
        }

        public Exception Exception { get; private set; }

        /// <summary>
        /// Run this worker asynchronously, starting threads to consume work items.
        /// </summary>
        /// <param name="consumeThreads">How many threads to use to consume work items (0 to consume items synchronously).</param>
        /// <param name="consumeName">Name prefix for consumption threads (null for synchronous consumption).</param>
        /// <param name="maxQueueSize">Maximum number of work items to be queued at any time.</param>
        public void RunAsync(int consumeThreads, string consumeName, int maxQueueSize = -1)
        {
            RunAsync(consumeThreads, consumeName, 0, null, maxQueueSize);
        }

        /// <summary>
        /// Run this worker asynchronously, potentially starting threads to produce or consume
        /// work items (or both).
        /// </summary>
        /// <param name="consumeThreads">How many threads to use to consume work items (0 to consume items synchronously).</param>
        /// <param name="consumeName">Name prefix for consumption threads (null for synchronous consumption).</param>
        /// <param name="produceThreads">How many threads to use to produce work items (0 to produce items synchronously).</param>
        /// <param name="produceName">Name prefix for production threads (null for synchronous production).</param>
        /// <param name="maxQueueSize">Maximum number of work items to be queued at any time.</param>
        public void RunAsync(int consumeThreads, string consumeName, int produceThreads, string produceName, int maxQueueSize = -1)
        {
            // Create a queue and a number of threads to work on queued items.
            _queue = maxQueueSize > 0 ? new BlockingCollection<TItem>(maxQueueSize) : new BlockingCollection<TItem>();
            _threadExit = new CountdownEvent(consumeThreads);
            if (produceThreads > 0)
                _produceThreads = new Thread[produceThreads];
            if (consumeThreads > 0)
                _consumeThreads = new Thread[consumeThreads];
            for (int i = 0; i < produceThreads; i++)
            {
                _produceThreads[i] = new Thread(Produce)
                {
                    Name = produceThreads <= 1 ? produceName : produceName + " (" + (i + 1) + ")"    // Not L10N
                };
                _produceThreads[i].Start(i);
            }
            for (int i = 0; i < consumeThreads; i++)
            {
                _consumeThreads[i] = new Thread(Consume)
                {
                    Name = consumeThreads <= 1 ? consumeName : consumeName + " (" + (i + 1) + ")"    // Not L10N
                };
                _consumeThreads[i].Start(i);
            }
        }

        public bool IsRunningAsync { get { return _queue != null; } }

        /// <summary>
        /// Private code for production threads.
        /// </summary>
        private void Produce(object threadIndex)
        {
            LocalizationHelper.InitThread();

            try
            {
                // Add work items until there aren't any more.
                while (Exception == null)
                {
                    var item = _produce((int) threadIndex);
                    if (item == null)
                        break;
                    _queue.Add(item);
                }
            }
            catch (Exception ex)
            {
                SetException(ex);
            }

            DoneAdding();
        }

        /// <summary>
        /// Private code for consumption threads.
        /// </summary>
        /// <param name="threadIndex"></param>
        private void Consume(object threadIndex)
        {
            LocalizationHelper.InitThread();

            try
            {
                // Take queued items and process them, until the QueueWorker is stopped.
                while (Exception == null)
                {
                    var item = _queue.Take();
                    if (item == null)
                        break;
                    _consume(item, (int) threadIndex);
                    Interlocked.Decrement(ref _itemsWaiting);
                    lock (this)
                    {
                        Monitor.PulseAll(this);
                    }
                }
            }
            catch (Exception ex)
            {
                SetException(ex);
            }
            _threadExit.Signal();
        }

        private void SetException(Exception ex)
        {
            lock (_exceptionLock)
            {
                if (Exception == null)
                {
                    Exception = ex;

                    // Clear work queue.
                    TItem item;
                    while (_queue.TryTake(out item))
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Add an item to the work queue.
        /// </summary>
        public void Add(TItem item)
        {
            if (item == null)
                return;

            if (_consumeThreads == null)
                _consume(item, 0);
            else
            {
                Interlocked.Increment(ref _itemsWaiting);
                _queue.Add(item);
            }
        }

        /// <summary>
        /// Add a collection of items to the work queue, and optionally wait
        /// for them to be processed.  No subsequent calls to Add are
        /// expected.
        /// </summary>
        public void Add(ICollection<TItem> items, bool wait = false)
        {
            foreach (var item in items)
            {
                Add(item);
            }
            DoneAdding(wait);
        }

        /// <summary>
        /// Return an item from the work queue.
        /// </summary>
        public TItem Take()
        {
            var item = (_produceThreads == null) ? _produce(0) : _queue.Take();
            return item;
        }

        /// <summary>
        /// Wait for the work queue to empty.
        /// </summary>
        public void Wait()
        {
            if (_consumeThreads == null)
                return;
            lock (this)
            {
                while (_itemsWaiting != 0)
                    Monitor.Wait(this);
            }
        }

        /// <summary>
        /// Stop the threads after all items have been queued and processed.
        /// </summary>
        public void DoneAdding(bool wait = false)
        {
            if (_consumeThreads == null)
            {
                _queue.Add(null);
                return;
            }
            for (int i = 0; i < _consumeThreads.Length; i++)
                _queue.Add(null);
            if (wait)
                _threadExit.Wait();
        }
    }
}
