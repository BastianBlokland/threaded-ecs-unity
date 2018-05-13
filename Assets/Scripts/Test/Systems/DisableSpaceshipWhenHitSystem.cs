using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class DisableSpaceshipWhenHitSystem : EntityTask
    {
		private readonly EntityContext context;

		public DisableSpaceshipWhenHitSystem(EntityContext context) : base(context, batchSize: 100)
		{
			this.context = context;
		}

        protected override void Execute(int execID, EntityID entity)
		{
			//Slightly hacky but set age to 0 'disable' the engine graphics
			context.SetComponent<AgeComponent>(entity, new AgeComponent());

			context.RemoveTag<LifetimeComponent>(entity);
			context.RemoveTag<AgeComponent>(entity);

			context.SetTag<DisabledTag>(entity);
			context.SetTag<ApplyGravityTag>(entity);
		}

		//Require the spaceship tag and hit tag
		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) + context.GetMask<SpaceshipTag>() + context.GetMask<HitTag>();

		//Ignore ships that are allready disabled
		protected override TagMask GetIllegalTags(EntityContext context)
			=> base.GetIllegalTags(context) + context.GetMask<DisabledTag>();
    }
}