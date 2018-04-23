using System;
using System.Threading;

namespace ECS.Systems
{
	public class TrackExecuteHandle
	{
		public event Action Completed = delegate {};

		private readonly SystemRunner runner;
		private readonly System[] systems;

		private bool isScheduled;
		private int systemsLeft;

		public TrackExecuteHandle(SystemRunner runner, params System[] systems)
		{
			this.runner = runner;
			this.systems = systems;
		}

		public void Schedule()
		{
			if(isScheduled)
				return;
			isScheduled = true;

			systemsLeft = systems.Length;
			for (int i = 0; i < systems.Length; i++)
			{
				SystemExecuteHandle handle = new SystemExecuteHandle(runner, systems[i]);
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