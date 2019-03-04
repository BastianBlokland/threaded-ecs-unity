using System;
using System.Collections.Generic;

using EntityID = System.UInt16;

namespace ECS.Storage
{
    /// <summary>
    /// Responsible for allocating and free-ing entities, uses a stack strategy to keep the used entity-ids close to eachother.
    ///
    /// Thread-safety: All public methods on this class are thread-safe (uses locking for allocating and free-ing) and the quering
    /// is safe because boolean reads are atomic
    /// </summary>
    public sealed class EntityAllocator
    {
        private readonly bool[] entityFreeStatus = new bool[EntityID.MaxValue];
        private readonly Stack<EntityID> freeEntities = new Stack<EntityID>();
        private readonly object lockObject = new object();

        public EntityAllocator()
        {
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
            lock(lockObject)
            {
                if(freeEntities.Count > 0)
                {
                    result = freeEntities.Pop();
                    entityFreeStatus[result.Value] = false;
                }
            }
            if(result == null)
                throw new Exception($"[{nameof(EntityAllocator)}] No free entities left to allocate!");
            return result.Value;
        }

        public bool IsAllocated(EntityID entity) => !entityFreeStatus[entity];

        public bool IsFree(EntityID entity) => entityFreeStatus[entity];

        public void Free(EntityID entity)
        {
            lock(lockObject)
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
