using ECS.Storage;
using ECS.Tasks;
using UnityEngine;
using Utils;

using EntityID = System.UInt16;
using static Utils.MathUtils;

namespace Demo
{
    public sealed class ApplyVelocitySystem : EntityTask<VelocityComponent, TransformComponent>
    {
		private readonly DeltaTimeHandle deltaTime;

		public ApplyVelocitySystem(DeltaTimeHandle deltaTime, EntityContext context) 
			: base(context, batchSize: 100)
		{
			this.deltaTime = deltaTime;
		}

        protected override void Execute(int execID, EntityID entity, ref VelocityComponent velo, ref TransformComponent trans)
		{
			if(velo.Velocity == Vector3.zero)
				return;
			Vector3 newPos = trans.Matrix.Position + velo.Velocity * deltaTime.Value;
			Vector3 dir = FastNormalize(velo.Velocity);
			trans.Matrix = Float3x4.FromPositionAndForward(newPos, dir);
		}
    }
}