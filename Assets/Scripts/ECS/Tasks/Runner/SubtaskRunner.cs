using System;
using System.Collections.Generic;
using System.Threading;

namespace ECS.Tasks.Runner
{
	public class SubtaskRunner : ITaskSource, IDisposable
	{
		private readonly int taskQueueCount;
		private readonly int executorCount;
		private readonly TaskQueue[] taskQueues;
		private readonly ExecutorThread[] executors;
		private readonly object pushLock;
		private int currentPushQueueIndex;
	
		public SubtaskRunner(int numberOfExecutors)
		{
			taskQueueCount = executorCount > 0 ? executorCount : 1;
			executorCount = numberOfExecutors;

			taskQueues = new TaskQueue[taskQueueCount];
			for (int i = 0; i < taskQueueCount; i++)
				taskQueues[i] = new TaskQueue();

			executors = new ExecutorThread[executorCount];
			for (int i = 0; i < executorCount; i++)
				executors[i] = new ExecutorThread(executorID: i, taskSource: this);
			
			pushLock = new object();
		}

		public void PushTask(ExecuteInfo.ISubtaskExecutor executor, int minIndex, int maxIndex)
		{
			lock(pushLock)
			{
				ExecuteInfo info = new ExecuteInfo(executor, minIndex, maxIndex);
				taskQueues[currentPushQueueIndex].PushTask(info);
				currentPushQueueIndex = (currentPushQueueIndex + 1) % taskQueueCount;
			}
		}

		public void WakeExecutors()
		{
			for (int i = 0; i < executorCount; i++)
				executors[i].Wake();
		}

		public void Help()
		{
			//Take a random executor id to not be contending the same executor all the time
			int executorID = System.Environment.TickCount % taskQueueCount;
			ExecuteInfo? info = GetTask(executorID: executorID);
			if(info.HasValue)
			{
				try { info.Value.Execute(); }
				catch(Exception) { } 
			}
		}

		public void Dispose()
		{
			for (int i = 0; i < executorCount; i++)
				executors[i].Dispose();
		}		

		private ExecuteInfo? GetTask(int executorID)
		{
			for (int i = 0; i < taskQueueCount; i++)
			{
				int queueIndex = (executorID + i) % taskQueueCount;
				ExecuteInfo? task = taskQueues[queueIndex].GetTask();
				if(task.HasValue)
					return task;
			}
			return null;
		}

		ExecuteInfo? ITaskSource.GetTask(int executorID)
		{
			return GetTask(executorID);
		}
	}
}