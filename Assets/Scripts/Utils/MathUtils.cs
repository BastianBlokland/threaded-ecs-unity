using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils
{
	public static class MathUtils
	{
		public static bool DoesRangeOverlap(float aStart, float aEnd, float bStart, float bEnd)
		{
			return aEnd >= bStart && aStart <= bEnd;
		}
	}
}