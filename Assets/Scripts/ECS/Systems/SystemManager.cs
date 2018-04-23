using System;

namespace ECS.Systems
{
	public class SystemManager : IDisposable
	{
		public bool IsRunning { get { return !isCompleted; } }

		private readonly int batchSize;
		private readonly ActionRunner runner;
		private readonly System[][] systems;
		private readonly Profiler.SystemTimelineTrack[][] timelineTracks;

		private volatile bool isCompleted = true;

		/// <summary>
		/// Wierd syntax but this 'systems' jagged array is a array of system tracks. So the outer array
		/// is scheduled linearly but the items in the inner array run in parallel. Need to see if i can
		/// come up with nicer syntax for this. 
		/// </summary>
		public SystemManager(int executorCount, int batchSize, Profiler.Timeline timeline, params System[][] systems)
		{
			this.batchSize = batchSize;
			this.runner = new ActionRunner(executorCount);
			this.systems = systems;

			//Create profiler tracks for all the systems
			if(timeline != null)
			{
				this.timelineTracks = new Profiler.SystemTimelineTrack[systems.Length][];
				for (int i = 0; i < systems.Length; i++)
				{
					timelineTracks[i] = new Profiler.SystemTimelineTrack[systems[i].Length];
					for (int j = 0; j < systems[i].Length; j++)
					{
						string trackName = systems[i][j].GetType().Name;
						timelineTracks[i][j] = timeline.CreateTrack<Profiler.SystemTimelineTrack>(trackName);
					}
				}
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

			TrackExecuteHandle firstTrack = null;
			TrackExecuteHandle previousTrack = null;
			for (int i = 0; i < systems.Length; i++)
			{
				TrackExecuteHandle trackHandle = new TrackExecuteHandle
				(
					runner: runner,
					systems: systems[i],
					batchSize: batchSize,
					profilerTracks: timelineTracks == null ? null : timelineTracks[i]
				);
				if(firstTrack == null)
					firstTrack = trackHandle;

				//Chain all the track together in a linear fashion
				if(previousTrack != null)
					previousTrack.Completed += trackHandle.Schedule;

				previousTrack = trackHandle;
			}

			//Follow the completion of the last track
			if(previousTrack != null)
				previousTrack.Completed += LastTrackCompleted;

			//Start the chain
			if(firstTrack != null)
				firstTrack.Schedule();
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