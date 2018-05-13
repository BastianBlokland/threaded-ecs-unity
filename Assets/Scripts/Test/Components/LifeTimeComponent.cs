using ECS.Storage;

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