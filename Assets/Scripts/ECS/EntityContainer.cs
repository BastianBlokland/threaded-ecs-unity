using System;
using System.Reflection;
using System.Collections.Generic;

using EntityID = System.UInt16;
using CompID = System.Byte;

namespace ECS
{
    public class EntityContainer
    {
		private readonly ComponentReflector reflector;
        private readonly IComponentContainer[] containers;
		private readonly ComponentMask[] entities;

		public EntityContainer(Assembly componentAssembly)
		{
			reflector = new ComponentReflector(componentAssembly);
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

		public void GetEntities(ComponentMask mask, IList<EntityID> outputList)
		{
			outputList.Clear();
			for (EntityID entity = 0; entity < EntityID.MaxValue; entity++)
			{
				if(entities[entity].Has(mask))
					outputList.Add(entity);
			}
		}

		public bool HasComponents(EntityID entity, ComponentMask mask)
		{
			return entities[entity].Has(mask);
		}

		public bool HasComponent<T>(EntityID entity)
			where T : struct, IComponent
		{
			CompID comp = GetID<T>();
			return HasComponent(entity, comp);
		}

		public bool HasComponent(EntityID entity, CompID comp)
		{
			return entities[entity].Has(comp);
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
			((IComponentContainer<T>)containers[comp]).Set(entity, data);
			entities[entity].Set(comp);
		}

		public void RemoveComponent<T>(EntityID entity)
			where T : struct, IComponent
		{
			CompID comp = GetID<T>();
			entities[entity].Unset(comp);
		}

		public void RemoveAllComponents(EntityID entity)
		{
			entities[entity].Clear();
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
    }
}