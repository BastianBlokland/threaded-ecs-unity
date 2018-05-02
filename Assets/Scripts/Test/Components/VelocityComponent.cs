using ECS.Storage;
using UnityEngine;

namespace Test.Components
{
    public struct VelocityComponent : IDataComponent
    {
        public Vector3 Velocity;

		public VelocityComponent(Vector3 velocity)
		{
			Velocity = velocity;
		}
    }
}