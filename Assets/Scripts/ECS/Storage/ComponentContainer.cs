using System;

using EntityID = System.UInt16;

namespace ECS.Storage
{
	public class ComponentContainer<T> : IComponentContainer<T>
		where T : struct, IComponent
	{
		public readonly T[] Data;

		public ComponentContainer()
		{
			Data = new T[EntityID.MaxValue];
		}

		public T Get(EntityID entity)
		{
			return Data[entity];
		}

		public void Set(EntityID entity, T dataEntry)
		{
			Data[entity] = dataEntry;
		}
	}

	public static class ComponentContainerUtils
	{
		public static IComponentContainer Create(Type compType)
		{
			Type containerType = typeof(ComponentContainer<>).MakeGenericType(compType);
			return Activator.CreateInstance(containerType) as IComponentContainer;
		}
	}
}