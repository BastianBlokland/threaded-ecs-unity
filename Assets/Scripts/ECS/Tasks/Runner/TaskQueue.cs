using System.Collections.Concurrent;

namespace ECS.Tasks.Runner
{
	public class TaskQueue
	{
		private readonly ConcurrentQueue<ExecuteInfo> queue;
	
		public TaskQueue()
		{
			queue = new ConcurrentQueue<ExecuteInfo>();
		}

		public void PushTask(ExecuteInfo executeInfo)
		{
			queue.Enqueue(executeInfo);
		}

		public ExecuteInfo? GetTask()
		{
			ExecuteInfo info;
			if(queue.TryDequeue(out info))
				return info;
			return null;
		}
	}
}