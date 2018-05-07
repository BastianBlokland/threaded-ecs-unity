using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class LifetimeSystem : EntityTask<LifetimeComponent>
    {
		private readonly DeltaTimeHandle deltaTime;
		private readonly EntityContext context;

		public LifetimeSystem(DeltaTimeHandle deltaTime, EntityContext context, SubtaskRunner runner, Profiler.Timeline profiler) 
			: base(context, runner, 100, profiler)
		{
			this.deltaTime = deltaTime;
			this.context = context;
		}

        protected override void Execute(int execID, EntityID entity, ref LifetimeComponent lifetime)
		{
			lifetime.RemainingLifetime -= deltaTime.Value;
			if(lifetime.RemainingLifetime <= 0)
				context.RemoveEntity(entity);
		}
    }
}