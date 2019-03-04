using System;
using System.Threading;

namespace ECS.Tasks
{
    public sealed class TaskQuerier : Runner.ExecuteInfo.ISubtaskExecutor
    {
        public event Action Completed;

        private readonly Runner.SubtaskRunner runner;
        private readonly ITaskExecutor[] tasks;
        private readonly Utils.Logger logger;
        private readonly Profiler.TimelineTrack profilerTrack;
        private volatile bool isRunning;
        private int remainingQueries;

        public TaskQuerier(Runner.SubtaskRunner runner, ITaskExecutor[] tasks, Utils.Logger logger, Profiler.Timeline profiler = null)
        {
            this.runner = runner;
            this.tasks = tasks;
            this.logger = logger;
            this.profilerTrack = profiler?.CreateTrack<Profiler.TimelineTrack>(GetType().Name);
        }

        public void QueryTasks()
        {
            if (isRunning)
                throw new Exception($"[{nameof(TaskQuerier)}] Allready running!");
            isRunning = true;

            profilerTrack?.LogStartWork();

            remainingQueries = tasks.Length;
            if (remainingQueries == 0)
                Complete();
            else
            {
                for (int i = 0; i < tasks.Length; i++)
                    runner.PushTask(this, i, i);
                runner.WakeExecutors();
            }
        }

        void Runner.ExecuteInfo.ISubtaskExecutor.ExecuteSubtask(int execID, int minSubtaskIndex, int maxSubtaskIndex)
        {
            try
            {
                for (int i = minSubtaskIndex; i <= maxSubtaskIndex; i++)
                    tasks[i].QuerySubtasks();
            }
            catch (Exception e) { logger?.Log(e); }

            if (Interlocked.Decrement(ref remainingQueries) == 0)
                Complete();
        }

        private void Complete()
        {
            profilerTrack?.LogEndWork();
            isRunning = false;

            Completed?.Invoke();
        }
    }
}
