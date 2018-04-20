using EntityID = System.UInt16;

namespace ECS
{
	public interface IComponentContainer 
	{

	}

	public interface IComponentContainer<T> : IComponentContainer
		where T : struct, IComponent
	{
		T Get(EntityID entity);
		void Set(EntityID entity, T dataEntry);
	}
}