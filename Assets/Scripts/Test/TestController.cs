using ECS;
using UnityEngine;

using EntityID = System.UInt16;

namespace Test
{
	public class TestController : MonoBehaviour
	{
		protected void Awake()
		{
			IEntityAllocator allocator = new StackEntityAllocator();
			EntityContainer entityContainer = new EntityContainer(typeof(TestComponent).Assembly);

			EntityID entity = allocator.Allocate();
			entityContainer.SetComponent(entity, new TestComponent { Test = false, Test2 = 1337f });

			if(entityContainer.HasComponent<TestComponent>(entity))
				Debug.Log("Found the component");
			else
				Debug.Log("Component is not there?");

			TestComponent c = entityContainer.GetComponent<TestComponent>(entity);
			Debug.Log("Got value: " + c.Test2);
		}
	}
}