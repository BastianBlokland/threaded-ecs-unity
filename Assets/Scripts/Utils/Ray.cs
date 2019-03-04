using UnityEngine;

namespace Utils
{
    public struct Ray
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;

        //Cache out the InverseDirection for quick dividing by the direction components
        public readonly Vector3 InvDirection;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
            InvDirection = new Vector3
            (
                x: 1f / Direction.x,
                y: 1f / Direction.y,
                z: 1f / Direction.z
            );
        }
    }
}
