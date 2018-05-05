using Utils;
using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using UnityEngine;
using Test.Components;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class ApplyVelocitySystem : EntityTask<VelocityComponent, TransformComponent>
    {
		private readonly DeltaTimeHandle deltaTime;

		public ApplyVelocitySystem(DeltaTimeHandle deltaTime, 
			EntityContext context, SubtaskRunner runner, Profiler.Timeline profiler) : base(context, runner, 100, profiler)
		{
			this.deltaTime = deltaTime;
		}

        protected override void Execute(EntityID entity, ref VelocityComponent velo, ref TransformComponent trans)
		{
			trans.Position += velo.Velocity * deltaTime.Value;
		}
    }
}