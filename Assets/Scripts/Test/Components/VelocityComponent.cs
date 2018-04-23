using ECS.Storage;
using UnityEngine;

namespace Test.Components
{
    public struct VelocityComponent : IComponent
    {
        public Vector3 Velocity;
    }
}