using ECS.Storage;
using UnityEngine;

namespace Test.Components
{
    public struct GraphicsComponent : IDataComponent
    {
        public byte GraphicsID;

		public GraphicsComponent(byte graphicsID)
		{
			GraphicsID = graphicsID;
		}
    }
}