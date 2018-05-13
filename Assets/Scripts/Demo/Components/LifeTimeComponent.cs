using ECS.Storage;

namespace Demo
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