using System;
using System.Threading;

namespace ECS.Tasks
{
    public sealed class GroupExecutor : ITaskExecutor
    {
        public event Action Completed;

        private readonly ITaskExecutor[] executors;
        private volatile bool isRunning;
        private int remainingTasks;

        public GroupExecutor(
                ITask[] tasks,
                Runner.SubtaskRunner runner,
                Utils.Logger logger = null,
                Profiler.Timeline profiler = null)
        {
            executors = new ITaskExecutor[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
                executors[i] = tasks[i].CreateExecutor(runner, logger, profiler);
        }

        public void QuerySubtasks()
        {
            for (int i = 0; i < executors.Length; i++)
                executors[i].QuerySubtasks();
        }

        public void RunSubtasks()
        {
            if (isRunning)
                throw new Exception($"[{nameof(GroupExecutor)}] Allready running!");
            isRunning = true;

            remainingTasks = executors.Length;
            if (remainingTasks == 0)
                Complete();
            else
            {
                for (int i = 0; i < executors.Length; i++)
                    executors[i].RunSubtasks();
            }
        }

        private void InnerTaskComplete()
        {
            if (Interlocked.Decrement(ref remainingTasks) == 0)
                Complete();
        }

        private void Complete()
        {
            isRunning = false;
            Completed?.Invoke();
        }
    }
}
