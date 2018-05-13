using ECS.Storage;
using ECS.Tasks;
using Utils;

using EntityID = System.UInt16;

namespace Demo
{
    public sealed class DisableSpaceshipWhenHitSystem : EntityTask
    {
		private readonly EntityContext context;

		public DisableSpaceshipWhenHitSystem(EntityContext context) 
			: base(context, batchSize: 100)
		{
			this.context = context;
		}

        protected override void Execute(int execID, EntityID entity)
		{
			//Slightly hacky but set age to 0 'disable' the engine graphics
			context.SetComponent<AgeComponent>(entity, new AgeComponent());
			context.RemoveTag<AgeComponent>(entity); //Removed the 'AgeComponent' tag so that the age will stay at 0

			//Remove the collider when disabled to make it a bit cheaper
			context.RemoveTag<ColliderComponent>(entity);

			context.SetTag<DisabledTag>(entity); 
			context.SetTag<ApplyGravityTag>(entity); //Apply gravity so it will start falling
		}

		//Require the spaceship tag and hit tag
		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) + context.GetMask<SpaceshipTag>() + context.GetMask<HitTag>();

		//Ignore ships that are allready disabled
		protected override TagMask GetIllegalTags(EntityContext context)
			=> base.GetIllegalTags(context) + context.GetMask<DisabledTag>();
    }
}