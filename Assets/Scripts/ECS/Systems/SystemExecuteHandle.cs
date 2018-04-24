using System;
using System.Collections.Generic;
using System.Threading;
using ECS.Storage;

using EntityID = System.UInt16;

namespace ECS.Systems
{
    public class SystemExecuteHandle : IActionExecutor
    {
        //NOTE: VERY important to realize that this can be called from any thread
		public event Action Completed = delegate {};

		private readonly ActionRunner runner;
		private readonly System system;
		private readonly Profiler.TimelineTrack timelineTrack;

		private bool isScheduled;
		private EntitySet entities;
		private CountdownEvent countdownEvent;

		public SystemExecuteHandle(ActionRunner runner, System system, Profiler.TimelineTrack timelineTrack = null)
		{
			this.runner = runner;
			this.system = system;
			this.timelineTrack = timelineTrack;
		}

		public void Schedule()
		{
			if(isScheduled)
				return;
			isScheduled = true;

			if(timelineTrack != null)
				timelineTrack.LogStartWork();

			//Note: entities list actually owned by the system so need to realise that you can't schedule the same system twice
			//as both of them would be the same entities reference
			entities = system.GetEntities();
			int count = entities.Count;

			if(count == 0)
				Complete();
			else
			{
				countdownEvent = new CountdownEvent(count);

				//NOTE: do not use 'entitiesLeft' instead of 'entities.Count' here as 'entitiesLeft' can be modified during the loop if
				//the tasks take very little time to execute, took me an hour to figure out why not all entities where scheduled :)
				int startOffset = system.BatchSize - 1;
				for (int i = 0; i < count; i += system.BatchSize)
				{
					int start = i;
					int end = start + startOffset;
					runner.Schedule(this, start, end >= count ? (count - 1) : end);
				}
			}
		}

		//----> RUNNING ON SEPARATE THREAD
		public void ExecuteElement(int index)
		{
			try 
			{
				EntityID entity = entities.Data[index];
				system.Execute(entity); 
			} 
			catch(Exception) { }

			if(countdownEvent.Signal())
			{
				Complete();
				countdownEvent.Dispose();
			}
		}
		//----> RUNNING ON SEPARATE THREAD

		private void Complete()
		{
			Completed();
			if(timelineTrack != null)
				timelineTrack.LogEndWork();
		}
    }
}