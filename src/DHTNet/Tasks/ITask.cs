using System;
using DHTNet.EventArgs;

namespace DHTNet.Tasks
{
    interface ITask
    {
        event EventHandler<TaskCompleteEventArgs> Completed;

        bool Active { get; }
        void Execute();
    }
}
