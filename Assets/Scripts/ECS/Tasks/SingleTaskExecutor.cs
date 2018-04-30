using System;
using System.Threading;

namespace ECS.Tasks
{
    public class SingleTaskExecutor : ITaskExecutor, SubtaskRunner.ISubtaskExecutor
    {
		public interface IExecutableTask
		{
			int PrepareSubtasks();

			void ExecuteSubtask(int index);
		}

        //NOTE: VERY important to realize that this can be called from any thread
		public event Action Completed = delegate {};

		private readonly IExecutableTask task;
		private readonly SubtaskRunner runner;
		private readonly int batchSize;
		private readonly Profiler.TimelineTrack profilerTrack;

		private bool isScheduled;
		private CountdownEvent countdownEvent;

		public SingleTaskExecutor(IExecutableTask task, SubtaskRunner runner, int batchSize, Profiler.TimelineTrack profilerTrack = null)
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

			if(profilerTrack != null)
				profilerTrack.LogStartWork();

			int subtaskCount = task.PrepareSubtasks();
			if(subtaskCount == 0)
				Complete();
			else
			{
				countdownEvent = new CountdownEvent(subtaskCount);

				int startOffset = batchSize - 1;
				for (int i = 0; i < subtaskCount; i += batchSize)
				{
					int start = i;
					int end = start + startOffset;
					runner.Schedule(this, start, end >= subtaskCount ? (subtaskCount - 1) : end);
				}
			}
		}

		//----> RUNNING ON SEPARATE THREAD
		void SubtaskRunner.ISubtaskExecutor.ExecuteSubtask(int subtaskIndex)
		{
			try { task.ExecuteSubtask(subtaskIndex); }
			catch(Exception) { }

			if(countdownEvent.Signal())
			{
				Complete();
				countdownEvent.Dispose();
			}
		}

		private void Complete()
		{
			Completed();
			if(profilerTrack != null)
				profilerTrack.LogEndWork();
		}
		//----> RUNNING ON SEPARATE THREAD
    }
}