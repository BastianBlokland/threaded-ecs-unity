namespace ECS.Tasks
{
	public abstract class RepeatedTask : ITask, SingleTaskExecutor.IExecutableTask
    {
		private readonly int batchSize;
		private readonly Profiler.TimelineTrack profilerTrack;

		public RepeatedTask(Profiler.Timeline profiler = null, int batchSize = 50)
		{
			this.batchSize = batchSize;
			
			if(profiler != null)
				profilerTrack = profiler.CreateTrack<Profiler.TimelineTrack>(GetType().Name);
		}

		public ITaskExecutor CreateExecutor(SubtaskRunner runner)
		{
			return new SingleTaskExecutor(this, runner, batchSize, profilerTrack);
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