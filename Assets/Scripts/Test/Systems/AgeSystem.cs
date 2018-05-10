using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class AgeSystem : EntityTask<AgeComponent>
    {
		private readonly DeltaTimeHandle deltaTime;

		public AgeSystem(DeltaTimeHandle deltaTime, EntityContext context) : base(context, batchSize: 100)
		{
			this.deltaTime = deltaTime;
		}

        protected override void Execute(int execID, EntityID entity, ref AgeComponent age)
		{
			age.Value += deltaTime.Value;
		}
    }
}