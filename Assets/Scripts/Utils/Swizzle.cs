using UnityEngine;

namespace Utils
{
    public static class Swizzle
    {
        public static Vector3 X0Y(this Vector2 vector)
        {
            return new Vector3(vector.x, 0f, vector.y);
        }
    }
}
