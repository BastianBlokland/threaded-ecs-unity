using ECS.Storage;
using UnityEngine;

namespace Demo
{
    public struct ColliderComponent : IComponent
    {
        public Vector3 Size;

        public ColliderComponent(Vector3 size)
        {
            Size = size;
        }
    }
}
