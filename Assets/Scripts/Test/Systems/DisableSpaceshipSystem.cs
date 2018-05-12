using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class DisableSpaceshipSystem : EntityTask<AgeComponent>
    {
		private readonly EntityContext context;

		public DisableSpaceshipSystem(EntityContext context) : base(context, batchSize: 100)
		{
			this.context = context;
		}

        protected override void Execute(int execID, EntityID entity, ref AgeComponent age)
		{
			if(age.Value > 10) //Engines apparently break after 10 seconds :)
			{
				//Slightly hacky but first reset the age to 0 before, unsetting the 'AgeComponent' tag so that the age wont be incremented
				//this way anyway asking for the age will get 0 instead of the last value
				context.SetComponent(entity, new AgeComponent());
				context.RemoveTag<AgeComponent>(entity);

				context.SetTag<GravityComponent>(entity);
				context.SetTag<DisabledComponent>(entity);
			}
		}

		//Require the spaceship tag to be set so we only operate on spaceships
		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) + context.GetMask<SpaceshipComponent>();	

		//Ignore allready disabled spaceships
		protected override TagMask GetIllegalTags(EntityContext context)
			=> base.GetIllegalTags(context) + context.GetMask<DisabledComponent>();
    }
}