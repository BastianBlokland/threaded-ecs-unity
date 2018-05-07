using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class CreateRenderBatchesSystem : EntityTask<GraphicsComponent, TransformComponent>
    {
		private readonly RenderSet renderSet;

		public CreateRenderBatchesSystem(RenderSet renderSet, EntityContext context) : base(context, batchSize: 100)
		{
			this.renderSet = renderSet;
		}

        protected override void Execute(int execID, EntityID entity, ref GraphicsComponent graphics, ref TransformComponent trans)
		{
			renderSet.Add(execID, graphics.GraphicsID, trans.Matrix);
		}
    }
}