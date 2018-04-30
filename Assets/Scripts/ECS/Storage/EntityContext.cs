using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;
using CompID = System.Byte;

namespace ECS.Storage
{
	/// <summary>
	/// Responsible for creating and removing of entities and adding and removing of components on those.
	/// 
	/// Thread-safety: This class is thread-safe IF you don't touch the same components on the same entity from multiple threads. So its
	/// important to manually keep track of not running tasks in parallel that touch the same components on the same entities. Adding and removing
	/// entities is completely safe and also adding and removing of components is completely safe.
	/// </summary>
    public class EntityContext
    {
		private readonly ComponentReflector reflector;
		private readonly EntityAllocator entityAllocator;
        
		//Storage for the actual component data
		private readonly IComponentContainer[] containers;
		
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
		
		public EntityContext()
			: this(componentAssembly: typeof(EntityContext).Assembly)
		{}

		public EntityContext(Assembly componentAssembly)
		{
			reflector = new ComponentReflector(componentAssembly);
			entityAllocator = new EntityAllocator();
			containers = new IComponentContainer[reflector.ComponentCount];
			entities = new ComponentMask[EntityID.MaxValue];
			entityLocks = new object[EntityID.MaxValue];
			entityQueryLock = new ReaderWriterLockSlim();

			//Allocate a container for each component type
			for (CompID comp = 0; comp < reflector.ComponentCount; comp++)
			{
				Type compType = reflector.GetType(comp);
				containers[comp] = ComponentContainerUtils.Create(compType);
			}

			//Create a component-mask and a lock object for each entity
			for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
			{
				entities[entity] = new ComponentMask();
				entityLocks[entity] = new object();
			}
		}

		public EntityID CreateEntity()
		{
			return entityAllocator.Allocate();
		}

		public bool HasEntity(EntityID entity)
		{
			return entityAllocator.IsAllocated(entity);
		}

		public void RemoveEntity(EntityID entity)
		{
			entityAllocator.Free(entity);

			entityQueryLock.EnterReadLock();
			{
				//Lock this particular entity for modification
				lock(entityLocks[entity])
				{
					entities[entity].Clear();
				}
			}
			entityQueryLock.ExitReadLock();
		}

		public void GetEntities(ComponentMask requiredComps, ComponentMask illegalComps, EntitySet outputSet)
		{
			outputSet.Clear();

			//I know this looks like its reversed but i wanted to avoid locking each individual entity in the
			//for loop as that adds a significant cost, so thats why its using the 'write' portion of a 'ReaderWriterLockSlim'
			//Its not using a entire 'lock' on  the 'entities' array as we still want to have multiple concurrent writers (as they
			//still individually lock each entry they modify)
			entityQueryLock.EnterWriteLock();
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					if(entities[entity].Has(requiredComps) && entities[entity].NotHas(illegalComps))
						outputSet.Add(entity);
				}
			}
			entityQueryLock.ExitWriteLock();
		}

		public int GetEntityCount(ComponentMask requiredComps, ComponentMask illegalComps)
		{
			int count = 0;
			//For info about this reverse looking lock: see comment on 'GetEntities' method
			entityQueryLock.EnterWriteLock();
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					if(entities[entity].Has(requiredComps) && entities[entity].NotHas(illegalComps))
						count++;
				}
			}
			entityQueryLock.ExitWriteLock();
			return count;
		}

		public bool HasComponent<T>(EntityID entity)
			where T : struct, IComponent
		{
			CompID comp = GetID<T>();
			return HasComponent(entity, comp);
		}

		public bool HasComponent(EntityID entity, CompID comp)
		{
			return HasComponents(entity, ComponentMask.CreateMask(comp));
		}

		public bool HasComponents(EntityID entity, ComponentMask mask)
		{
			bool has;
			lock(entityLocks[entity])
			{
				has = entities[entity].Has(mask);
			}
			return has;
		}

		public T GetComponent<T>(EntityID entity)
			where T : struct, IComponent
		{
			CompID comp = GetID<T>();
			return ((IComponentContainer<T>)containers[comp]).Get(entity);
		}

		public void SetComponent<T>(EntityID entity, T data)
			where T : struct, IComponent
		{
			CompID comp = GetID<T>();
			ComponentMask compMask = ComponentMask.CreateMask(comp);
			((IComponentContainer<T>)containers[comp]).Set(entity, data);

			entityQueryLock.EnterReadLock();
			{
				//Locks this particular entity for modification.
				lock(entityLocks[entity])
				{
					entities[entity].Add(compMask);
				}
			}
			entityQueryLock.ExitReadLock();
		}

		public void RemoveComponent<T>(EntityID entity)
			where T : struct, IComponent
		{
			ComponentMask compMask = GetMask<T>();

			entityQueryLock.EnterReadLock();
			{
				//Locks this particular entity for modification.
				lock(entityLocks[entity])
				{
					entities[entity].Remove(compMask);
				}
			}
			entityQueryLock.ExitReadLock();
		}

		public void Clear()
		{
			entityQueryLock.EnterReadLock();
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					lock(entityLocks[entity])
					{
						entities[entity].Clear();					
					}
				}
			}
			entityQueryLock.ExitReadLock();
		}

		public IComponentContainer<T> GetContainer<T>()
			where T : struct, IComponent
		{
			CompID comp = GetID<T>();
			return ((IComponentContainer<T>)containers[comp]);
		}

		public CompID GetID<T>()
			where T : struct, IComponent
		{
			return reflector.GetID<T>();
		}

		public ComponentMask GetMask<T1>()
			where T1 : struct, IComponent
		{
			return ComponentMask.CreateMask(GetID<T1>());
		}

		public ComponentMask GetMask<T1, T2>()
			where T1 : struct, IComponent
			where T2 : struct, IComponent
		{
			return ComponentMask.CreateMask(GetID<T1>(), GetID<T2>());
		}
    }
}