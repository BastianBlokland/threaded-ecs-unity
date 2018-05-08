using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class ApplyVelocitySystem : EntityTask<VelocityComponent, TransformComponent>
    {
		private readonly DeltaTimeHandle deltaTime;

		public ApplyVelocitySystem(DeltaTimeHandle deltaTime, EntityContext context) : base(context, batchSize: 100)
		{
			this.deltaTime = deltaTime;
		}

        protected override void Execute(int execID, EntityID entity, ref VelocityComponent velo, ref TransformComponent trans)
		{
			trans.Position += velo.Velocity * deltaTime.Value;
		}
    }
}