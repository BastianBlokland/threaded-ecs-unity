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
		
		//To make access to the component-masks thread-safe there are to layers of locking, if you want to read from the ENTIRE
		//array (as done by the 'GetEntities' that queries ALL the entries) then it locks the entire array into reading mode. That's 
		//also what the 'entityReaders' readers count is used for so that only the first thread takes the 'read' lock and all other threads
		//are then also allowed to read. And any thread that does modification to the array first takes a lock to put the array into 'write'
		//mode and then all other threads are also allowed to write. For safety for individual writes it has lock objects per entity entry
		//so that you can 'lock' access to a single entry in the array
		private readonly ComponentMask[] entities;
		private readonly object[] entityLocks;
		private int entityReaders; //How many threads are currently reading from 'entities'
		private int entityWriters; //How many threads are currently writing to 'entities'
		
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

			//Locks the 'entities' array if its the first 'writer' and releases when it was the last
			if(Interlocked.Increment(ref entityWriters) == 1) Monitor.Enter(entities);
			{
				//Lock this particular entity for modification
				lock(entityLocks[entity])
				{
					entities[entity].Clear();
				}
			}
			if(Interlocked.Decrement(ref entityWriters) == 0) Monitor.Exit(entities);
		}

		public void GetEntities(ComponentMask requiredComps, ComponentMask illegalComps, IList<EntityID> outputList)
		{
			outputList.Clear();

			//Locks the 'entities' array if its the first 'reader' and releases when it was the last
			if(Interlocked.Increment(ref entityReaders) == 1) Monitor.Enter(entities);
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					if(entities[entity].Has(requiredComps) && entities[entity].NotHas(illegalComps))
						outputList.Add(entity);
				}
			}
			if(Interlocked.Decrement(ref entityReaders) == 0) Monitor.Exit(entities);
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
			//Lock this entity so that it won't be modified while we are reading it
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

			//Note: locks the entities array for reading IF its the first one to start writing
			if(Interlocked.Increment(ref entityWriters) == 1) Monitor.Enter(entities);
			{
				//Locks this particular entity for modification.
				lock(entityLocks[entity])
				{
					entities[entity].Add(compMask);
				}
			}
			if(Interlocked.Decrement(ref entityWriters) == 0) Monitor.Exit(entities);
		}

		public void RemoveComponent<T>(EntityID entity)
			where T : struct, IComponent
		{
			ComponentMask compMask = GetMask<T>();

			//Note: locks the entities array for reading IF its the first one to start writing
			if(Interlocked.Increment(ref entityWriters) == 1) Monitor.Enter(entities);
			{
				//Locks this particular entity for modification.
				lock(entityLocks[entity])
				{
					entities[entity].Remove(compMask);
				}
			}
			if(Interlocked.Decrement(ref entityWriters) == 0) Monitor.Exit(entities);
		}

		public void Clear()
		{
			//Allways lock the entire entities array, btw 'lock' turns into Monitor.Enter and Monitor.Exit so 
			//its safe to mix lock on 'entities' and the 'Monitor.Enter' calls that the individual modifiction calls use
			//to lock the entities array into write mode.
			lock(entities)
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
					entities[entity].Clear();
			}
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