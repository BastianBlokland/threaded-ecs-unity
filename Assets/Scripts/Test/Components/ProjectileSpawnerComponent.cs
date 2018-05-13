using ECS.Storage;

using EntityID = System.UInt16;

namespace Test.Components
{
    public struct ProjectileSpawnerComponent : IComponent
    {
        public int ShotsRemaining;
		public float Cooldown;
		public EntityID? Target;

		public ProjectileSpawnerComponent(float cooldown)
		{
			ShotsRemaining = 0;
			Cooldown = cooldown;
			Target = null;
		}
    }
}