using System;
using ECS.Tasks.Runner;
using Utils;

namespace ECS.Tasks
{
    public interface ITask
	{
		ITaskExecutor CreateExecutor(SubtaskRunner runner, Logger logger = null, Profiler.Timeline profiler = null);
	}
}