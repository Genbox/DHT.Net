using System;
using DHTNet.EventArgs;

namespace DHTNet.Tasks
{
    internal abstract class Task : ITask
    {
        public event EventHandler<TaskCompleteEventArgs> Completed;

        public bool Active { get; protected set; }

        public abstract void Execute();

        protected virtual void RaiseComplete(TaskCompleteEventArgs e)
        {
            EventHandler<TaskCompleteEventArgs> h = Completed;
            if (h != null)
                h(this, e);
        }
    }
}