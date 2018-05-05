using System;
using System.Threading;

namespace ECS.Tasks
{
    public sealed class SingleTaskExecutor : ITaskExecutor, Runner.ExecuteInfo.ISubtaskExecutor
    {
		public interface IExecutableTask
		{
			int PrepareSubtasks();

			void ExecuteSubtask(int index);
		}

        //NOTE: VERY important to realize that this can be called from any thread
		public event Action Completed;

		private readonly IExecutableTask task;
		private readonly int batchSize;
		private readonly Runner.SubtaskRunner runner;
		private readonly Profiler.TimelineTrack profilerTrack;

		private bool isScheduled;
		private int remainingBatches;

		public SingleTaskExecutor(IExecutableTask task, Runner.SubtaskRunner runner, int batchSize, Profiler.TimelineTrack profilerTrack = null)
		{
			this.task = task;
			this.runner = runner;
			this.batchSize = batchSize;
			this.profilerTrack = profilerTrack;
		}

		public void Schedule()
		{
			if(isScheduled)
				return;
			isScheduled = true;

			profilerTrack?.LogStartWork();

			int subtaskCount = task.PrepareSubtasks();
			if(subtaskCount == 0)
				Complete();
			else
			{
				remainingBatches = (subtaskCount - 1) / batchSize + 1; //'Trick' to round up using integer division

				int startOffset = batchSize - 1;
				int maxIndex = subtaskCount - 1;
				for (int i = 0; i < subtaskCount; i += batchSize)
				{
					int start = i;
					int end = start + startOffset;
					runner.PushTask(this, start, end >= subtaskCount ? maxIndex : end);
				}
				runner.WakeExecutors();
			}
		}

		//----> RUNNING ON SEPARATE THREAD
		void Runner.ExecuteInfo.ISubtaskExecutor.ExecuteSubtask(int minSubtaskIndex, int maxSubtaskIndex)
		{
			try
			{
				for (int i = minSubtaskIndex; i <= maxSubtaskIndex; i++)
					task.ExecuteSubtask(i);
			} catch (Exception) { }

			if(Interlocked.Decrement(ref remainingBatches) == 0)
				Complete();
		}

		private void Complete()
		{
			profilerTrack?.LogEndWork();
			Completed?.Invoke();
		}
		//----> RUNNING ON SEPARATE THREAD
    }
}