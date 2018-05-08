using System;
using System.Threading;
using ECS.Tasks.Runner;
using Utils;

namespace ECS.Tasks
{
	public sealed class GroupTask : ITask
    {
		private readonly ITask[] innerTasks;

		public GroupTask(params ITask[] innerTasks)
		{
			this.innerTasks = innerTasks;
		}

		public ITaskExecutor CreateExecutor(SubtaskRunner runner, Logger logger = null, Profiler.Timeline profiler = null)
			=> new GroupExecutor(innerTasks, runner, logger, profiler);
	}
}