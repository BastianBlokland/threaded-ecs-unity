using System;

namespace ECS.Tasks.Runner
{
	public sealed class SubtaskRunner : ITaskSource, IDisposable
	{
		private readonly int taskQueueCount;
		private readonly int executorCount;
		private readonly TaskQueue[] taskQueues;
		private readonly ExecutorThread[] executors;
		private readonly object pushLock;
		private int currentPushQueueIndex;
	
		public SubtaskRunner(int numberOfExecutors)
		{
			taskQueueCount = numberOfExecutors > 0 ? numberOfExecutors : 1;
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
			var info = new ExecuteInfo(executor, minIndex, maxIndex);
			lock(pushLock)
			{
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
			var executorID = System.Environment.TickCount % taskQueueCount; //Note: 'TickCount' has a very bad resolution, need to think of a better way to distribute
			var info = GetTask(execID: executorID);
			if(info.HasValue)
			{
				try { info.Value.Execute(execID: -1); }
				catch(Exception) { } 
			}
		}

		public void Dispose()
		{
			for (int i = 0; i < executorCount; i++)
				executors[i].Dispose();
		}		

		private ExecuteInfo? GetTask(int execID)
		{
			for (int i = 0; i < taskQueueCount; i++)
			{
				var queueIndex = (execID + i) % taskQueueCount;
				var task = taskQueues[queueIndex].GetTask();
				if(task.HasValue)
					return task;
			}
			return null;
		}

		ExecuteInfo? ITaskSource.GetTask(int execID) => GetTask(execID);
	}
}