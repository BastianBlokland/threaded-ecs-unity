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
		[SerializeField] private int executorCount = 1;
		[SerializeField] private int batchSize = 50;
		[SerializeField] private GraphicsAssetsLibrary assetsLibrary;
		[SerializeField] private Profiler.Timeline timeline;

		private EntityContext entityContext;
		private DeltaTimeHandle deltaTime;
		private RenderSet renderSet;
		private SystemManager systemManager;

		private Profiler.TimelineTrack blockMainTrack;
		private Profiler.TimelineTrack renderTrack;
		private Profiler.TimelineTrack scheduleTrack;

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
			systemManager = new SystemManager(executorCount, batchSize, timeline, new []
			{
				new ECS.Systems.System[] { new ApplyVelocitySystem(deltaTime, entityContext) },
				new ECS.Systems.System[] { new CreateRenderBatchesSystem(renderSet, entityContext) }
			});

			for (int i = 0; i < EntityID.MaxValue; i++)
			{
				Vector3 position = new Vector3(Random.Range(-100f, 100f), 0, Random.Range(-100f, 100f));
				EntityID entity = entityContext.CreateEntity();
				entityContext.SetComponent(entity, new TransformComponent { Position = position, Rotation = Quaternion.identity });
				entityContext.SetComponent(entity, new VelocityComponent { Velocity = Vector3.up * Random.value });
				entityContext.SetComponent(entity, new GraphicsComponent { GraphicsID = (byte)Random.Range(1, 4) });	
			}

			if(timeline != null)
			{
				blockMainTrack = timeline.CreateTrack<Profiler.TimelineTrack>("Finishing systems on main");
				renderTrack = timeline.CreateTrack<Profiler.TimelineTrack>("Rendering");
				scheduleTrack = timeline.CreateTrack<Profiler.TimelineTrack>("Scheduling");
				timeline.StartTimers();
			}
		}
		
		protected void Update()
		{
			if(blockMainTrack != null) blockMainTrack.LogStartWork();
			{
				//Wait for the systems to be complete
				systemManager.Complete();
			}
			if(blockMainTrack != null) blockMainTrack.LogEndWork();

			if(renderTrack != null) renderTrack.LogStartWork();
			{
				//Render the results of the systems
				renderSet.Render();
			}
			if(renderTrack != null) renderTrack.LogEndWork();

			//Setup the systems
			renderSet.Clear();
			deltaTime.Update(Time.deltaTime);

			if(scheduleTrack != null) scheduleTrack.LogStartWork();
			{
				//Schedule the systems
				systemManager.Schedule();
			}
			if(scheduleTrack != null) scheduleTrack.LogEndWork();
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