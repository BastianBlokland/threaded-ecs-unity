namespace ECS.Tasks
{
	public class GroupTask : ITask
    {
		private readonly ITask[] tasks;

		public GroupTask(params ITask[] tasks)
		{
			this.tasks = tasks;
		}

		public ITaskExecutor CreateExecutor(SubtaskRunner runner)
		{
			return new GroupTaskExecutor(runner, tasks);
		}
	}
}