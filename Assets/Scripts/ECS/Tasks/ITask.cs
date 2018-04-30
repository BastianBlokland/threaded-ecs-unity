namespace ECS.Tasks
{
    public interface ITask
	{
		ITaskExecutor CreateExecutor(SubtaskRunner runner);
	}
}