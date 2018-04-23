using ECS.Systems;

namespace Profiler
{
	public class SystemTimelineTrack : TimelineTrack
	{
		public void Track(SystemExecuteHandle executeHandle)
		{
			LogStartWork();
			executeHandle.Completed += OnDependencyCompleted;
		}

		private void OnDependencyScheduled()
		{
			LogStartWork();
		}

		private void OnDependencyCompleted()
		{
			LogEndWork();
		}
	}
}