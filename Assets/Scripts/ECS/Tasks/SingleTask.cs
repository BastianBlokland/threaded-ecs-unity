using ECS.Storage;
using ECS.Tasks.Runner;
using Utils;

namespace ECS.Tasks
{
    public abstract class SingleTask : ITask, SubtaskExecutor.IProvider
    {
        private readonly int batchSize;

        public SingleTask(int batchSize)
        {
            this.batchSize = batchSize;
        }

        public ITaskExecutor CreateExecutor(SubtaskRunner runner, Logger logger = null, Profiler.Timeline profiler = null)
        {
            return new SubtaskExecutor(this, runner, batchSize, logger, profiler);
        }

        int SubtaskExecutor.IProvider.PrepareSubtasks() => PrepareSubtasks();
        void SubtaskExecutor.IProvider.ExecuteSubtask(int execID, int index) => ExecuteSubtask(execID, index);

        protected abstract int PrepareSubtasks();
        protected abstract void ExecuteSubtask(int execID, int index);
    }
}
