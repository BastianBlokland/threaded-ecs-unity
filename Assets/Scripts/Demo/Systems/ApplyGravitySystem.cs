using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using UnityEngine;
using Utils;

using EntityID = System.UInt16;

namespace Demo
{
    public sealed class ApplyGravitySystem : EntityTask<VelocityComponent>
    {
		public const float GRAVITY = -9.81f;

		private readonly DeltaTimeHandle deltaTime;

		public ApplyGravitySystem(DeltaTimeHandle deltaTime, EntityContext context) 
			: base(context, batchSize: 100)
		{
			this.deltaTime = deltaTime;
		}

        protected override void Execute(int execID, EntityID entity, ref VelocityComponent velo)
		{
			velo.Velocity += Vector3.up * GRAVITY * deltaTime.Value;
		}

		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) + context.GetMask<ApplyGravityTag>();
    }
}