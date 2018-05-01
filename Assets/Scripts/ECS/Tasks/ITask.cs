namespace ECS.Tasks
{
    public interface ITask
	{
		ITaskExecutor CreateExecutor(Runner.SubtaskRunner runner);
	}
}