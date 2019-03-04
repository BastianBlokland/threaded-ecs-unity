using ECS.Storage;
using UnityEngine;

namespace Demo
{
    public struct VelocityComponent : IComponent
    {
        public Vector3 Velocity;

        public VelocityComponent(Vector3 velocity)
        {
            Velocity = velocity;
        }
    }
}
