using ECS.Storage;
using UnityEngine;

namespace Test.Components
{
    public struct GraphicsComponent : IComponent
    {
        public byte GraphicsID;

		public GraphicsComponent(byte graphicsID)
		{
			GraphicsID = graphicsID;
		}
    }
}