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
		private readonly IRandomProvider random;
		private readonly EntityContext context;

		public SpawnSpaceshipSystem(int targetCount, IRandomProvider random, EntityContext context) : base(batchSize: 100)
		{
			this.targetCount = targetCount;
			this.random = random;
			this.context = context;
		}

		protected override int PrepareSubtasks()
		{
			int currentCount = context.GetEntityCount(requiredTags: context.GetMask<SpaceshipComponent>(), illegalTags: TagMask.Empty);
			return Max(0, targetCount - currentCount);
		}

		protected override void ExecuteSubtask(int execID, int index)
		{
			const float MIN_SPEED = 20f;
			const float MAX_SPEED = 40f;
			const float LIFETIME = 10f;

			AABox spawnArea = new AABox
			(
				min: new Vector3(-1000f, 50f, -150f),
				max: new Vector3(1000f, 100f, -75f)	
			);
			Vector3 position = random.Inside(spawnArea);
			Vector3 velocity = Vector3.forward * random.Between(MIN_SPEED, MAX_SPEED);

			var entity = context.CreateEntity();
			context.SetComponent(entity, new TransformComponent(Float3x4.FromPosition(position)));
			context.SetComponent(entity, new VelocityComponent(velocity: velocity));
			context.SetComponent(entity, new GraphicComponent(graphicID: 0));
			context.SetComponent(entity, new LifetimeComponent(totalLifetime: LIFETIME));
			context.SetComponent(entity, new AgeComponent());
			//context.SetTag<GravityComponent>(entity);
			context.SetTag<SpaceshipComponent>(entity);
		}
    }
}