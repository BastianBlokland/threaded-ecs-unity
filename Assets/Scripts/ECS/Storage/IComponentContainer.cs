using EntityID = System.UInt16;

namespace ECS.Storage
{
	public interface IComponentContainer<T> : IComponentContainer
		where T : struct, IComponent
	{
		T[] Data { get; }

		T Get(EntityID entity);
		void Set(EntityID entity, T dataEntry);
	}

	public interface IComponentContainer { }
}