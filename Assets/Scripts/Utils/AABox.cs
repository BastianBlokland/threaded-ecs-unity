using UnityEngine;

using static UnityEngine.Mathf;
using static Utils.MathUtils;

namespace Utils
{
	public struct AABox
	{
		public Vector3 Size => new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
		public Vector3 Center => Min + Size * .5f;

		public Vector3 Min;
		public Vector3 Max;

		public AABox(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;
		}

		public static AABox FromCenterAndExtents(Vector3 origin, Vector3 size)
		{
			Vector3 halfSize = size * .5f;
			return new AABox
			(
				min: origin - halfSize,
				max: origin + halfSize
			);
		}

		public static bool Contains(AABox box, Vector3 point)
			=>	point.x > box.Min.x && point.x < box.Max.x && 
				point.y > box.Min.y && point.y < box.Max.y && 
				point.z > box.Min.z && point.z < box.Max.z;

		public static bool Intersect(AABox a, AABox b)
			=> 	a.Min.x < b.Max.x && a.Max.x > b.Min.x &&
				a.Min.y < b.Max.y && a.Max.y > b.Min.y &&
				a.Min.z < b.Max.z && a.Max.z > b.Min.z;

		//Implementation based on: https://gamedev.stackexchange.com/questions/18436/most-efficient-aabb-vs-ray-collision-algorithms
		public static bool Intersect(AABox box, Ray ray, out float time)
		{
			float t1 = (box.Min.x - ray.Origin.x) * ray.InvDirection.x; //Same as dividing by the direction
			float t2 = (box.Max.x - ray.Origin.x) * ray.InvDirection.x;
			float t3 = (box.Min.y - ray.Origin.y) * ray.InvDirection.y;
			float t4 = (box.Max.y - ray.Origin.y) * ray.InvDirection.y;
			float t5 = (box.Min.z - ray.Origin.z) * ray.InvDirection.z;
			float t6 = (box.Max.z - ray.Origin.z) * ray.InvDirection.z;

			float tMin = Max(Min(t1, t2), Min(t3, t4), Min(t5, t6));
			float tMax = Min(Max(t1, t2), Max(t3, t4), Max(t5, t6));
			
			// ray is intersecting AABB, but the whole AABB is behind us
			if(tMax < 0)
			{
				time = tMax;
				return false;
			}

			// ray doesn't intersect AABB
			if (tMin > tMax)
			{
				time = tMax;
				return false;
			}

			time = tMin;
			return true;
		}

		//Note: Output set needs to have a length that has a cube-root (like 8, so we can fit 2 cubes per axis (2 * 2 * 2 = 8))
		public static void Subdivide(AABox box, AABox[] output)
		{
			int axisCount = PerfectCubeRoot(output.Length);

			Vector3 sizePerSub = new Vector3
			(
				x: box.Size.x / axisCount, 
				y: box.Size.y / axisCount,
				z: box.Size.z / axisCount
			);

			for (int x = 0; x < axisCount; x++)
			for (int y = 0; y < axisCount; y++)
			for (int z = 0; z < axisCount; z++)
			{
				//How far this sub-box is from the 'min' point of the full box
				Vector3 subBoxOffset = new Vector3(sizePerSub.x * x, sizePerSub.y * y, sizePerSub.z * z);

				AABox subBox = new AABox(box.Min + subBoxOffset, box.Min + subBoxOffset + sizePerSub);
				int index = x * axisCount * axisCount + y * axisCount + z;
				output[index] = subBox;
			}
		}
	}
}