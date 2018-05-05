using System;
using System.Threading;

namespace ECS.Tasks
{
	public class GroupTask : ITaskExecutor
    {
		public event Action Completed;

		private readonly ITaskExecutor[] innerTasks;

		private volatile bool isRunning;
		private int remainingTasks;

		public GroupTask(Runner.SubtaskRunner runner, params ITaskExecutor[] innerTasks)
		{
			this.innerTasks = innerTasks;
			for (int i = 0; i < innerTasks.Length; i++)
				innerTasks[i].Completed += InnerTaskComplete;
		}

		public void Schedule()
		{
			if(isRunning)
				throw new Exception("[SubtaskExecutor] Allready running!");
			isRunning = true;

			remainingTasks = innerTasks.Length;
			if(remainingTasks == 0)
				Complete();
			else
			{
				for (int i = 0; i < innerTasks.Length; i++)
					innerTasks[i].Schedule();
			}
		}

		private void InnerTaskComplete()
		{
			if(Interlocked.Decrement(ref remainingTasks) == 0)
				Complete();
		}

		private void Complete()
		{
			isRunning = false;
			Completed?.Invoke();
		}
	}
}