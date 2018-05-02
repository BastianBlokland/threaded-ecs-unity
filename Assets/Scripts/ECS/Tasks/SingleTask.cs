namespace ECS.Tasks
{
	public abstract class SingleTask : RepeatedTask
    {
		public SingleTask(int batchSize) : base(batchSize)
		{
			
		}

		protected sealed override int GetRepeatCount()
		{
			return 1;
		}
	}
}