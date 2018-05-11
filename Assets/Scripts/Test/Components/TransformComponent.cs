using ECS.Storage;
using UnityEngine;
using Utils;

using static UnityEngine.Vector3;
using static Utils.MathUtils;

namespace Test.Components
{
    public struct TransformComponent : IComponent
    {
        public Float3x4 Matrix;

		public TransformComponent(Vector3 position)
		{
			Matrix = Float3x4.FromPosition(position);
		}

        public TransformComponent(Vector3 position, Vector3 forward) : this(position, forward, Vector3.up) { }

        public TransformComponent(Vector3 position, Vector3 forward, Vector3 upReference)
        {
            Vector3 right = FastNormalize(Cross(upReference, forward));
            Vector3 up = FastNormalize(Cross(forward, right));
            Matrix = Float3x4.FromPositionAndAxis(position, right, up, forward);
        }
    }
}