using Utils;
using ECS.Storage;
using ECS.Tasks;
using UnityEngine;
using Test.Components;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class SpawnCubesSystem : RepeatedTask
    {
		private readonly int targetObjectCount;
		private readonly EntityContext context;

		public SpawnCubesSystem(int targetObjectCount, EntityContext context, Profiler.Timeline profiler) 
			: base(profiler)
		{
			this.targetObjectCount = targetObjectCount;
			this.context = context;
		}

		protected override int GetRepeatCount()
		{
			int currentCubeCount = context.GetEntityCount(requiredComps: context.GetMask<CubeComponent>(), illegalComps: ComponentMask.Empty);
			return Mathf.Max(0, targetObjectCount - currentCubeCount);
		}

        protected override void Execute()
		{
			EntityID entity = context.CreateEntity();
			context.SetComponent(entity, new TransformComponent(position: new Vector3(0f, 10f, 0f)));
			context.SetComponent(entity, new VelocityComponent());
			context.SetComponent(entity, new GraphicsComponent(graphicsID: 1));
			context.SetComponent(entity, new GravityComponent());
			context.SetComponent(entity, new CubeComponent());
		}
    }
}