using Utils;
using ECS.Storage;
using ECS.Systems;
using UnityEngine;
using Test.Components;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class ApplyVelocitySystem : System<VelocityComponent, TransformComponent>
    {
		private readonly DeltaTimeHandle deltaTime;

		public ApplyVelocitySystem(DeltaTimeHandle deltaTime, EntityContext context, int batchSize) : base(context, batchSize)
		{
			this.deltaTime = deltaTime;
		}

        protected override void Execute(EntityID entity, ref VelocityComponent velo, ref TransformComponent trans)
		{
			trans.Position += velo.Velocity * deltaTime.Value;
		}
    }
}