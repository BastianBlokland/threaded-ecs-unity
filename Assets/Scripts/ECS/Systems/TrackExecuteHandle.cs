using System;
using System.Threading;

namespace ECS.Systems
{
	public class TrackExecuteHandle
	{
		public event Action Completed = delegate {};

		private readonly int batchSize;
		private readonly ActionRunner runner;
		private readonly System[] systems;
		private readonly Profiler.SystemTimelineTrack[] profilerTracks;
		
		private bool isScheduled;
		private int systemsLeft;

		public TrackExecuteHandle(
			int batchSize,
			ActionRunner runner, 
			System[] systems,
			Profiler.SystemTimelineTrack[] profilerTracks = null)
		{
			this.batchSize = batchSize;
			this.runner = runner;
			this.systems = systems;
			this.profilerTracks = profilerTracks;
		}

		public void Schedule()
		{
			if(isScheduled)
				return;
			isScheduled = true;

			systemsLeft = systems.Length;
			for (int i = 0; i < systems.Length; i++)
			{
				SystemExecuteHandle handle = new SystemExecuteHandle(batchSize, runner, systems[i]);

				if(profilerTracks != null)
					profilerTracks[i].Track(handle);

				handle.Completed += SingleSystemComplete;
				handle.Schedule();
			}
		}

		private void SingleSystemComplete()
		{
			if(Interlocked.Decrement(ref systemsLeft) == 0)
			{
				Completed();
			}
		}
	}
}