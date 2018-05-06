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

		public GroupTask(params ITaskExecutor[] innerTasks)
		{
			this.innerTasks = innerTasks;
			for (int i = 0; i < innerTasks.Length; i++)
				innerTasks[i].Completed += InnerTaskComplete;
		}

		public void QuerySubtasks()
		{
			for (int i = 0; i < innerTasks.Length; i++)
				innerTasks[i].QuerySubtasks();
		}

		public void RunSubtasks()
		{
			if(isRunning)
				throw new Exception($"[{nameof(GroupTask)}] Allready running!");
			isRunning = true;

			remainingTasks = innerTasks.Length;
			if(remainingTasks == 0)
				Complete();
			else
			{
				for (int i = 0; i < innerTasks.Length; i++)
					innerTasks[i].RunSubtasks();
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