using System;
using DHTNet.EventArgs;

namespace DHTNet.Tasks
{
    internal abstract class Task : ITask
    {
        public event EventHandler<TaskCompleteEventArgs> Completed;

        private bool active;

        public bool Active
        {
            get { return active; }
            protected set { active = value; }
        }

        public abstract void Execute();

        protected virtual void RaiseComplete(TaskCompleteEventArgs e)
        {
            EventHandler<TaskCompleteEventArgs> h = Completed;
            if (h != null)
                h(this, e);
        }
    }
}
