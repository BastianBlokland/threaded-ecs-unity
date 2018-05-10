using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;
using Utils.Rendering;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class RegisterRenderObjects : EntityTask<GraphicComponent, TransformComponent>
    {
		private readonly RenderManager renderManager;

		public RegisterRenderObjects(RenderManager renderManager, EntityContext context) : base(context, batchSize: 100)
		{
			this.renderManager = renderManager;
		}

        protected override void Execute(int execID, EntityID entity, ref GraphicComponent graphics, ref TransformComponent trans)
		{
			renderManager.Add(execID, graphics.GraphicID, trans.Matrix);
		}
    }
}