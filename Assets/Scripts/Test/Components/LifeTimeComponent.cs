using ECS.Storage;
using UnityEngine;

namespace Test.Components
{
    public struct LifetimeComponent : IComponent
    {
		public float TotalLifetime;

		public LifetimeComponent(float totalLifetime)
		{
			TotalLifetime = totalLifetime;
		}
    }
}