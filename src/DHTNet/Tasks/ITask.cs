using System;
using DHTNet.EventArgs;

namespace DHTNet.Tasks
{
    internal interface ITask
    {
        bool Active { get; }
        event EventHandler<TaskCompleteEventArgs> Completed;
        void Execute();
    }
}