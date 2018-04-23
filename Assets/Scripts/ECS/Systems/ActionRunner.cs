using System;
using System.Collections.Concurrent;
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
				for (int i = minIndex; i <= maxIndex; i++)
					executor.ExecuteElement(i);
			}
		}

		//----> Syncing data
		private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
		private readonly BlockingCollection<ActionInfo> actionQueue = new BlockingCollection<ActionInfo>();

		public ActionRunner(int executorCount)
		{
			for(int i = 0; i < executorCount; i++)
			{
				Thread executorThread = new Thread(ThreadExecutor);
				executorThread.Priority = ThreadPriority.Highest;
				executorThread.Start();
			}
		}

		public void Schedule(IActionExecutor executor, int minIndex, int maxIndex)
		{
			actionQueue.Add(new ActionInfo(executor, minIndex, maxIndex));
		}

		public void Help()
		{
			ActionInfo action;
			if(actionQueue.TryTake(out action))
			{
				try { action.Execute(); }
				catch(Exception) { }
			}
		}

		public void Dispose()
		{
			cancelTokenSource.Cancel();
		}

		//----> RUNNING ON SEPARATE THREAD
		private void ThreadExecutor()
		{
			while(!cancelTokenSource.IsCancellationRequested)
			{
				ActionInfo action = actionQueue.Take(cancelTokenSource.Token);
				try { action.Execute(); }
				catch(Exception) { }
			}
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}