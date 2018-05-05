using System;

using EntityID = System.UInt16;

namespace ECS.Storage
{
	public sealed class ComponentContainer<T> : IComponentContainer<T>
		where T : struct, IComponent
	{
		public T[] Data { get; }

		public ComponentContainer()
		{
			Data = new T[EntityID.MaxValue];
		}

		public T Get(EntityID entity)
		{
			return Data[entity];
		}

		public void Set(EntityID entity, T data)
		{
			Data[entity] = data;
		}
	}

	public static class ComponentContainerUtils
	{
		public static IComponentContainer Create(Type componentType)
		{
			var type = typeof(ComponentContainer<>).MakeGenericType(componentType);
			return Activator.CreateInstance(type) as IComponentContainer;
		}
	}
}