using System;
using System.Collections.Concurrent;
using System.Threading;

using EntityID = System.UInt16;

namespace ECS.Systems
{
	public class SystemRunner : IDisposable
	{
		private struct ActionInfo
		{
			private readonly SystemExecuteHandle executor;
			private readonly EntityID entity;

			public ActionInfo(SystemExecuteHandle executor, EntityID entity)
			{
				this.executor = executor;
				this.entity = entity;
			}

			public void Execute()
			{
				executor.Execute(entity);
			}
		}

		//----> Syncing data
		private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
		private readonly BlockingCollection<ActionInfo> actionQueue = new BlockingCollection<ActionInfo>();

		public SystemRunner(int executorCount)
		{
			for(int i = 0; i < executorCount; i++)
			{
				Thread executorThread = new Thread(ThreadExecutor);
				executorThread.Priority = ThreadPriority.Highest;
				executorThread.Start();
			}
		}

		public void Schedule(SystemExecuteHandle executor, EntityID entity)
		{
			actionQueue.Add(new ActionInfo(executor, entity));
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