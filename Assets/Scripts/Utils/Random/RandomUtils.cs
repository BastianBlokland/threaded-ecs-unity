using UnityEngine;

using static UnityEngine.Mathf;
using static Utils.MathUtils;

namespace Utils.Random
{
	public static class RandomUtils
	{
		public static Vector3 Inside(this IRandomProvider random, AABox box)
			=> new Vector3
			(
				x: box.Min.x + box.Size.x * random.GetNext(),
				y: box.Min.y + box.Size.y * random.GetNext(),
				z: box.Min.z + box.Size.z * random.GetNext()
			);

		public static Vector2 Direction2D(this IRandomProvider random)
		{
			var dir = new Vector2
			(
				x: random.GetNext() - .5f, 
				y: random.GetNext() - .5f
			);
			if(dir == Vector2.zero) //Should be very rare
				return Vector2.up;
			return FastNormalize(dir);
		}

		public static Vector3 Direction3D(this IRandomProvider random)
		{
			var dir = new Vector3
			(
				x: random.GetNext() - .5f, 
				y: random.GetNext() - .5f,
				z: random.GetNext() - .5f
			);
			if(dir == Vector3.zero) //Should be very rare
				return Vector3.forward;
			return FastNormalize(dir);
		}

		public static int Between(this IRandomProvider random, int minValue, int maxValue)
			=> FloorToInt(random.Between((float)minValue, (float)maxValue));

		public static float Between(this IRandomProvider random, float minValue, float maxValue)
			=> minValue + (maxValue - minValue) * random.GetNext();
	}
}