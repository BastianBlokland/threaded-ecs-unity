using System;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;

namespace ECS.Storage
{
    public class EntityAllocator
    {
		private readonly ReaderWriterLockSlim readWriteLock;
		private readonly bool[] entityFreeStatus;
		private readonly Stack<EntityID> freeEntities;

		public EntityAllocator()
		{
			readWriteLock = new ReaderWriterLockSlim();
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
			readWriteLock.EnterWriteLock();
			{
				if(freeEntities.Count > 0)
				{
					result = freeEntities.Pop();
					entityFreeStatus[result.Value] = false;
				}
			}
			readWriteLock.ExitWriteLock();

			if(result == null)
				throw new Exception("[StackEntityAllocator] No free entities left to allocate!");
			return result.Value;
		}

		public bool IsAllocated(EntityID entity)
		{
			return !IsFree(entity);
		}
	
		public bool IsFree(EntityID entity)
		{
			bool result;
			readWriteLock.EnterReadLock();
			{
				result = entityFreeStatus[entity];
			}
			readWriteLock.ExitReadLock();
			return result;
		}

		public void Free(EntityID entity)
		{
			readWriteLock.EnterWriteLock();
			{
				if(!entityFreeStatus[entity])
				{
					entityFreeStatus[entity] = true;
					freeEntities.Push(entity);
				}
			}
			readWriteLock.ExitWriteLock();
		}
    }
}