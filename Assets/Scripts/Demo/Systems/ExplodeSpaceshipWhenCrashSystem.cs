using ECS.Storage;
using ECS.Tasks;
using Utils;

using EntityID = System.UInt16;

namespace Demo
{
    public sealed class ExplodeSpaceshipWhenCrashSystem : EntityTask<TransformComponent>
    {
		private readonly EntityContext context;

		public ExplodeSpaceshipWhenCrashSystem(EntityContext context) 
			: base(context, batchSize: 100)
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

		//Only operate on disabled spaceships
		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) + context.GetMask<SpaceshipTag>() + context.GetMask<DisabledTag>();
    }
}