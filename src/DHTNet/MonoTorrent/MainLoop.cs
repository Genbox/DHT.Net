//
// MainLoop.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2008 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections.Generic;
using System.Threading;

namespace DHTNet.MonoTorrent
{
    public class MainLoop
    {
        private readonly ICache<DelegateTask> _cache = new Cache<DelegateTask>(true).Synchronize();

        private readonly TimeoutDispatcher _dispatcher = new TimeoutDispatcher();
        private readonly AutoResetEvent _handle = new AutoResetEvent(false);
        private readonly Queue<DelegateTask> _tasks = new Queue<DelegateTask>();
        private readonly Thread _thread;

        public MainLoop(string name)
        {
            _thread = new Thread(Loop);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Loop()
        {
            while (true)
            {
                DelegateTask task = null;

                lock (_tasks)
                {
                    if (_tasks.Count > 0)
                        task = _tasks.Dequeue();
                }

                if (task == null)
                {
                    _handle.WaitOne();
                }
                else
                {
                    bool reuse = !task.IsBlocking;
                    task.Execute();
                    if (reuse)
                        _cache.Enqueue(task);
                }
            }
        }

        private void Queue(DelegateTask task)
        {
            lock (_tasks)
            {
                _tasks.Enqueue(task);
                _handle.Set();
            }
        }

        public void Queue(Action task)
        {
            DelegateTask dTask = _cache.Dequeue();
            dTask.Task = task;
            Queue(dTask);
        }

        public void QueueWait(Action task)
        {
            DelegateTask dTask = _cache.Dequeue();
            dTask.Task = task;
            try
            {
                QueueWait(dTask);
            }
            finally
            {
                _cache.Enqueue(dTask);
            }
        }

        public object QueueWait(Func<object> task)
        {
            DelegateTask dTask = _cache.Dequeue();
            dTask.Job = task;

            try
            {
                QueueWait(dTask);
                return dTask.JobResult;
            }
            finally
            {
                _cache.Enqueue(dTask);
            }
        }

        private void QueueWait(DelegateTask t)
        {
            t.WaitHandle.Reset();
            t.IsBlocking = true;
            if (Thread.CurrentThread == _thread)
                t.Execute();
            else
                Queue(t);

            t.WaitHandle.WaitOne();

            if (t.StoredException != null)
                throw new Exception("Exception in mainloop", t.StoredException);
        }

        public uint QueueTimeout(TimeSpan span, Func<bool> task)
        {
            DelegateTask dTask = _cache.Dequeue();
            dTask.Timeout = task;

            return _dispatcher.Add(span, delegate
            {
                QueueWait(dTask);
                return dTask.TimeoutResult;
            });
        }

        public AsyncCallback Wrap(AsyncCallback callback)
        {
            return delegate(IAsyncResult result) { Queue(delegate { callback(result); }); };
        }

        private class DelegateTask : ICacheable
        {
            public DelegateTask()
            {
                WaitHandle = new ManualResetEvent(false);
            }

            public bool IsBlocking { get; set; }

            public Func<object> Job { get; set; }

            public Exception StoredException { get; set; }

            public Action Task { get; set; }

            public Func<bool> Timeout { get; set; }

            public object JobResult { get; private set; }

            public bool TimeoutResult { get; private set; }

            public ManualResetEvent WaitHandle { get; }

            public void Initialise()
            {
                IsBlocking = false;
                Job = null;
                JobResult = null;
                StoredException = null;
                Task = null;
                Timeout = null;
                TimeoutResult = false;
            }

            public void Execute()
            {
                try
                {
                    if (Job != null)
                        JobResult = Job();
                    else if (Task != null)
                        Task();
                    else if (Timeout != null)
                        TimeoutResult = Timeout();
                }
                catch (Exception ex)
                {
                    StoredException = ex;

                    // FIXME: I assume this case can't happen. The only user interaction
                    // with the mainloop is with blocking tasks. Internally it's a big bug
                    // if i allow an exception to propagate to the mainloop.
                    if (!IsBlocking)
                        throw;
                }
                finally
                {
                    WaitHandle.Set();
                }
            }
        }
    }
}