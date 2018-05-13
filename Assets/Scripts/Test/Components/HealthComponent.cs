using ECS.Storage;

namespace Test.Components
{
    public struct HealthComponent : IComponent
    {
		public int Health;

		public HealthComponent(int health)
		{
			Health = health;
		}
    }
}