using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using UnityEngine;
using Utils;
using Utils.Random;

using static UnityEngine.Mathf;

namespace Test.Systems
{
    public sealed class SpawnSpaceshipSystem : SingleTask
    {
		private readonly int targetCount;
		private readonly int maxSpawnPerIteration;
		private readonly IRandomProvider random;
		private readonly EntityContext context;

		public SpawnSpaceshipSystem(int targetCount, int maxPerIteration, IRandomProvider random, EntityContext context) : base(batchSize: 100)
		{
			this.targetCount = targetCount;
			this.maxSpawnPerIteration = maxPerIteration;
			this.random = random;
			this.context = context;
		}

		protected override int PrepareSubtasks()
		{
			int currentCount = context.GetEntityCount(requiredTags: context.GetMask<SpaceshipComponent>(), illegalTags: TagMask.Empty);
			return Max(0, Min(targetCount - currentCount, maxSpawnPerIteration));
		}

		protected override void ExecuteSubtask(int execID, int index)
		{
			const float MIN_SPEED = 15f;
			const float MAX_SPEED = 50f;

			AABox spawnArea = new AABox
			(
				min: new Vector3(-500f, 25f, -500f),
				max: new Vector3(500f, 75f, 500f)	
			);
			Vector3 position = random.Inside(spawnArea);
			Vector3 velocity = Vector3.forward * random.Between(MIN_SPEED, MAX_SPEED);

			var entity = context.CreateEntity();
			context.SetComponent(entity, new TransformComponent(Float3x4.FromPosition(position)));
			context.SetComponent(entity, new VelocityComponent(velocity: velocity));
			context.SetComponent(entity, new GraphicComponent(graphicID: 0));
			context.SetComponent(entity, new AgeComponent());
			context.SetComponent(entity, new LifetimeComponent(totalLifetime: 10));
			context.SetTag<SpaceshipComponent>(entity);
		}
    }
}