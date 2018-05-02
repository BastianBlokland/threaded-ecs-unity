using Utils;
using ECS.Storage;
using ECS.Tasks;
using UnityEngine;
using Test.Components;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class DestroyBelow0System : EntityTask<TransformComponent>
    {
		private readonly EntityContext context;

		public DestroyBelow0System(EntityContext context, Profiler.Timeline profiler) 
			: base(context, batchSize: 100, profiler: profiler)
		{
			this.context = context;
		}

        protected override void Execute(EntityID entity, ref TransformComponent trans)
		{
			if(trans.Position.y <= 0)
				context.RemoveEntity(entity);
		}
    }
}