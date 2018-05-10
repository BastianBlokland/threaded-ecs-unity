using ECS.Storage;
using ECS.Tasks;
using Test.Components;
using Utils;
using Utils.Rendering;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public sealed class RegisterRenderObjectsSystem : EntityTask<GraphicComponent, AgeComponent, TransformComponent>
    {
		private readonly RenderManager renderManager;

		public RegisterRenderObjectsSystem(RenderManager renderManager, EntityContext context) : base(context, batchSize: 100)
		{
			this.renderManager = renderManager;
		}

        protected override void Execute(int execID, EntityID entity, ref GraphicComponent graphics, ref AgeComponent age, ref TransformComponent trans)
		{
			renderManager.Add(execID, graphics.GraphicID, trans.Matrix, age.Value);
		}

		//Note: Slightly strange but this system on have a 'GraphicComponent' the other two components are optional
		//what will happen is that the values of 'age' and 'transform' will be 'random' as they have not been initialized
		//but if you don't use those in the shader then there won't be any problem. Need to think about if there are more
		//elegant ways to do this, but at least it allows a single system to provide all the data for all the different
		//graphic types and then the shader can decide what data it will actually use
		protected override TagMask GetRequiredTags(EntityContext context)
			=> context.GetMask<GraphicComponent>();	
    }
}