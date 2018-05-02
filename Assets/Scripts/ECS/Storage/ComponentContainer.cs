using System;

using EntityID = System.UInt16;

namespace ECS.Storage
{
	public sealed class ComponentContainer<T> : IComponentContainer<T>
		where T : struct, IDataComponent
	{
		public T[] Data { get { return data; } }

		private readonly T[] data;

		public ComponentContainer()
		{
			data = new T[EntityID.MaxValue];
		}

		public T Get(EntityID entity)
		{
			return data[entity];
		}

		public void Set(EntityID entity, T dataEntry)
		{
			data[entity] = dataEntry;
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