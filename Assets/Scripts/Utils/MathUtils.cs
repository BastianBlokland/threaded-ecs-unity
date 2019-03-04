using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils
{
    public static class MathUtils
    {
        [StructLayout(LayoutKind.Explicit, Size = 4)]
        private struct IntFloat
        {
            [FieldOffset(0)] public float FloatValue;
            [FieldOffset(0)] public int IntValue;
        }

        //Implementation based on: https://en.wikipedia.org/wiki/Fast_inverse_square_root
        public static float FastInvSqrRoot(float number)
        {
            var conv = new IntFloat { FloatValue = number };

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

        public static float ManhattanDistance(Vector3 vector)
            => Math.Abs(vector.x) + Math.Abs(vector.y) + Math.Abs(vector.z);

        public static bool DoesRangeOverlap(float aStart, float aEnd, float bStart, float bEnd)
            => aEnd >= bStart && aStart <= bEnd;

        public static bool Bigger(float val, float ref1, float ref2, float ref3)
            => val > ref1 && val > ref2 && val > ref3;

        public static float CubeRoot(float val)
            => Mathf.Pow(val, 1f / 3f);

        public static int PerfectCubeRoot(int val)
        {
            int cubeRoot = (int)CubeRoot(val);
            if ((cubeRoot * cubeRoot * cubeRoot) != val)
                throw new Exception($"[{nameof(MathUtils)}] '{val}' has no perfect cube-root");
            return cubeRoot;
        }
    }
}
