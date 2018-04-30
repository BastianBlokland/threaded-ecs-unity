namespace ECS.Tasks
{
	public abstract class SingleTask : ITask, TaskExecuteHandle.IExecutableTask
    {
		int TaskExecuteHandle.IExecutableTask.PrepareSubtasks()
		{
			return 1;
		}

		void TaskExecuteHandle.IExecutableTask.ExecuteSubtask(int index)
		{
			Execute();
		}

		protected abstract void Execute();
	}
}