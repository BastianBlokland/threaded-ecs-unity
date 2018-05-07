using ECS.Storage;
using UnityEngine;
using Utils;

namespace Test.Components
{
    public struct TransformComponent : IComponent
    {
        public Float3x4 Matrix;

		public TransformComponent(Vector3 position)
		{
			Matrix = Float3x4.FromPosition(position);
		}
    }
}