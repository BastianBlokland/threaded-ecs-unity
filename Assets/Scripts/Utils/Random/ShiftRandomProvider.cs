using System;

namespace Utils
{
	public class ShiftRandomProvider : IRandomProvider
	{
		private readonly object lockObject;
		private ushort lfsr;
		private ushort bit;

		public ShiftRandomProvider() : this(seed: (ushort)(DateTimeOffset.Now.UtcTicks % ushort.MaxValue)) { }

		public ShiftRandomProvider(ushort seed)
		{
			lockObject = new object();
			lfsr = seed > 0 ? seed : (ushort)1; //Seed of 0 is unsupported
			bit = 0;
		}

		public float GetNext()
		{
			float result = 0f;
			lock(lockObject)
			{
				//NOTE: Distribution with single shift is very poor
				//thats why we do multiple shifts to improve the distribution
				//after testing 7 seems to be a nice amount :)
				for (int i = 0; i < 7; i++)
					Shift(ref lfsr, ref bit);

				//Convert to standard 0 - 1 float range
				result = (float)lfsr / ushort.MaxValue;
			}
			return result;
		}

		private static void Shift(ref ushort lfsr, ref ushort bit)
		{
			//Implementation of: Linear Feedback Shift Register (https://en.wikipedia.org/wiki/Linear-feedback_shift_register)
			/* taps: 16 14 13 11; feedback polynomial: x^16 + x^14 + x^13 + x^11 + 1 */
			bit = (ushort)(((lfsr >> 0) ^ (lfsr >> 2) ^ (lfsr >> 3) ^ (lfsr >> 5)) & 1);
			lfsr = (ushort)((lfsr >> 1) | (bit << 15));
		}
	}
}