using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using Test.Systems;
using UnityEngine;
using Utils;
using Utils.Random;
using Utils.Rendering;

namespace Test
{
	public class TestController : MonoBehaviour
	{
		[Header("Area within the action takes places (entities are destroyed when they leave")]
		[SerializeField] private Vector3 minArea = new Vector3(-250f, 0f, -250f);
		[SerializeField] private Vector3 maxArea = new Vector3(250f, 100f, 250f);

		[SerializeField] private int executorCount = 1;
		[SerializeField] private int spaceshipCount = 1000;
		[SerializeField] private int maxSpaceshipSpawnPerIteration = 100;
		[SerializeField] private int turretCount = 1000;
		[SerializeField] private GraphicAssetLibrary assetLibrary;
		[SerializeField] private Profiler.Timeline timeline;

		private Utils.Logger logger;
		private SubtaskRunner subtaskRunner;
		private EntityContext entityContext;
		private DeltaTimeHandle deltaTime;
		private IRandomProvider random;
		private RenderManager renderManager;
		private ColliderManager colliderManager;
		private TaskManager systemManager;

		private Profiler.TimelineTrack blockMainTrack;
		private Profiler.TimelineTrack renderTrack;

		protected void Awake()
		{
			if(assetLibrary == null)
			{
				Debug.LogError($"[{nameof(TestController)}] No '{nameof(GraphicAssetLibrary)}' provided!");
				return;
			}

			AABox area = new AABox(minArea, maxArea);

			logger = new Utils.Logger(UnityEngine.Debug.Log);
			subtaskRunner = new SubtaskRunner(executorCount);
			entityContext = new EntityContext();
			deltaTime = new DeltaTimeHandle();
			random = new ShiftRandomProvider();
			renderManager = new RenderManager(executorCount, assetLibrary);
			colliderManager = new ColliderManager(area);
			systemManager = new TaskManager(subtaskRunner, new ECS.Tasks.ITask[]
			{
				new ApplyGravitySystem(deltaTime, entityContext),
				new ApplyVelocitySystem(deltaTime, entityContext),
				new RegisterColliderSystem(colliderManager, entityContext),
				new TestCollisionSystem(deltaTime, colliderManager, entityContext),
				new AgeSystem(deltaTime, entityContext),
				new RegisterRenderObjectsSystem(renderManager, entityContext),
				new ExplodeSpaceshipWhenHitGroundSystem(entityContext),
				new SpawnProjectilesSystem(random, deltaTime, entityContext),
				new SpawnTurretSystem(turretCount, random, entityContext),
				new DisableSpaceshipWhenOutOfHealthSystem(entityContext),
				new SpawnSpaceshipSystem(spaceshipCount, maxSpaceshipSpawnPerIteration, random, entityContext),
				new LifetimeSystem(entityContext)
			}, logger, timeline);

			blockMainTrack = timeline?.CreateTrack<Profiler.TimelineTrack>("Finishing systems on main");
			renderTrack = timeline?.CreateTrack<Profiler.TimelineTrack>("Rendering");
			timeline?.StartTimers();
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
				renderManager.Render();
			}
			renderTrack?.LogEndWork();

			//Log any messages that where recorded on other threads
			logger?.Print();

			//Setup the systems
			renderManager.Clear();
			colliderManager.Clear();
			deltaTime.Update(Time.deltaTime);

			//Start the systems
			systemManager.Run();
		}

		protected void OnDrawGizmosSelected()
		{
			AABox area = new AABox(minArea, maxArea);
			Gizmos.color = new Color(0f, 1f, 0f, .1f);
			Gizmos.DrawCube(area.Center, area.Size);
		}

		protected void OnDestroy()
		{
			subtaskRunner?.Dispose();
			renderManager?.Dispose();
		}
	}
}