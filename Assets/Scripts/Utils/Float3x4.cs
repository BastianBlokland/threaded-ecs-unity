using UnityEngine;

using static UnityEngine.Vector3;
using static Utils.MathUtils;

namespace Utils
{
    /// <summary>
    /// Simple 3x4 matrices that maps directly to the cg float3x4 object on the gpu.
    /// Note: 3x4 because we don't need the projection column here
    /// </summary>
    public struct Float3x4
    {
        public const int SIZE = sizeof(float) * 3 * 4;

        public static Float3x4 Identity { get; }
            = new Float3x4
            {
                m00 = 1f,
                m01 = 0f,
                m02 = 0f,
                m10 = 0f,
                m11 = 1f,
                m12 = 0f,
                m20 = 0f,
                m21 = 0f,
                m22 = 1f,
                m30 = 0f,
                m31 = 0f,
                m32 = 0f
            };

        public Vector3 Position
        {
            get { return new Vector3(m30, m31, m32); }
            set { m30 = value.x; m31 = value.y; m32 = value.z; }
        }

        public float m00, m01, m02;
        public float m10, m11, m12;
        public float m20, m21, m22;
        public float m30, m31, m32;

        public static Float3x4 FromPosition(Vector3 position)
            => new Float3x4
            {
                m00 = 1f,
                m01 = 0f,
                m02 = 0f,
                m10 = 0f,
                m11 = 1f,
                m12 = 0f,
                m20 = 0f,
                m21 = 0f,
                m22 = 1f,
                m30 = position.x,
                m31 = position.y,
                m32 = position.z
            };

        public static Float3x4 FromPositionAndForward(Vector3 position, Vector3 forward) => FromPositionAndForward(position, forward, Vector3.up);

        public static Float3x4 FromPositionAndForward(Vector3 position, Vector3 forward, Vector3 upReference)
        {
            Vector3 right = FastNormalize(Cross(upReference, forward));
            Vector3 up = FastNormalize(Cross(forward, right));
            return FromPositionAndAxis(position, right, up, forward);
        }

        public static Float3x4 FromPositionAndAxis(Vector3 position, Vector3 right, Vector3 up, Vector3 forward)
            => new Float3x4
            {
                m00 = right.x,
                m01 = right.y,
                m02 = right.z,
                m10 = up.x,
                m11 = up.y,
                m12 = up.z,
                m20 = forward.x,
                m21 = forward.y,
                m22 = forward.z,
                m30 = position.x,
                m31 = position.y,
                m32 = position.z
            };
    }
}
