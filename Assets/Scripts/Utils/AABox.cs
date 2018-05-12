using UnityEngine;

namespace Utils
{
	public struct AABox
	{
		public Vector3 Size => new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);

		public Vector3 Min;
		public Vector3 Max;

		public AABox(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;
		}
	}
}