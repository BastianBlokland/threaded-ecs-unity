namespace ECS.Tasks
{
	public abstract class RepeatedTask : ITask, TaskExecuteHandle.IExecutableTask
    {
		int TaskExecuteHandle.IExecutableTask.PrepareSubtasks()
		{
			return GetRepeatCount();
		}

		void TaskExecuteHandle.IExecutableTask.ExecuteSubtask(int index)
		{
			Execute();
		}

		protected abstract int GetRepeatCount();

		protected abstract void Execute();
	}
}