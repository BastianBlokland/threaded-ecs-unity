using ECS.Storage;

namespace Test.Components
{
    public struct GraphicComponent : IComponent
    {
        public byte GraphicID;

		public GraphicComponent(byte graphicID)
		{
			GraphicID = graphicID;
		}
    }
}