using ECS.Storage;
using ECS.Tasks;
using UnityEngine;
using Utils;
using Utils.Random;

using static UnityEngine.Mathf;

namespace Demo
{
    public sealed class SpawnSpaceshipSystem : SingleTask
    {
        const int MAX_SPAWN_PER_ITERATION = 5;

        private readonly AABox spawnArea;
        private readonly int targetCount;
        private readonly IRandomProvider random;
        private readonly EntityContext context;

        public SpawnSpaceshipSystem(AABox spawnArea, int targetCount, IRandomProvider random, EntityContext context)
            : base(batchSize: 100)
        {
            this.spawnArea = spawnArea;
            this.targetCount = targetCount;
            this.random = random;
            this.context = context;
        }

        protected override int PrepareSubtasks()
        {
            int currentCount = context.GetEntityCount(requiredTags: context.GetMask<SpaceshipTag>(), illegalTags: TagMask.Empty);
            return Max(0, Min(targetCount - currentCount, MAX_SPAWN_PER_ITERATION));
        }

        protected override void ExecuteSubtask(int execID, int index)
        {
            Vector3 position = random.Inside(spawnArea);
            Vector3 velocity = Vector3.forward * random.Between(10f, 15f);

            var entity = context.CreateEntity();
            context.SetComponent(entity, new TransformComponent(Float3x4.FromPosition(position)));
            context.SetComponent(entity, new VelocityComponent(velocity: velocity));
            context.SetComponent(entity, new GraphicComponent(graphicID: 0));
            context.SetComponent(entity, new AgeComponent());
            context.SetComponent(entity, new LifetimeComponent(totalLifetime: random.Between(30f, 35f)));
            context.SetComponent(entity, new ColliderComponent(size: new Vector3(3, 2, 3)));
            context.SetTag<SpaceshipTag>(entity);
        }
    }
}
