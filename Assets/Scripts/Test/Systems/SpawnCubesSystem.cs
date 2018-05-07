using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using Test.Components;
using UnityEngine;
using Utils;

using static UnityEngine.Mathf;

namespace Test.Systems
{
    public class SpawnCubesSystem : SubtaskExecutor
    {
		private readonly int targetObjectCount;
		private readonly IRandomProvider random;
		private readonly EntityContext context;

		public SpawnCubesSystem(int targetObjectCount, IRandomProvider random, 
			EntityContext context, SubtaskRunner runner, Profiler.Timeline profiler) : base(runner, 100, profiler)
		{
			this.targetObjectCount = targetObjectCount;
			this.random = random;
			this.context = context;
		}

		protected override int PrepareSubtasks()
		{
			int currentCubeCount = context.GetEntityCount(requiredTags: context.GetMask<CubeComponent>(), illegalTags: TagMask.Empty);
			return Max(0, targetObjectCount - currentCubeCount);
		}

		protected override void ExecuteSubtask(int execID, int index)
		{
			const float STARTING_HEIGHT = 100f;
			const float STARTING_SPEED = 25f;

			var entity = context.CreateEntity();
			context.SetComponent(entity, new TransformComponent(position: new Vector3(0f, STARTING_HEIGHT, 0f)));
			context.SetComponent(entity, new VelocityComponent(velocity: random.Direction3D() * STARTING_SPEED));
			context.SetComponent(entity, new GraphicsComponent(graphicsID: 1));
			context.SetComponent(entity, new LifetimeComponent(lifetime: random.GetNext() * 5f));
			context.SetTag<GravityComponent>(entity);
			context.SetTag<CubeComponent>(entity);
		}
    }
}