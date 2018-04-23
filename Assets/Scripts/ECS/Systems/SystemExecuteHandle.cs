using System;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;

namespace ECS.Systems
{
    public class SystemExecuteHandle
    {
        //NOTE: VERY important to realize that this can be called from any thread
		public event Action Completed = delegate {};

		private readonly SystemRunner runner;
		private readonly System system;

		private bool isScheduled;
		private int entitiesLeft;

		public SystemExecuteHandle(SystemRunner runner, System system)
		{
			this.runner = runner;
			this.system = system;
		}

		public void Schedule()
		{
			if(isScheduled)
				return;
			isScheduled = true;

			//Note: entities list actually owned by the system so get what we want from it and then discard the reference
			IList<EntityID> entities = system.GetEntities();
			
			entitiesLeft = entities.Count;
			if(entitiesLeft == 0)
			{
				Completed();
			}
			else
			{
				for (int i = 0; i < entitiesLeft; i++)
					runner.Schedule(this, entities[i]);
			}
		}

		//----> RUNNING ON SEPARATE THREAD
		public void Execute(EntityID entity)
		{
			try { system.Execute(entity); } 
			catch(Exception) { }

			if(Interlocked.Decrement(ref entitiesLeft) == 0)
			{
				Completed();
			}
		}
		//----> RUNNING ON SEPARATE THREAD
    }
}