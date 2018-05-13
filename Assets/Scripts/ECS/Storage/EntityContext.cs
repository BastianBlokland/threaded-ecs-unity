using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

using EntityID = System.UInt16;
using TagID = System.Byte;

namespace ECS.Storage
{
	/// <summary>
	/// Responsible for creating/removing of entities, getting/setting of tags and getting/setting of components
	/// 
	/// Thread-safety: This class is thread-safe IF you don't touch the same components on the same entity from multiple threads. So its
	/// important to manually keep track of not running tasks in parallel that touch the same components on the same entities. Adding and removing
	/// entities is completely safe and also adding and removing of tags is completely safe.
	/// </summary>
    public sealed class EntityContext
    {
		private readonly EntityAllocator entityAllocator;
		private readonly TagReflector reflector;
		private readonly EntityTagContainer tagContainer;	
		private readonly IComponentContainer[] containers;
		
		public EntityContext()
			: this(componentAssembly: typeof(EntityContext).Assembly)
		{}

		public EntityContext(Assembly componentAssembly)
		{
			entityAllocator = new EntityAllocator();
			reflector = new TagReflector(componentAssembly);
			tagContainer = new EntityTagContainer();
			containers = new IComponentContainer[reflector.TagCount];
			
			//Allocate a container for each component type
			for (TagID tag = 0; tag < reflector.TagCount; tag++)
			{
				var compType = reflector.GetType(tag);
				if(typeof(IComponent).IsAssignableFrom(compType))
					containers[tag] = ComponentContainerUtils.Create(compType);
			}
		}

		public EntityID CreateEntity() => entityAllocator.Allocate();

		public bool HasEntity(EntityID entity) => entityAllocator.IsAllocated(entity);

		public void RemoveEntity(EntityID entity)
		{
			entityAllocator.Free(entity);
			tagContainer.RemoveTags(entity, TagMask.Full);
		}

		public void GetEntities(TagMask requiredTags, TagMask illegalTags, EntitySet outputSet)
			=> tagContainer.Query(requiredTags, illegalTags, outputSet);

		public int GetEntityCount(TagMask requiredTags, TagMask illegalTags)
			=> tagContainer.Query(requiredTags, illegalTags);

		public bool HasTag<T>(EntityID entity) where T : struct, ITag => tagContainer.HasTags(entity, GetMask<T>());

		public bool HasTags(EntityID entity, TagMask mask) => tagContainer.HasTags(entity, mask);

		public T GetComponent<T>(EntityID entity) where T : struct, IComponent => GetContainer<T>().Get(entity);

		public void SetTag<T>(EntityID entity)
			where T : struct, ITag
		{
			var tagMask = GetMask<T>();
			tagContainer.SetTags(entity, tagMask);
		}

		public void SetComponent<T>(EntityID entity, T data)
			where T : struct, IComponent
		{
			var tagID = GetID<T>();
			((IComponentContainer<T>)containers[tagID]).Set(entity, data);
			SetTag<T>(entity);
		}

		public void RemoveTag<T>(EntityID entity)
			where T : struct, ITag
		{
			var tagMask = GetMask<T>();
			tagContainer.RemoveTags(entity, tagMask);
		}

		public IComponentContainer<T> GetContainer<T>()
			where T : struct, IComponent
		{
			var tagID = GetID<T>();
			return ((IComponentContainer<T>)containers[tagID]);
		}

		public TagID GetID<T>() where T : struct, ITag => reflector.GetID<T>();

		public TagMask GetMask<T>() where T : struct, ITag => reflector.GetMask<T>();
    }
}