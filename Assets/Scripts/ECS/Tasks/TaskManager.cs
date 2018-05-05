using System;

namespace ECS.Tasks
{
	public sealed class TaskManager
	{
		public bool IsRunning => !isCompleted;

		private readonly Runner.SubtaskRunner runner;
		private readonly ITaskExecutor[] tasks;
		private volatile bool isCompleted = true;

		public TaskManager(Runner.SubtaskRunner runner, params ITaskExecutor[] tasks)
		{
			this.runner = runner;
			this.tasks = tasks;

			ITaskExecutor previousTask = null;
			for (int i = 0; i < tasks.Length; i++)
			{
				//Chain all the tasks together in a linear fashion
				if(previousTask != null)
					previousTask.Completed += tasks[i].Schedule;
				previousTask = tasks[i];
			}
			
			//Follow the completion of the last task
			if(previousTask != null)
				previousTask.Completed += LastTaskCompleted;
		}

		public void Complete()
		{
			while(!isCompleted)
				runner.Help();
		}

		public void Schedule()
		{
			//First complete the previous run
			Complete();

			isCompleted = false;

			//Start the chain
			if(tasks.Length > 0)
				tasks[0].Schedule();
			else //If there is no task then consider it to be complete allready
				LastTaskCompleted();
		}

		private void LastTaskCompleted() => isCompleted = true;
	}
}