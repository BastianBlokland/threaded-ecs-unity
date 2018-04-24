using EntityID = System.UInt16;

namespace ECS.Storage
{
	/// <summary>
	/// Class that can be used to hold a 'set' of entities
	/// Why not just use a List<EntityID>? 
	/// - We know the max amount of entities allready so we can pre-allocate everything (yes yes you can do so with a list as well)
	/// - We need very fast clear (because its used in the quering) List<T>.Clear actually travels through each element in the
	/// list to clear the entries (https://referencesource.microsoft.com/#mscorlib/system/collections/generic/list.cs)
	/// - Without c#7 (ref-returns) we need to expose the actually array to be able to modify elements directly
	/// </summary>
	public class EntitySet
	{
		public readonly EntityID[] Data;
		public int Count;

		public EntitySet()
		{
			Data = new EntityID[EntityID.MaxValue];
		}

		public void Add(EntityID entity)
		{
			Data[Count] = entity;
			Count++;
		}

		public void Clear()
		{
			Count = 0;
		}
	}
}