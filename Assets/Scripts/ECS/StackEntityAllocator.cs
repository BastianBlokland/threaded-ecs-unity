using System.Collections.Generic;
using EntityID = System.UInt16;

namespace ECS
{
    public class StackEntityAllocator : IEntityAllocator
    {
		private readonly Stack<EntityID> freeEntities;

		public StackEntityAllocator()
		{
			freeEntities = new Stack<EntityID>();

			//Start with all possible entity-id's being 'free'
			for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				freeEntities.Push(entity);
		}

		public EntityID Allocate()
		{
			return freeEntities.Pop();
		}

		/// <summary>
		/// NOTE: This has no guard against someone 'free-ing' a entity multiple times causing it to be 
		/// on the 'free-entities' stack multiple times, need to do some thinking if there is some performant
		/// way of checking. In the mean time be carefull :)
		/// </summary>
		public void Free(EntityID entity)
		{
			freeEntities.Push(entity);
		}
    }
}