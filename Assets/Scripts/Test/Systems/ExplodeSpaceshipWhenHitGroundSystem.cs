using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class ExplodeSpaceshipWhenHitGroundSystem : EntityTask<TransformComponent>
    {
		private readonly EntityContext context;

		public ExplodeSpaceshipWhenHitGroundSystem(EntityContext context) : base(context, batchSize: 100)
		{
			this.context = context;
		}

        protected override void Execute(int execID, EntityID entity, ref TransformComponent trans)
		{
			if(trans.Matrix.Position.y <= 0f) //We've hit the ground
			{
				//Remove spaceship
				context.RemoveEntity(entity);

				//Spawn explosion
				EntityID explosionEntity = context.CreateEntity();
				context.SetComponent(explosionEntity, new TransformComponent(Float3x4.FromPosition(trans.Matrix.Position)));
				context.SetComponent(explosionEntity, new GraphicComponent(graphicID: 4));
				context.SetComponent(explosionEntity, new LifetimeComponent(totalLifetime: 1));
				context.SetComponent(explosionEntity, new AgeComponent());
			}
		}

		//Require the spaceship tag to be set so we only operate on spaceships
		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) + context.GetMask<SpaceshipComponent>();	
    }
}