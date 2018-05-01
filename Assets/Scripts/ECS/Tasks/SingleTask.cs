namespace ECS.Tasks
{
	public abstract class SingleTask : RepeatedTask
    {
		protected sealed override int GetRepeatCount()
		{
			return 1;
		}
	}
}