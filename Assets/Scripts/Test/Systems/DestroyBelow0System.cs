using Utils;
using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using UnityEngine;
using Test.Components;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class DestroyBelow0System : EntityTask<TransformComponent>
    {
		private readonly EntityContext context;

		public DestroyBelow0System(EntityContext context, SubtaskRunner runner, Profiler.Timeline profiler) 
			: base(context, runner, 100, profiler)
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