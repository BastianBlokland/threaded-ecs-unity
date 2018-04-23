using ECS.Storage;
using UnityEngine;

namespace Test.Components
{
    public struct TransformComponent : IComponent
    {
        public Vector3 Position;
		public Quaternion Rotation;
    }
}