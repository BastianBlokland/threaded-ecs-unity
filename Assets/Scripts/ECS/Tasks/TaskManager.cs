using System;

namespace ECS.Tasks
{
	public class TaskManager : IDisposable
	{
		public bool IsRunning { get { return !isCompleted; } }

		private readonly SubtaskRunner runner;
		private readonly ITask[] tasks;

		private volatile bool isCompleted = true;

		public TaskManager(int executorCount, params ITask[] tasks)
		{
			this.runner = new SubtaskRunner(executorCount);
			this.tasks = tasks;
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

			ITaskExecutor firstTask = null;
			ITaskExecutor previousTask = null;
			for (int i = 0; i < tasks.Length; i++)
			{
				ITaskExecutor executor = tasks[i].CreateExecutor(runner);
				if(firstTask == null)
					firstTask = executor;

				//Chain all the tasks together in a linear fashion
				if(previousTask != null)
					previousTask.Completed += executor.Schedule;

				previousTask = executor;
			}

			//Follow the completion of the last task
			if(previousTask != null)
				previousTask.Completed += LastTaskCompleted;

			//Start the chain
			if(firstTask != null)
				firstTask.Schedule();
			else //If there is no task then consider it to be complete allready
				LastTaskCompleted();
		}

		public void Dispose()
		{
			runner.Dispose();
		}

		private void LastTaskCompleted()
		{
			isCompleted = true;
		}
	}
}