using System;
using System.Collections.Generic;
using System.Threading;

namespace ECS.Tasks.Runner
{
	public class TaskQueue
	{
		private readonly Queue<ExecuteInfo> queue;
		private readonly object lockObject;
	
		public TaskQueue()
		{
			queue = new Queue<ExecuteInfo>();
			lockObject = new object();
		}

		public void BeginPushingTasks()
		{
			Monitor.Enter(lockObject);
		}

		public void PushTask(ExecuteInfo executeInfo)
		{
			queue.Enqueue(executeInfo);
		}

		public void EndPushingTasks()
		{
			Monitor.Exit(lockObject);
		}	

		public ExecuteInfo? GetTask()
		{
			lock(lockObject)
			{
				if(queue.Count == 0)
					return null;
				return queue.Dequeue();
			}
		}
	}
}