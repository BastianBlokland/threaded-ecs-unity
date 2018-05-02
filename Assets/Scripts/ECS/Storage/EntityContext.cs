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
		private readonly EntityTagContainer tagContainer;	
		private readonly IComponentContainer[] containers;
		
		public EntityContext()
			: this(componentAssembly: typeof(EntityContext).Assembly)
		{}

		public EntityContext(Assembly componentAssembly)
		{
			reflector = new ComponentReflector(componentAssembly);
			entityAllocator = new EntityAllocator();
			tagContainer = new EntityTagContainer();
			containers = new IComponentContainer[reflector.ComponentCount];
			
			//Allocate a container for each data component type
			for (CompID comp = 0; comp < reflector.ComponentCount; comp++)
			{
				Type compType = reflector.GetType(comp);
				if(reflector.IsDataComponent(compType))
					containers[comp] = ComponentContainerUtils.Create(compType);
				else
					containers[comp] = null;
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
			tagContainer.RemoveTags(entity, ComponentMask.Full);
		}

		public void GetEntities(ComponentMask requiredComps, ComponentMask illegalComps, EntitySet outputSet)
		{
			tagContainer.GetEntities(requiredComps, illegalComps, outputSet);
		}

		public int GetEntityCount(ComponentMask requiredComps, ComponentMask illegalComps)
		{
			return tagContainer.GetEntityCount(requiredComps, illegalComps);
		}

		public bool HasComponents(EntityID entity, ComponentMask mask)
		{
			return tagContainer.HasTags(entity, mask);
		}

		public T GetComponent<T>(EntityID entity)
			where T : struct, IDataComponent
		{
			CompID comp = GetID<T>();
			return ((IComponentContainer<T>)containers[comp]).Get(entity);
		}

		public void SetComponent<T>(EntityID entity)
			where T : struct, ITagComponent
		{
			ComponentMask compMask = GetMask<T>();
			tagContainer.SetTags(entity, compMask);
		}

		public void SetComponent<T>(EntityID entity, T data)
			where T : struct, IDataComponent
		{
			CompID comp = GetID<T>();
			((IComponentContainer<T>)containers[comp]).Set(entity, data);
			SetComponent<T>(entity);
		}

		public void RemoveComponent<T>(EntityID entity)
			where T : struct, ITagComponent
		{
			ComponentMask compMask = GetMask<T>();
			tagContainer.RemoveTags(entity, compMask);
		}

		public IComponentContainer<T> GetContainer<T>()
			where T : struct, IDataComponent
		{
			CompID comp = GetID<T>();
			return ((IComponentContainer<T>)containers[comp]);
		}

		public CompID GetID<T>()
			where T : struct, ITagComponent
		{
			return reflector.GetID<T>();
		}

		public ComponentMask GetMask<T1>()
			where T1 : struct, ITagComponent
		{
			return ComponentMask.CreateMask(GetID<T1>());
		}

		public ComponentMask GetMask<T1, T2>()
			where T1 : struct, ITagComponent
			where T2 : struct, ITagComponent
		{
			return ComponentMask.CreateMask(GetID<T1>(), GetID<T2>());
		}
    }
}