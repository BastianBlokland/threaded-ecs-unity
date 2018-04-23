using Utils;
using ECS.Storage;
using ECS.Systems;
using UnityEngine;
using Test.Components;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class CreateRenderBatchesSystem : System<GraphicsComponent, TransformComponent>
    {
		private readonly RenderSet renderSet;

		public CreateRenderBatchesSystem(RenderSet renderSet, EntityContext context, int batchSize) : base(context, batchSize)
		{
			this.renderSet = renderSet;
		}

        protected override void Execute(EntityID entity, ref GraphicsComponent graphics, ref TransformComponent trans)
		{
			Matrix4x4 matrix = Matrix4x4.TRS(trans.Position, trans.Rotation, Vector3.one);
			renderSet.Add(graphics.GraphicsID, matrix);
		}
    }
}