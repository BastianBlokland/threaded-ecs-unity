using UnityEngine;

namespace Utils
{
	public static class RandomUtils
	{
		public static Vector3 Direction(this IRandomProvider random)
		{
			Vector3 dir = new Vector3
			(
				x: random.GetNext() - .5f, 
				y: random.GetNext() - .5f,
				z: random.GetNext() - .5f
			);
			if(dir == Vector3.zero) //Should be very rare
				return Vector3.forward;
			return MathUtils.FastNormalize(dir);
		}

		public static int Between(this IRandomProvider random, int minValue, int maxValue)
		{
			return Mathf.FloorToInt(random.Between((float)minValue, (float)maxValue));
		}

		public static float Between(this IRandomProvider random, float minValue, float maxValue)
		{
			return minValue + (maxValue - minValue) * random.GetNext();
		}
	}
}