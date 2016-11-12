#if !DISABLE_DHT
using DHTNet.Tasks;

namespace DHTNet.EventArgs
{
    class TaskCompleteEventArgs : System.EventArgs
    {
        private Task task;

        public Task Task
        {
            get { return task; }
            protected internal set { task = value; }
        }

        public TaskCompleteEventArgs(Task task)
        {
            this.task = task;
        }
    }
}
#endif