using ECS;
using UnityEngine;

namespace Test
{
    public struct TransformComponent : IComponent
    {
        public Vector3 Position;
		public Quaternion Rotation;
    }
}