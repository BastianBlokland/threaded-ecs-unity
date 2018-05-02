using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;
using CompID = System.Byte;

namespace ECS.Storage
{
	public sealed class EntityTagContainer : IDisposable
    {	
		//Locking on the entities array is done with per element locks that are being held in the 'entityLocks' array, this
		//makes it possible for multiple concurrent writers to different elements. There one additional method of locking for the
		//'GetEntities' method because individually locking each element was adding to much time to the query so its using a 
		//'ReaderWriterLockSlim' lock in reverse in the sense that its takes a 'writer' lock when querying and all the places
		//that do modifictions take a 'read' lock. This we we can still have concurrent modifications (to different elements) as 
		//the 'ReaderWriterLockSlim' allows for multiple simultaneous readers but will stop all modifications we we are quering
		//one done side is that this will NOT allow for concurrent 'GetEntities' queries even tho that would be save, but i suspect
		//that will be a very rare occurrence. In the mean time if anyone comes up with a smarter way to do this locking feel free :)
		private readonly ComponentMask[] entities;
		private readonly object[] entityLocks;
		private readonly ReaderWriterLockSlim entityQueryLock;
		
		public EntityTagContainer()
		{
			entities = new ComponentMask[EntityID.MaxValue];
			entityLocks = new object[EntityID.MaxValue];
			entityQueryLock = new ReaderWriterLockSlim();

			//Create a component-mask and a lock object for each entity
			for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
			{
				entities[entity] = new ComponentMask();
				entityLocks[entity] = new object();
			}
		}

		public bool HasTags(EntityID entity, ComponentMask mask)
		{
			bool has;
			lock(entityLocks[entity])
			{
				has = entities[entity].Has(mask);
			}
			return has;
		}

		public void SetTags(EntityID entity, ComponentMask mask)
		{
			entityQueryLock.EnterReadLock();
			{
				//Locks this particular entity for modification.
				lock(entityLocks[entity])
				{
					entities[entity].Add(mask);
				}
			}
			entityQueryLock.ExitReadLock();
		}

		public void RemoveTags(EntityID entity, ComponentMask mask)
		{
			entityQueryLock.EnterReadLock();
			{
				//Locks this particular entity for modification.
				lock(entityLocks[entity])
				{
					entities[entity].Remove(mask);
				}
			}
			entityQueryLock.ExitReadLock();
		}

		public void GetEntities(ComponentMask requiredTags, ComponentMask illegalTags, EntitySet outputSet)
		{
			outputSet.Clear();
			bool noIllegalComps = illegalTags.IsEmpty;

			//I know this looks like its reversed but i wanted to avoid locking each individual entity in the
			//for loop as that adds a significant cost, so thats why its using the 'write' portion of a 'ReaderWriterLockSlim'
			//Its not using a entire 'lock' on  the 'entities' array as we still want to have multiple concurrent writers (as they
			//still individually lock each entry they modify)
			entityQueryLock.EnterWriteLock();
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					if(entities[entity].Has(requiredTags) && (noIllegalComps || entities[entity].NotHas(illegalTags)))
						outputSet.Add(entity);
				}
			}
			entityQueryLock.ExitWriteLock();
		}

		public int GetEntityCount(ComponentMask requiredTags, ComponentMask illegalTags)
		{
			int count = 0;
			bool noIllegalComps = illegalTags.IsEmpty;

			//For info about this reverse looking lock: see comment on 'GetEntities' method
			entityQueryLock.EnterWriteLock();
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					if(entities[entity].Has(requiredTags) && (noIllegalComps || entities[entity].NotHas(illegalTags)))
						count++;
				}
			}
			entityQueryLock.ExitWriteLock();
			return count;
		}

		public void Dispose()
		{
			entityQueryLock.Dispose();
		}
    }
}