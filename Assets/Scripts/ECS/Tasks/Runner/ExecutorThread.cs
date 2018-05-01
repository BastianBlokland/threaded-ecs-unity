using System;
using System.Collections.Generic;
using System.Threading;

namespace ECS.Tasks.Runner
{
	public class ExecutorThread : IDisposable
	{
		private readonly int executorID;
		private readonly ITaskSource taskSource;
		private readonly CancellationTokenSource cancelTokenSource;
		private readonly ManualResetEventSlim wakeEvent;
		private readonly Thread thread;
		
		public ExecutorThread(int executorID, ITaskSource taskSource)
		{
			this.executorID = executorID;
			this.taskSource = taskSource;

			cancelTokenSource = new CancellationTokenSource();
			wakeEvent = new ManualResetEventSlim();
			thread = new Thread(ExecuteLoop);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.AboveNormal;
			thread.Start();
		}

		public void Wake()
		{
			wakeEvent.Set();
		}

		public void Dispose()
		{
			//Request cancellation
			cancelTokenSource.Cancel();

			//Wait for the executor thread to cancel itself
			thread.Join();

			//Dispose resources
			cancelTokenSource.Dispose();
			wakeEvent.Dispose();
		}

		//----> RUNNING ON SEPARATE THREAD
		private void ExecuteLoop()
		{
			CancellationToken token = cancelTokenSource.Token;
			while(!token.IsCancellationRequested)
			{
				ExecuteInfo? task;
				do
				{
					task = taskSource.GetTask(executorID);
					if(task.HasValue)
					{
						try { task.Value.Execute(); }
						catch(Exception) { }
					}
				} while(task.HasValue);

				//No tasks left, go to sleep and wait to be woken
				wakeEvent.Wait(token);
				wakeEvent.Reset();
			}
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}