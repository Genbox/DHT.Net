using System;
using DHTNet.EventArgs;

namespace DHTNet.Tasks
{
    internal abstract class Task 
    {
        public event EventHandler<TaskCompleteEventArgs> Completed;

        public bool Active { get; protected set; }

        public abstract void Execute();

        protected virtual void RaiseComplete(TaskCompleteEventArgs e)
        {
            Completed?.Invoke(this, e);
        }
    }
}