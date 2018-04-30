using Utils;
using ECS.Storage;
using ECS.Tasks;
using UnityEngine;
using Test.Components;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class ApplyGravitySystem : EntityTask<VelocityComponent>
    {
		private const float GRAVITY = -9.81f;

		private readonly DeltaTimeHandle deltaTime;

		public ApplyGravitySystem(DeltaTimeHandle deltaTime, EntityContext context, Profiler.Timeline profiler) 
			: base(context, profiler)
		{
			this.deltaTime = deltaTime;
		}

        protected override void Execute(EntityID entity, ref VelocityComponent velo)
		{
			velo.Velocity += Vector3.up * GRAVITY * deltaTime.Value;
		}

		protected override ComponentMask GetRequiredComponents(EntityContext context)
		{
			 return base.GetRequiredComponents(context)
			 	.Add(context.GetMask<GravityComponent>());
		}
    }
}