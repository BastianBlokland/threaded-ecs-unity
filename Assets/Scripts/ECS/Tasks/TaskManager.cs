using System;

namespace ECS.Tasks
{
	public sealed class TaskManager
	{
		private readonly Runner.SubtaskRunner runner;
		private readonly TaskQuerier querier;
		private volatile bool isRunning;

		public TaskManager(Runner.SubtaskRunner runner, ITask[] tasks, Utils.Logger logger = null, Profiler.Timeline profiler = null)
		{
			this.runner = runner;

			//Create executors for all the tasks
			ITaskExecutor[] executors = new ITaskExecutor[tasks.Length];
			for (int i = 0; i < tasks.Length; i++)
				executors[i] = tasks[i].CreateExecutor(runner, logger, profiler);

			//Create a querier for querying all the executors for work
			querier = new TaskQuerier(runner, executors, logger, profiler);

			//Setup a chain between the executors, starting from the querier
			for (int i = 0; i < executors.Length; i++)
			{
				switch(i)
				{
					case 0: querier.Completed += executors[i].RunSubtasks;  break;
					default: executors[i - 1].Completed += executors[i].RunSubtasks; break;
				}
			}
			if(executors.Length > 0)
				executors[executors.Length - 1].Completed += LastTaskCompleted;
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