using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils
{
	public static class MathUtils
	{
		[StructLayout(LayoutKind.Explicit, Size = 4)]
		private struct IntFloat 
		{
			[FieldOffset(0)]
			public float FloatValue;

			[FieldOffset(0)]
			public int IntValue;
		}

		//Implementation based on: https://en.wikipedia.org/wiki/Fast_inverse_square_root
		public static float FastInvSqrRoot(float number)
		{
			IntFloat conv = new IntFloat { FloatValue = number }; 

			float x2 = number * .5f;
	 		conv.IntValue = 0x5f3759df - (conv.IntValue >> 1);
	 		conv.FloatValue = conv.FloatValue * (1.5f - (x2 * conv.FloatValue * conv.FloatValue));
	 		return conv.FloatValue;
		}

		public static Vector2 FastNormalize(Vector2 vector)
		{
			float sqrMag = vector.sqrMagnitude;
			float invSqrRoot = FastInvSqrRoot(sqrMag);
			return new Vector2(vector.x * invSqrRoot, vector.y * invSqrRoot);
		}

		public static Vector3 FastNormalize(Vector3 vector)
		{
			float sqrMag = vector.sqrMagnitude;
			float invSqrRoot = FastInvSqrRoot(sqrMag);
			return new Vector3(vector.x * invSqrRoot, vector.y * invSqrRoot, vector.z * invSqrRoot);
		}

		public static bool DoesRangeOverlap(float aStart, float aEnd, float bStart, float bEnd)
		{
			return aEnd >= bStart && aStart <= bEnd;
		}
	}
}