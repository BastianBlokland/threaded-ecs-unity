using ECS.Storage;
using UnityEngine;

using EntityID = System.UInt16;

namespace Test
{
	public class TestController : MonoBehaviour
	{
		protected void Awake()
		{
			EntityContext context = new EntityContext();

			EntityID entity = context.CreateEntity();
			context.SetComponent(entity, new TestComponent { Test = false, Test2 = 1337f });

			if(context.HasComponent<TestComponent>(entity))
				Debug.Log("Found the component");
			else
				Debug.Log("Component is not there?");

			TestComponent c = context.GetComponent<TestComponent>(entity);
			Debug.Log("Got value: " + c.Test2);
		}
	}
}