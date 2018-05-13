using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using UnityEngine;
using Utils;
using Utils.Rendering;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class TestCollisionSystem : EntityTask<TransformComponent, VelocityComponent>
    {
		private readonly DeltaTimeHandle deltaTime;
		private readonly ColliderManager colliderManager;
		private readonly EntityContext context;

		public TestCollisionSystem(DeltaTimeHandle deltaTime, ColliderManager colliderManager, EntityContext context) 
			: base(context, batchSize: 100)
		{
			this.deltaTime = deltaTime;
			this.colliderManager = colliderManager;
			this.context = context;
		}

        protected override void Execute(int execID, EntityID entity, ref TransformComponent trans, ref VelocityComponent velo)
		{
			Vector3 pos = trans.Matrix.Position;
			Vector3 nextPos = pos + velo.Velocity * deltaTime.Value;
			Line line = new Line(pos, nextPos);

			EntityID target;
			if(colliderManager.Intersect(line, out target))
			{
				//Remove the projectile
				context.RemoveEntity(entity);

				//Mark the target as hit
				context.SetTag<HitTag>(target);

				//Spawn a impact
				EntityID impactEntity = context.CreateEntity();
				context.SetComponent(impactEntity, new TransformComponent(Float3x4.FromPosition(pos)));
				context.SetComponent(impactEntity, new GraphicComponent(graphicID: 3));
				context.SetComponent(impactEntity, new LifetimeComponent(totalLifetime: 1));
				context.SetComponent(impactEntity, new AgeComponent());
			}
		}

		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) + context.GetMask<ProjectileTag>();
    }
}