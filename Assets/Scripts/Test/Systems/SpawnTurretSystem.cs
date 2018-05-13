using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using UnityEngine;
using Utils;
using Utils.Random;

using static UnityEngine.Mathf;

namespace Test.Systems
{
    public sealed class SpawnTurretSystem : SingleTask
    {
		private readonly int count;
		private readonly IRandomProvider random;
		private readonly EntityContext context;

		private bool setupDone;

		public SpawnTurretSystem(int count, IRandomProvider random, EntityContext context) : base(batchSize: 100)
		{
			this.count = count;
			this.random = random;
			this.context = context;
		}

		protected override int PrepareSubtasks()
		{
			if(setupDone)
				return 0;
			setupDone = true;
			return count;
		}

		protected override void ExecuteSubtask(int execID, int index)
		{
			AABox spawnArea = new AABox
			(
				min: new Vector3(-100f, 1f, -100f),
				max: new Vector3(100f, 1f, 100f)	
			);
			Vector3 position = random.Inside(spawnArea);

			var entity = context.CreateEntity();
			context.SetComponent(entity, new TransformComponent(Float3x4.FromPosition(position)));
			context.SetComponent(entity, new GraphicComponent(graphicID: 1));
			context.SetComponent(entity, new ProjectileSpawnerComponent(cooldown: random.GetNext() * 5f));
		}
    }
}