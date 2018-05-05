using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;
using TagID = System.Byte;

namespace ECS.Storage
{
	/// <summary>
	/// Responsible for creating and removing of entities and adding and removing of components on those.
	/// 
	/// Thread-safety: This class is thread-safe IF you don't touch the same components on the same entity from multiple threads. So its
	/// important to manually keep track of not running tasks in parallel that touch the same components on the same entities. Adding and removing
	/// entities is completely safe and also adding and removing of components is completely safe.
	/// </summary>
    public sealed class EntityContext : IDisposable
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
			for (TagID tag = 0; tag < reflector.ComponentCount; tag++)
			{
				Type compType = reflector.GetType(tag);
				if(reflector.IsComponent(compType))
					containers[tag] = ComponentContainerUtils.Create(compType);
				else
					containers[tag] = null;
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
			tagContainer.RemoveTags(entity, TagMask.Full);
		}

		public void GetEntities(TagMask requiredComps, TagMask illegalComps, EntitySet outputSet)
		{
			tagContainer.GetEntities(requiredComps, illegalComps, outputSet);
		}

		public int GetEntityCount(TagMask requiredComps, TagMask illegalComps)
		{
			return tagContainer.GetEntityCount(requiredComps, illegalComps);
		}

		public bool HasComponents(EntityID entity, TagMask mask)
		{
			return tagContainer.HasTags(entity, mask);
		}

		public T GetComponent<T>(EntityID entity)
			where T : struct, IComponent
		{
			TagID tag = GetID<T>();
			return ((IComponentContainer<T>)containers[tag]).Get(entity);
		}

		public void SetTag<T>(EntityID entity)
			where T : struct, ITag
		{
			TagMask tagMask = GetMask<T>();
			tagContainer.SetTags(entity, tagMask);
		}

		public void SetComponent<T>(EntityID entity, T data)
			where T : struct, IComponent
		{
			TagID tag = GetID<T>();
			((IComponentContainer<T>)containers[tag]).Set(entity, data);
			SetTag<T>(entity);
		}

		public void RemoveTag<T>(EntityID entity)
			where T : struct, ITag
		{
			TagMask tagMask = GetMask<T>();
			tagContainer.RemoveTags(entity, tagMask);
		}

		public IComponentContainer<T> GetContainer<T>()
			where T : struct, IComponent
		{
			TagID id = GetID<T>();
			return ((IComponentContainer<T>)containers[id]);
		}

		public TagID GetID<T>()
			where T : struct, ITag
		{
			return reflector.GetID<T>();
		}

		public TagMask GetMask<T1>()
			where T1 : struct, ITag
		{
			return new TagMask(GetID<T1>());
		}

		public void Dispose()
		{
			tagContainer.Dispose();
		}
    }
}