using System;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;

namespace ECS.Tasks
{
	public class SubtaskRunner : IDisposable
	{
		public interface ISubtaskExecutor
		{
			void ExecuteSubtask(int subtaskIndex);
		}

		private struct ExecuteInfo
		{
			private readonly ISubtaskExecutor executor;
			private readonly int minSubtaskIndex;
			private readonly int maxSubtaskIndex;

			public ExecuteInfo(ISubtaskExecutor executor, int minSubtaskIndex, int maxSubtaskIndex)
			{
				this.executor = executor;
				this.minSubtaskIndex = minSubtaskIndex;
				this.maxSubtaskIndex = maxSubtaskIndex;
			}

			public void Execute()
			{
				if(executor != null)
				{
					for (int i = minSubtaskIndex; i <= maxSubtaskIndex; i++)
						executor.ExecuteSubtask(i);
				}
			}
		}

		private readonly Queue<ExecuteInfo> workQueue;
		private readonly object lockObject;
		private volatile bool cancel;

		public SubtaskRunner(int executorCount)
		{
			workQueue = new Queue<ExecuteInfo>();
			lockObject = new object();

			for(int i = 0; i < executorCount; i++)
			{
				Thread executorThread = new Thread(ThreadExecutor);
				executorThread.Priority = ThreadPriority.Highest;
				executorThread.Start();
			}
		}

		public void Schedule(ISubtaskExecutor executor, int minIndex, int maxIndex)
		{
			lock(lockObject)
			{
				workQueue.Enqueue(new ExecuteInfo(executor, minIndex, maxIndex));
				Monitor.Pulse(lockObject);
			}
		}

		public void Help()
		{
			ExecuteInfo work = new ExecuteInfo();
			lock(lockObject)
			{
				if(workQueue.Count > 0)
					work = workQueue.Dequeue();
			}
			try { work.Execute(); }
			catch(Exception) {}
		}

		public void Dispose()
		{
			cancel = true;
			//Wake up all the executors thread so they can cancel themselves
			lock(lockObject)
			{
				Monitor.PulseAll(lockObject); 
			}
		}

		//----> RUNNING ON SEPARATE THREAD
		private void ThreadExecutor()
		{
			while(!cancel)
			{
				ExecuteInfo task;				
				lock(lockObject)
				{
					while(workQueue.Count == 0 && !cancel)
						Monitor.Wait(lockObject);
					task = workQueue.Dequeue();
				}
				if(!cancel)
				{
					try { task.Execute(); }
					catch(Exception) { }
				}
			}
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}