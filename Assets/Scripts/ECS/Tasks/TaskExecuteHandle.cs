using System;
using System.Collections.Generic;
using System.Threading;
using ECS.Storage;

using EntityID = System.UInt16;

namespace ECS.Tasks
{
    public class TaskExecuteHandle : SubtaskRunner.ISubtaskExecutor
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
		private readonly Profiler.TimelineTrack timelineTrack;

		private bool isScheduled;
		private CountdownEvent countdownEvent;

		public TaskExecuteHandle(IExecutableTask task, SubtaskRunner runner, int batchSize, Profiler.TimelineTrack timelineTrack = null)
		{
			this.task = task;
			this.runner = runner;
			this.batchSize = batchSize;
			this.timelineTrack = timelineTrack;
		}

		public void Schedule()
		{
			if(isScheduled)
				return;
			isScheduled = true;

			if(timelineTrack != null)
				timelineTrack.LogStartWork();

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
			if(timelineTrack != null)
				timelineTrack.LogEndWork();
		}
		//----> RUNNING ON SEPARATE THREAD
    }
}