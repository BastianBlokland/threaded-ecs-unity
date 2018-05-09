using System;
using System.Threading;

namespace ECS.Tasks
{
    public sealed class SubtaskExecutor : ITaskExecutor, Runner.ExecuteInfo.ISubtaskExecutor
    {
		public interface IProvider
		{
			int PrepareSubtasks();
			void ExecuteSubtask(int execID, int index);
		}

		public event Action Completed;

		private readonly IProvider provider;
		private readonly Runner.SubtaskRunner runner;
		private readonly int batchSize;
		private readonly Utils.Logger logger;
		private readonly Profiler.TimelineTrack profilerTrack;

		private volatile bool isRunning;
		private int totalSubtaskCount;
		private int remainingBatches;

		public SubtaskExecutor(
				IProvider provider, 
				Runner.SubtaskRunner runner, 
				int batchSize, 
				Utils.Logger logger = null, 
				Profiler.Timeline profiler = null)
		{
			this.provider = provider;
			this.runner = runner;
			this.batchSize = batchSize;
			this.logger = logger;
			this.profilerTrack = profiler?.CreateTrack<Profiler.TimelineTrack>(provider.GetType().Name);
		}

		public void QuerySubtasks() => totalSubtaskCount = provider.PrepareSubtasks();

		public void RunSubtasks()
		{
			if(isRunning)
				throw new Exception($"[{nameof(SubtaskExecutor)}] Allready running!");
			if(runner == null)
				throw new Exception($"[{nameof(SubtaskExecutor)}] No '{nameof(Runner.SubtaskRunner)}' provided!");
			isRunning = true;

			profilerTrack?.LogStartWork();

			if(totalSubtaskCount == 0)
			{	
				remainingBatches = 0;
				Complete();
			}
			else
			{
				remainingBatches = (totalSubtaskCount - 1) / batchSize + 1; //'Trick' to round up using integer division

				int startOffset = batchSize - 1;
				int maxIndex = totalSubtaskCount - 1;
				for (int i = 0; i < totalSubtaskCount; i += batchSize)
				{
					int start = i;
					int end = start + startOffset;
					runner.PushTask(this, start, end >= totalSubtaskCount ? maxIndex : end);
				}
				runner.WakeExecutors();
			}
		}

		void Runner.ExecuteInfo.ISubtaskExecutor.ExecuteSubtask(int execID, int minSubtaskIndex, int maxSubtaskIndex)
		{
			try
			{
				for (int i = minSubtaskIndex; i <= maxSubtaskIndex; i++)
					provider.ExecuteSubtask(execID, index: i);
			} catch (Exception e) { logger?.Log(e); }

			if(Interlocked.Decrement(ref remainingBatches) == 0)
				Complete();
		}

		private void Complete()
		{
			profilerTrack?.LogEndWork();
			isRunning = false;
			
			Completed?.Invoke();
		}
    }
}