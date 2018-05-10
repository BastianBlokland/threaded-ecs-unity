using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class LifetimeSystem : EntityTask<AgeComponent, LifetimeComponent>
    {
		private readonly EntityContext context;

		public LifetimeSystem(EntityContext context) : base(context, batchSize: 100)
		{
			this.context = context;
		}

        protected override void Execute(int execID, EntityID entity, ref AgeComponent age, ref LifetimeComponent lifetime)
		{
			if(age.Value > lifetime.TotalLifetime)
				context.RemoveEntity(entity);
		}
    }
}