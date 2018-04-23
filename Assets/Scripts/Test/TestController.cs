using ECS.Storage;
using ECS.Systems;
using UnityEngine;
using Utils;
using EntityID = System.UInt16;

namespace Test
{
	public class TestController : MonoBehaviour
	{
		private EntityContext entityContext;
		private DeltaTimeHandle deltaTime;
		private SystemManager systemManager;

		protected void Awake()
		{
			entityContext = new EntityContext();
			deltaTime = new DeltaTimeHandle();
			systemManager = new SystemManager(new []
			{
				new [] { new ApplyVelocitySystem(deltaTime, entityContext) }
			});

			EntityID entity = entityContext.CreateEntity();
			entityContext.SetComponent(entity, new TransformComponent());
			entityContext.SetComponent(entity, new VelocityComponent { Velocity = Vector3.forward });
		}
		
		protected void Update()
		{
			deltaTime.Update(Time.deltaTime);
			systemManager.Schedule();
		}
	}
}