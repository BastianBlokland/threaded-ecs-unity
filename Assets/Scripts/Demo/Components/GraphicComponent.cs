using ECS.Storage;

namespace Demo
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