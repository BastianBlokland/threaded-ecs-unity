using System;

namespace ECS.Tasks
{
	public sealed class TaskManager
	{
		private readonly Runner.SubtaskRunner runner;
		private readonly TaskQuerier querier;
		private volatile bool isRunning;

		public TaskManager(Runner.SubtaskRunner runner, ITaskExecutor[] tasks, Profiler.Timeline profiler = null)
		{
			this.runner = runner;
			this.querier = new TaskQuerier(runner, tasks, profiler);

			//Setup chain between tasks, starting form the querier
			for (int i = 0; i < tasks.Length; i++)
			{
				switch(i)
				{
					case 0: querier.Completed += tasks[i].RunSubtasks;  break;
					default: tasks[i - 1].Completed += tasks[i].RunSubtasks; break;
				}
			}
			if(tasks.Length > 0)
				tasks[tasks.Length - 1].Completed += LastTaskCompleted;
			else
				querier.Completed += LastTaskCompleted;
		}

		public void Complete()
		{
			while(isRunning)
				runner.Help();
		}

		public void Run()
		{
			//First complete the previous run
			Complete();

			isRunning = true;

			//Start the chain
			querier.QueryTasks();
		}

		private void LastTaskCompleted() => isRunning = false;
	}
}