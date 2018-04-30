namespace ECS.Tasks
{
	public abstract class SingleTask : RepeatedTask
    {
		public SingleTask() : base(batchSize: 1)
		{

		}

		protected sealed override int GetRepeatCount()
		{
			return 1;
		}
	}
}