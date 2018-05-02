using System;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;

namespace ECS.Storage
{
    public sealed class EntityAllocator
    {
		private readonly bool[] entityFreeStatus;
		private readonly Stack<EntityID> freeEntities;

		public EntityAllocator()
		{
			entityFreeStatus = new bool[EntityID.MaxValue];
			freeEntities = new Stack<EntityID>();

			//Start with all possible entity-id's being 'free'
			for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
			{
				entityFreeStatus[entity] = true;
				freeEntities.Push(entity);
			}
		}

		public EntityID Allocate()
		{
			EntityID? result = null;
			
			lock(freeEntities)
			{
				if(freeEntities.Count > 0)
				{
					result = freeEntities.Pop();
					entityFreeStatus[result.Value] = false;
				}
			}

			if(result == null)
				throw new Exception("[StackEntityAllocator] No free entities left to allocate!");
			return result.Value;
		}

		public bool IsAllocated(EntityID entity)
		{
			return !entityFreeStatus[entity];
		}
	
		public bool IsFree(EntityID entity)
		{
			return entityFreeStatus[entity];
		}

		public void Free(EntityID entity)
		{
			lock(freeEntities)
			{
				if(!entityFreeStatus[entity])
				{
					entityFreeStatus[entity] = true;
					freeEntities.Push(entity);
				}
			}
		}
    }
}