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
		private CountdownEvent countdownEvent;

		public TrackExecuteHandle(
			ActionRunner runner, 
			System[] systems,
			Profiler.SystemTimelineTrack[] profilerTracks = null)
		{
			this.runner = runner;
			this.systems = systems;
			this.profilerTracks = profilerTracks;
		}

		public void Schedule()
		{
			if(isScheduled)
				return;
			isScheduled = true;

			countdownEvent = new CountdownEvent(systems.Length);
			for (int i = 0; i < systems.Length; i++)
			{
				SystemExecuteHandle handle = new SystemExecuteHandle(runner, systems[i]);

				if(profilerTracks != null)
					profilerTracks[i].Track(handle);

				handle.Completed += SingleSystemComplete;
				handle.Schedule();
			}
		}

		private void SingleSystemComplete()
		{
			if(countdownEvent.Signal())
			{
				Completed();
				countdownEvent.Dispose();
			}
		}
	}
}