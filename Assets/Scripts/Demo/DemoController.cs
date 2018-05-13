using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using UnityEngine;
using Utils;
using Utils.Random;
using Utils.Rendering;

namespace Demo
{
	public class DemoController : MonoBehaviour
	{
		[SerializeField] private Vector3 minCollisionArea = new Vector3(-250f, 0f, -250f);
		[SerializeField] private Vector3 maxCollisionArea = new Vector3(250f, 100f, 250f);

		[SerializeField] private Vector3 minSpaceshipSpawnArea = new Vector3(-150f, 25f, -150f);
		[SerializeField] private Vector3 maxSpaceshipSpawnArea = new Vector3(150f, 150f, -100f);

		[SerializeField] private Vector3 minTurretSpawnArea = new Vector3(-100f, 1f, -100f);
		[SerializeField] private Vector3 maxTurretSpawnArea = new Vector3(100f, 1f, 100f);

		[SerializeField] private int executorCount = 1;
		[SerializeField] private int spaceshipCount = 1000;
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

		protected void Awake()
		{
			if(assetLibrary == null)
			{
				Debug.LogError($"[{nameof(DemoController)}] No '{nameof(GraphicAssetLibrary)}' provided!");
				return;
			}

			//Create utilities
			logger = new Utils.Logger(UnityEngine.Debug.Log);
			subtaskRunner = new SubtaskRunner(executorCount);
			entityContext = new EntityContext();
			deltaTime = new DeltaTimeHandle();
			random = new ShiftRandomProvider();
			renderManager = new RenderManager(executorCount, assetLibrary);
			colliderManager = new ColliderManager(new AABox(minCollisionArea, maxCollisionArea));

			//Create systems
			systemManager = new TaskManager(subtaskRunner, new ECS.Tasks.ITask[]
			{
				new ApplyVelocitySystem(deltaTime, entityContext),
				new RegisterColliderSystem(colliderManager, entityContext),
				new TestCollisionSystem(deltaTime, colliderManager, entityContext),
				new ApplyGravitySystem(deltaTime, entityContext),

				new ExplodeSpaceshipWhenCrashSystem(entityContext),
				new DisableSpaceshipWhenHitSystem(entityContext),

				new SpawnProjectilesSystem(colliderManager, deltaTime, entityContext),
				new SpawnTurretSystem(new AABox(minTurretSpawnArea, maxTurretSpawnArea), turretCount, random, entityContext),
				new SpawnSpaceshipSystem(new AABox(minSpaceshipSpawnArea, maxSpaceshipSpawnArea), spaceshipCount, random, entityContext),

				new AgeSystem(deltaTime, entityContext),
				new LifetimeSystem(entityContext),

				new RegisterRenderObjectsSystem(renderManager, entityContext),
			}, logger, timeline);

			timeline?.StartTimers();
		}
		
		protected void Update()
		{
			//Wait for the systems to be complete
			systemManager.Complete();

			//Render the results of the systems
			renderManager.Render();

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
			AABox collisionArea = new AABox(minCollisionArea, maxCollisionArea);
			Gizmos.color = new Color(0f, 1f, 0f, .1f);
			Gizmos.DrawCube(collisionArea.Center, collisionArea.Size);
		}

		protected void OnDestroy()
		{
			subtaskRunner?.Dispose();
			renderManager?.Dispose();
		}
	}
}