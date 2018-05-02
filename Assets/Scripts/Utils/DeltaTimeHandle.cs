namespace Utils
{
    public sealed class DeltaTimeHandle
    {
        public float Value { get; private set; }

		public void Update(float deltaTime)
		{
			Value = deltaTime;
		}
    }
}