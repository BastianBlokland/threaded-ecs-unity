using System;
using System.Threading;

namespace ECS.Tasks
{
    public class GroupTaskExecutor : ITaskExecutor
    {
        //NOTE: VERY important to realize that this can be called from any thread
		public event Action Completed = delegate {};

		private readonly ITaskExecutor[] innerExecutors;

		private bool isScheduled;
		private CountdownEvent countdownEvent;

		public GroupTaskExecutor(Runner.SubtaskRunner runner, params ITask[] innerTasks)
		{
			innerExecutors = new ITaskExecutor[innerTasks.Length];
			for (int i = 0; i < innerTasks.Length; i++)
				innerExecutors[i] = innerTasks[i].CreateExecutor(runner);
		}

		public void Schedule()
		{
			if(isScheduled)
				return;
			isScheduled = true;

			countdownEvent = new CountdownEvent(innerExecutors.Length);
			for (int i = 0; i < innerExecutors.Length; i++)
			{
				innerExecutors[i].Completed += InnerTaskComplete;
				innerExecutors[i].Schedule();
			}	
		}

		private void InnerTaskComplete()
		{
			if(countdownEvent.Signal())
			{
				Completed();
				countdownEvent.Dispose();
			}
		}
		//----> RUNNING ON SEPARATE THREAD
    }
}