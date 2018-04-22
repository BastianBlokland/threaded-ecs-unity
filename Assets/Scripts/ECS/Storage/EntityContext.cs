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
	/// Thread-safety: This class is thread-safe IF you don't touch the same 'entity' from multiple threads.
	/// So its perfectly safe if thread 1 is adding / removing components from entityA and thread 2 is 
	/// from entityB, but thread1 and thread2 cannot both be adjusting entityA. Multiple threads are allowed to 
	/// create entities but 2 threads cannot remove the same entity at the same time, but different threads removing
	/// different entities at the same time is no problem. Also querying for entities is allways safe, but its using
	/// locks to stop the writing during the 'quering'.
	/// </summary>
    public class EntityContext
    {
		private readonly ComponentReflector reflector;
		private readonly EntityAllocator entityAllocator;
        
		//Storage for the actual component data
		private readonly IComponentContainer[] containers;
		
		//Storage for lookup of what components are on what entities
		//Locking on this array is done in a way that allows for safe 'reading' with 'GetEntities' (stops writing
		//while using the 'GetEntities') And allows for multiple threads to write to SEPARATE entries in the array
		//this way we can can go wide easily without requiring locking but you have to manually make sure that you 
		//are not writing to the same entry from multiple threads.
		private readonly ComponentMask[] entities;
		private int entitiesReaders; //How many threads are currently reading from 'entities'
		private int entitiesWriters; //How many threads are currently writing to 'entities'
		
		public EntityContext()
			: this(componentAssembly: typeof(EntityContext).Assembly)
		{}

		public EntityContext(Assembly componentAssembly)
		{
			reflector = new ComponentReflector(componentAssembly);
			entityAllocator = new EntityAllocator();
			containers = new IComponentContainer[reflector.ComponentCount];
			entities = new ComponentMask[EntityID.MaxValue];

			//Allocate a container for each component type
			for (CompID comp = 0; comp < reflector.ComponentCount; comp++)
			{
				Type compType = reflector.GetType(comp);
				containers[comp] = ComponentContainerUtils.Create(compType);
			}

			//Create a component-mask for each entity
			for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				entities[entity] = new ComponentMask();
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
			if(Interlocked.Increment(ref entitiesWriters) == 1) Monitor.Enter(entities);
			{
				entities[entity].Clear();	
			}
			if(Interlocked.Decrement(ref entitiesWriters) == 0) Monitor.Exit(entities);
		}

		public void GetEntities(ComponentMask requiredComps, ComponentMask illegalComps, IList<EntityID> outputList)
		{
			outputList.Clear();

			//Locks the 'entities' array if its the first 'reader' and releases when it was the last
			if(Interlocked.Increment(ref entitiesReaders) == 1) Monitor.Enter(entities);
			{
				for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
				{
					if(entities[entity].Has(requiredComps) && entities[entity].NotHas(illegalComps))
						outputList.Add(entity);
				}
			}
			if(Interlocked.Decrement(ref entitiesReaders) == 0) Monitor.Exit(entities);
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

			//Locks the 'entities' array if its the first 'reader' and releases when it was the last
			//TODO: Need to investigate if we need locking here, its safer but it might cause unnecessary stalls
			//as its actually perfectly safe to writer to entity A and check for components on entity B.
			if(Interlocked.Increment(ref entitiesReaders) == 1) Monitor.Enter(entities);
			{
				has = entities[entity].Has(mask);
			}
			if(Interlocked.Decrement(ref entitiesReaders) == 0) Monitor.Exit(entities);

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
			//BUT: there is no safety against trying to write to the same entity from multiple threads
			if(Interlocked.Increment(ref entitiesWriters) == 1) Monitor.Enter(entities);
			{
				entities[entity].Add(compMask);
			}
			if(Interlocked.Decrement(ref entitiesWriters) == 0) Monitor.Exit(entities);
		}

		public void RemoveComponent<T>(EntityID entity)
			where T : struct, IComponent
		{
			ComponentMask compMask = GetMask<T>();

			//Note: locks the entities array for reading IF its the first one to start writing
			//BUT: there is no safety against trying to write to the same entity from multiple threads
			if(Interlocked.Increment(ref entitiesWriters) == 1) Monitor.Enter(entities);
			{
				entities[entity].Remove(compMask);
			}
			if(Interlocked.Decrement(ref entitiesWriters) == 0) Monitor.Exit(entities);
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