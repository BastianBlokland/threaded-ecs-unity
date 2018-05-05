using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using Test.Systems;
using UnityEngine;
using Utils;

namespace Test
{
	public class TestController : MonoBehaviour
	{
		[SerializeField] private int executorCount = 1;
		[SerializeField] private int cubeCount = 100;
		[SerializeField] private GraphicsAssetsLibrary assetsLibrary;
		[SerializeField] private Profiler.Timeline timeline;

		private SubtaskRunner subtaskRunner;
		private EntityContext entityContext;
		private DeltaTimeHandle deltaTime;
		private IRandomProvider random;
		private RenderSet renderSet;
		private TaskManager systemManager;

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

			Application.targetFrameRate = -1;

			subtaskRunner = new SubtaskRunner(executorCount);
			entityContext = new EntityContext();
			deltaTime = new DeltaTimeHandle();
			random = new ShiftRandomProvider();
			renderSet = new RenderSet(assetsLibrary);
			systemManager = new TaskManager(subtaskRunner, new ECS.Tasks.ITaskExecutor[]
			{
				new SpawnCubesSystem(cubeCount, random, entityContext, subtaskRunner, timeline),
				new ApplyVelocitySystem(deltaTime, entityContext, subtaskRunner, timeline),
				new ApplyGravitySystem(deltaTime, entityContext, subtaskRunner, timeline),
				new DestroyBelow0System(entityContext, subtaskRunner, timeline),
				new CreateRenderBatchesSystem(renderSet, entityContext, subtaskRunner, timeline)
			});

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
			blockMainTrack?.LogStartWork();
			{
				//Wait for the systems to be complete
				systemManager.Complete();
			}
			blockMainTrack?.LogEndWork();

			renderTrack?.LogStartWork();
			{
				//Render the results of the systems
				renderSet.Render();
			}
			renderTrack?.LogEndWork();

			//Setup the systems
			renderSet.Clear();
			deltaTime.Update(Time.deltaTime);

			scheduleTrack?.LogStartWork();
			{
				//Schedule the systems
				systemManager.Schedule();
			}
			scheduleTrack?.LogEndWork();
		}

		protected void OnDestroy()
		{
			if(entityContext != null)
			{
				entityContext.Dispose();
				entityContext = null;
			}
			if(subtaskRunner != null)
			{
				subtaskRunner.Dispose();
				subtaskRunner = null;
			}
		}
	}
}