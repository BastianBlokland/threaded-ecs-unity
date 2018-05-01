namespace ECS.Tasks.Runner
{
	public interface ITaskSource
	{
		ExecuteInfo? GetTask(int executorID);
	}
}