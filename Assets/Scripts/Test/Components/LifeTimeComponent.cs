using ECS.Storage;
using UnityEngine;

namespace Test.Components
{
    public struct LifetimeComponent : IComponent
    {
		public float RemainingLifetime;

		public LifetimeComponent(float lifetime)
		{
			RemainingLifetime = lifetime;
		}
    }
}