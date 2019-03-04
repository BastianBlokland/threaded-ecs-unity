using System;

using EntityID = System.UInt16;

namespace ECS.Storage
{
    /// <summary>
    /// Responsible for storing component data.
    ///
    /// Thread-safety: Contains no locking but due to the nature of the storage its completely safe to
    /// read or write to different entities.
    /// </summary>
    public sealed class ComponentContainer<T> : IComponentContainer<T>
        where T : struct, IComponent
    {
        public T[] Data { get; } = new T[EntityID.MaxValue];

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
