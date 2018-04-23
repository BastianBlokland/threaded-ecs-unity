using Utils;
using ECS.Storage;
using ECS.Systems;
using UnityEngine;

using EntityID = System.UInt16;

namespace Test
{
    public class ApplyVelocitySystem : System<VelocityComponent, TransformComponent>
    {
		private readonly DeltaTimeHandle deltaTime;

		public ApplyVelocitySystem(DeltaTimeHandle deltaTime, EntityContext context) : base(context)
		{
			this.deltaTime = deltaTime;
		}

        protected override void Execute(EntityID entity, ref VelocityComponent velo, ref TransformComponent trans)
		{
			trans.Position += velo.Velocity * deltaTime.Value;
		}
    }
}