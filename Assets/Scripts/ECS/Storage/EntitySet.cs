using System.Collections;
using System.Collections.Generic;

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
	/// 
	/// Thread-safety: NOT thread-safe
	/// </summary>
	public sealed class EntitySet : IReadOnlyList<EntityID>
	{
		public EntityID[] Data { get; } = new EntityID[EntityID.MaxValue];
		public int Count { get; set; }

		public EntityID this[int index]
		{
			get { return Data[index]; }
			set { Data[index] = value; }
		}
		
		public void Add(EntityID entity)
		{
			Data[Count] = entity;
			Count++;
		}

		public void Clear() => Count = 0;

		//Part of 'IReadOnlyList' but at the moment have no support for a enumerator
		IEnumerator<ushort> IEnumerable<ushort>.GetEnumerator() { throw new System.NotImplementedException(); }
		IEnumerator IEnumerable.GetEnumerator() { throw new System.NotImplementedException(); }
	}
}