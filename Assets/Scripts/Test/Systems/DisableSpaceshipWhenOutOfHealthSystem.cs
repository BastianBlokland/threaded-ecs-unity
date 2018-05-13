using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class DisableSpaceshipWhenOutOfHealthSystem : EntityTask<HealthComponent>
    {
		private readonly EntityContext context;

		public DisableSpaceshipWhenOutOfHealthSystem(EntityContext context) : base(context, batchSize: 100)
		{
			this.context = context;
		}

        protected override void Execute(int execID, EntityID entity, ref HealthComponent health)
		{
			if(health.Health <= 0)
			{
				//Slightly hacky but set age to 0 'disable' the engine graphics
				context.SetComponent<AgeComponent>(entity, new AgeComponent());

				context.RemoveTag<HealthComponent>(entity);
				context.RemoveTag<ColliderComponent>(entity);
				context.RemoveTag<LifetimeComponent>(entity);
				context.RemoveTag<AgeComponent>(entity);

				context.SetTag<DisabledComponent>(entity);
				context.SetTag<GravityComponent>(entity);
			}
		}

		//Require the spaceship tag to be set so we only operate on spaceships
		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) + context.GetMask<SpaceshipComponent>();	
    }
}