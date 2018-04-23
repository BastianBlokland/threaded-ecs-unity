namespace Utils
{
    public class DeltaTimeHandle
    {
        public float Value { get; private set; }

		public void Update(float deltaTime)
		{
			Value = deltaTime;
		}
    }
}