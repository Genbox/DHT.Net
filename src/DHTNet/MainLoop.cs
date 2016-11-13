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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DHTNet.Timeout;

namespace DHTNet
{
    public class MainLoop
    {
        private readonly TimeoutDispatcher _dispatcher = new TimeoutDispatcher();
        private readonly AutoResetEvent _handle = new AutoResetEvent(false);
        private readonly Queue<DelegateTask> _tasks = new Queue<DelegateTask>();
        private readonly Task _task;

        public MainLoop()
        {
            _task = Task.Factory.StartNew(Loop, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
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
                    _handle.WaitOne();
                else
                    task.Execute();
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
            Queue(new DelegateTask(task));
        }

        public void QueueWait(Action task)
        {
            QueueWait(new DelegateTask(task));
        }

        private void QueueWait(DelegateTask t)
        {
            t.WaitHandle.Reset();
            t.IsBlocking = true;
            if (Task.CurrentId == _task.Id)
                t.Execute();
            else
                Queue(t);

            t.WaitHandle.WaitOne();

            if (t.StoredException != null)
                throw new Exception("Exception in mainloop", t.StoredException);
        }

        public uint QueueTimeout(TimeSpan span, Func<bool> task)
        {
            DelegateTask dTask = new DelegateTask();
            dTask.Timeout = task;

            return _dispatcher.Add(span, (state, timeout) =>
            {
                QueueWait(dTask);
                return dTask.TimeoutResult;
            });
        }

        private class DelegateTask 
        {
            public DelegateTask()
            {
                WaitHandle = new ManualResetEvent(false);
            }

            public DelegateTask(Action task)
            {
                Task = task;
                WaitHandle = new ManualResetEvent(false);
            }

            public bool IsBlocking { get; set; }

            public Func<object> Job { get; set; }

            public Exception StoredException { get; private set; }

            public Action Task { get; set; }

            public Func<bool> Timeout { get; set; }

            public object JobResult { get; private set; }

            public bool TimeoutResult { get; private set; }

            public ManualResetEvent WaitHandle { get; }

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