using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using UnityEngine;
using Utils;
using Utils.Rendering;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class RegisterColliderSystem : EntityTask<ColliderComponent, TransformComponent>
    {
		private readonly ColliderManager colliderManager;

		public RegisterColliderSystem(ColliderManager colliderManager, EntityContext context) : base(context, batchSize: 100)
		{
			this.colliderManager = colliderManager;
		}

        protected override void Execute(int execID, EntityID entity, ref ColliderComponent collider, ref TransformComponent trans)
		{
			Vector3 pos = trans.Matrix.Position;
			AABox box = AABox.FromCenterAndExtents(pos, collider.Size);
			colliderManager.Add(box, entity);
		}
    }
}