using UnityEngine;

using static System.Math;
using static Utils.MathUtils;

namespace Utils
{
    public struct Line
    {
        public float SqrMagnitude => (Destination - Origin).sqrMagnitude;

        public readonly Vector3 Origin;
        public readonly Vector3 Destination;

        public Line(Vector3 origin, Vector3 destination)
        {
            Origin = origin;
            Destination = destination;
        }

        public Ray GetRay()
            => new Ray(Origin, FastNormalize(Destination - Origin));

        public AABox GetBounds()
            => new AABox
            (
                min: new Vector3(Min(Origin.x, Destination.x), Min(Origin.y, Destination.y), Min(Origin.z, Destination.z)),
                max: new Vector3(Max(Origin.x, Destination.x), Max(Origin.y, Destination.y), Max(Origin.z, Destination.z))
            );
    }
}
