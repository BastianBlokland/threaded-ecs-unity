using ECS.Storage;
using ECS.Systems;
using Test.Components;
using Test.Systems;
using UnityEngine;
using Utils;

using EntityID = System.UInt16;

namespace Test
{
	public class TestController : MonoBehaviour
	{
		[SerializeField] private bool multiThreaded = true;
		[SerializeField] private GraphicsAssetsLibrary assetsLibrary;

		private EntityContext entityContext;
		private DeltaTimeHandle deltaTime;
		private RenderSet renderSet;
		private SystemManager systemManager;

		protected void Awake()
		{
			if(assetsLibrary == null)
			{
				Debug.LogError("[TestController] No 'GraphicsAssetsLibrary' provided!");
				return;
			}

			entityContext = new EntityContext();
			deltaTime = new DeltaTimeHandle();
			renderSet = new RenderSet(assetsLibrary);
			systemManager = new SystemManager(multiThreaded, new []
			{
				new ECS.Systems.System[] { new ApplyVelocitySystem(deltaTime, entityContext) },
				new ECS.Systems.System[] { new CreateRenderBatchesSystem(renderSet, entityContext) }
			});

			for (int i = 0; i < 5000; i++)
			{
				Vector3 position = new Vector3(Random.Range(-100f, 100f), 0, Random.Range(-100f, 100f));
				EntityID entity = entityContext.CreateEntity();
				entityContext.SetComponent(entity, new TransformComponent { Position = position, Rotation = Quaternion.identity });
				entityContext.SetComponent(entity, new VelocityComponent { Velocity = Vector3.up * Random.value });
				entityContext.SetComponent(entity, new GraphicsComponent { GraphicsID = 1 });	
			}
		}
		
		protected void Update()
		{
			//Wait for the systems to be complete
			systemManager.Complete();

			//Render the results of the systems
			renderSet.Render();

			//Setup the systems
			renderSet.Clear();
			deltaTime.Update(Time.deltaTime);

			//Schedule the systems
			systemManager.Schedule();
		}

		protected void OnDestroy()
		{
			if(systemManager != null)
			{
				systemManager.Dispose();
				systemManager = null;
			}
		}
	}
}