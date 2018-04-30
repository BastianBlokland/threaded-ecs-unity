using ECS.Storage;
using UnityEngine;

namespace Test.Components
{
    public struct TransformComponent : IComponent
    {
        public Vector3 Position;
		public Quaternion Rotation;

		public TransformComponent(Vector3 position)
		{
			Position = position;
			Rotation = Quaternion.identity;
		}

		public TransformComponent(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}
    }
}