using System;
using System.Threading;
using Utils;

using EntityID = System.UInt16;

namespace ECS.Storage
{
	/// <summary>
	/// Can be used for setting/unsetting tags on entities and quering for entities with certain tags
	/// 
	/// Thread-Safety: All public methods on this class are thread-safe. Locking is done by two mechanisms: There is a
	/// total 'ModeLock' where you can specify wether you want to read or write to the 'entities' collection. (allows for concurrent
	/// readers or concurrent writers but not both at the same time), on top of that writing is locked by individual locks per entity.
	/// This is done this way so you can safely read from the entire 'entities' array while its in 'read' mode without having to individually 
	/// lock entities. And also allows concurrent writes to different entities.
	/// </summary>
	public sealed class EntityTagContainer
    {
		private const int READ_LOCK_MODE = 1;
		private const int WRITE_LOCK_MODE = 2;

		private readonly TagMask[] entities;
		private readonly object[] entityWriteLocks;
		private readonly ModeLock entitiesLock;
		
		public EntityTagContainer()
		{
			entities = new TagMask[EntityID.MaxValue];
			entityWriteLocks = new object[EntityID.MaxValue];
			entitiesLock = new ModeLock();

			//Create a tag-mask and a lock object for each entity
			for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
			{
				entities[entity] = new TagMask();
				entityWriteLocks[entity] = new object();
			}
		}

		public bool HasTags(EntityID entity, TagMask mask)
		{
			bool has;
			entitiesLock.Enter(READ_LOCK_MODE);
			{
				has = entities[entity].Has(mask);
			}
			entitiesLock.Exit();
			return has;
		}

		public void SetTags(EntityID entity, TagMask mask)
		{
			entitiesLock.Enter(WRITE_LOCK_MODE);
			{
				//Locks this particular entity for modification.
				lock(entityWriteLocks[entity])
				{
					entities[entity].Add(mask);
				}
			}
			entitiesLock.Exit();
		}

		public void RemoveTags(EntityID entity, TagMask mask)
		{
			entitiesLock.Enter(WRITE_LOCK_MODE);
			{
				//Locks this particular entity for modification.
				lock(entityWriteLocks[entity])
				{
					entities[entity].Remove(mask);
				}
			}
			entitiesLock.Exit();
		}

		public void Query(TagMask requiredTags, TagMask illegalTags, EntitySet outputSet)
		{
			outputSet.Clear();
			bool noIllegalComps = illegalTags.IsEmpty;

			entitiesLock.Enter(READ_LOCK_MODE);
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					if(entities[entity].Has(requiredTags) && (noIllegalComps || entities[entity].NotHas(illegalTags)))
						outputSet.Add(entity);
				}
			}
			entitiesLock.Exit();
		}

		public int Query(TagMask requiredTags, TagMask illegalTags)
		{
			int count = 0;
			bool noIllegalComps = illegalTags.IsEmpty;

			entitiesLock.Enter(READ_LOCK_MODE);
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					if(entities[entity].Has(requiredTags) && (noIllegalComps || entities[entity].NotHas(illegalTags)))
						count++;
				}
			}
			entitiesLock.Exit();
			return count;
		}
    }
}