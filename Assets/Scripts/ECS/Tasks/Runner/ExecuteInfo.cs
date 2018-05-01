namespace ECS.Tasks.Runner
{
	public struct ExecuteInfo
	{
		public interface ISubtaskExecutor
		{
			void ExecuteSubtask(int subtaskIndex);
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

		public void Execute()
		{
			if(executor != null)
			{
				for (int i = minSubtaskIndex; i <= maxSubtaskIndex; i++)
					executor.ExecuteSubtask(i);
			}
		}
	}
}