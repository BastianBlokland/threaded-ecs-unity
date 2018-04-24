using System;

namespace ECS.Systems
{
	public class SystemManager : IDisposable
	{
		public bool IsRunning { get { return !isCompleted; } }

		private readonly ActionRunner runner;
		private readonly System[] systems;
		private readonly Profiler.TimelineTrack[] timelineTracks;

		private volatile bool isCompleted = true;

		public SystemManager(int executorCount, Profiler.Timeline timeline, params System[] systems)
		{
			this.runner = new ActionRunner(executorCount);
			this.systems = systems;

			//Create profiler tracks for all the systems
			if(timeline != null)
			{
				this.timelineTracks = new Profiler.TimelineTrack[systems.Length];
				for (int i = 0; i < systems.Length; i++)
					timelineTracks[i] = timeline.CreateTrack<Profiler.TimelineTrack>(systems[i].GetType().Name);
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

			SystemExecuteHandle firstSystem = null;
			SystemExecuteHandle previousSystem = null;
			for (int i = 0; i < systems.Length; i++)
			{
				SystemExecuteHandle executeHandle = new SystemExecuteHandle(runner, systems[i], timelineTracks[i]);
				if(firstSystem == null)
					firstSystem = executeHandle;

				//Chain all the track together in a linear fashion
				if(previousSystem != null)
					previousSystem.Completed += executeHandle.Schedule;

				previousSystem = executeHandle;
			}

			//Follow the completion of the last track
			if(previousSystem != null)
				previousSystem.Completed += LastTrackCompleted;

			//Start the chain
			if(firstSystem != null)
				firstSystem.Schedule();
			else //If there where no track then consider it to be complete allready
				LastTrackCompleted();
		}

		public void Dispose()
		{
			runner.Dispose();
		}

		private void LastTrackCompleted()
		{
			isCompleted = true;
		}
	}
}