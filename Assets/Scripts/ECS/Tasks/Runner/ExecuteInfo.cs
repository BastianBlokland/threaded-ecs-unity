namespace ECS.Tasks.Runner
{
    public struct ExecuteInfo
    {
        public interface ISubtaskExecutor
        {
            void ExecuteSubtask(int execID, int minSubtaskIndex, int maxSubtaskIndex);
        }

        private readonly ISubtaskExecutor executor;
        private readonly int minSubtaskIndex;
        private readonly int maxSubtaskIndex;

        public ExecuteInfo(ISubtaskExecutor executor, int minSubtaskIndex, int maxSubtaskIndex)
        {
            this.executor = executor;
            this.minSubtaskIndex = minSubtaskIndex;
            this.maxSubtaskIndex = maxSubtaskIndex;
        }

        public void Execute(int execID) => executor?.ExecuteSubtask(execID, minSubtaskIndex, maxSubtaskIndex);
    }
}
