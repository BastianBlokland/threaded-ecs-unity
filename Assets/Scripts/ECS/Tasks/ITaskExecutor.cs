using System;

namespace ECS.Tasks
{
    public interface ITaskExecutor
    {
        event Action Completed;

        void QuerySubtasks();
        void RunSubtasks();
    }
}
