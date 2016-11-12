using DHTNet.Tasks;

namespace DHTNet.EventArgs
{
    internal class TaskCompleteEventArgs : System.EventArgs
    {
        public TaskCompleteEventArgs(Task task)
        {
            Task = task;
        }

        public Task Task { get; protected internal set; }
    }
}