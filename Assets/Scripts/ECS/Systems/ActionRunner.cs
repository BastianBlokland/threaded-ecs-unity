using System;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;

namespace ECS.Systems
{
	public class ActionRunner : IDisposable
	{
		private struct ActionInfo
		{
			private readonly IActionExecutor executor;
			private readonly int minIndex;
			private readonly int maxIndex;

			public ActionInfo(IActionExecutor executor, int minIndex, int maxIndex)
			{
				this.executor = executor;
				this.minIndex = minIndex;
				this.maxIndex = maxIndex;
			}

			public void Execute()
			{
				if(executor != null)
				{
					for (int i = minIndex; i <= maxIndex; i++)
						executor.ExecuteElement(i);
				}
			}
		}

		private readonly Queue<ActionInfo> actionQueue;
		private readonly object lockObject;
		private volatile bool cancel;

		public ActionRunner(int executorCount)
		{
			actionQueue = new Queue<ActionInfo>();
			lockObject = new object();

			for(int i = 0; i < executorCount; i++)
			{
				Thread executorThread = new Thread(ThreadExecutor);
				executorThread.Priority = ThreadPriority.Highest;
				executorThread.Start();
			}
		}

		public void Schedule(IActionExecutor executor, int minIndex, int maxIndex)
		{
			lock(lockObject)
			{
				actionQueue.Enqueue(new ActionInfo(executor, minIndex, maxIndex));
				Monitor.Pulse(lockObject);
			}
		}

		public void Help()
		{
			ActionInfo action = new ActionInfo();
			lock(lockObject)
			{
				if(actionQueue.Count > 0)
					action = actionQueue.Dequeue();
			}
			try { action.Execute(); }
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
				ActionInfo action;				
				lock(lockObject)
				{
					while(actionQueue.Count == 0 && !cancel)
						Monitor.Wait(lockObject);
					action = actionQueue.Dequeue();
				}
				if(!cancel)
				{
					try { action.Execute(); }
					catch(Exception) { }
				}
			}
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}