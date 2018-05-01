namespace ECS.Tasks
{
	public abstract class RepeatedTask : ITask, SingleTaskExecutor.IExecutableTask
    {
		private readonly int batchSize;
		private readonly Profiler.TimelineTrack profilerTrack;

		public RepeatedTask(Profiler.Timeline profiler = null)
		{
			if(profiler != null)
				profilerTrack = profiler.CreateTrack<Profiler.TimelineTrack>(GetType().Name);
		}

		public ITaskExecutor CreateExecutor(Runner.SubtaskRunner runner)
		{
			return new SingleTaskExecutor(this, runner, profilerTrack);
		}

		int SingleTaskExecutor.IExecutableTask.PrepareSubtasks()
		{
			return GetRepeatCount();
		}

		void SingleTaskExecutor.IExecutableTask.ExecuteSubtask(int index)
		{
			Execute();
		}

		protected abstract int GetRepeatCount();

		protected abstract void Execute();
	}
}