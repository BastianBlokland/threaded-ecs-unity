using System;
using System.Threading;

namespace ECS.Tasks
{
    public abstract class SubtaskExecutor : ITaskExecutor, Runner.ExecuteInfo.ISubtaskExecutor
    {
		public event Action Completed;

		private readonly Runner.SubtaskRunner runner;
		private readonly int batchSize;
		private readonly Profiler.TimelineTrack profilerTrack;

		private volatile bool isRunning;
		private int totalSubtaskCount;
		private int remainingBatches;

		public SubtaskExecutor(Runner.SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
		{
			this.runner = runner;
			this.batchSize = batchSize;
			this.profilerTrack = profiler?.CreateTrack<Profiler.TimelineTrack>(GetType().Name);
		}

		public void QuerySubtasks()
		{
			totalSubtaskCount = PrepareSubtasks();
		}

		public void RunSubtasks()
		{
			if(isRunning)
				throw new Exception($"[{nameof(SubtaskExecutor)}] Allready running!");
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

		void Runner.ExecuteInfo.ISubtaskExecutor.ExecuteSubtask(int minSubtaskIndex, int maxSubtaskIndex)
		{
			try
			{
				for (int i = minSubtaskIndex; i <= maxSubtaskIndex; i++)
					ExecuteSubtask(i);
			} catch (Exception) { }

			if(Interlocked.Decrement(ref remainingBatches) == 0)
				Complete();
		}

		private void Complete()
		{
			profilerTrack?.LogEndWork();
			isRunning = false;
			
			Completed?.Invoke();
		}

		protected abstract int PrepareSubtasks();
		protected abstract void ExecuteSubtask(int index);
    }
}