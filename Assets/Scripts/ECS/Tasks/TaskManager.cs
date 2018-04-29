using System;

namespace ECS.Tasks
{
	public class TaskManager : IDisposable
	{
		public bool IsRunning { get { return !isCompleted; } }

		private readonly int batchCount;
		private readonly SubtaskRunner runner;
		private readonly ITask[] tasks;
		private readonly Profiler.TimelineTrack[] timelineTracks;

		private volatile bool isCompleted = true;

		public TaskManager(int executorCount, int batchCount, Profiler.Timeline timeline, params ITask[] tasks)
		{
			this.batchCount = batchCount;
			this.runner = new SubtaskRunner(executorCount);
			this.tasks = tasks;

			//Create profiler tracks for all the tasks
			if(timeline != null)
			{
				this.timelineTracks = new Profiler.TimelineTrack[tasks.Length];
				for (int i = 0; i < tasks.Length; i++)
					timelineTracks[i] = timeline.CreateTrack<Profiler.TimelineTrack>(tasks[i].GetType().Name);
			}
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

			TaskExecuteHandle firstTask = null;
			TaskExecuteHandle previousTask = null;
			for (int i = 0; i < tasks.Length; i++)
			{
				TaskExecuteHandle executor = new TaskExecuteHandle(tasks[i], runner, batchCount, timelineTracks[i]);
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