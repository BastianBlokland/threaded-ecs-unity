using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TagID = System.Byte;

namespace ECS.Storage
{
    public sealed class ComponentReflector
    {
		public readonly int ComponentCount;

		private readonly Dictionary<Type, TagID> typeToIDLookup;
		private readonly Type[] idToTypeLookup;

		public ComponentReflector(Assembly assembly)
		{
			//Create lookups
			typeToIDLookup = new Dictionary<Type, TagID>(TagID.MaxValue);
			idToTypeLookup = new Type[TagID.MaxValue];
			
			//Find all the component types and add them to the lookups
			Type requiredBase = typeof(ITag);
			TagID id = 0;
			foreach(Type compType in assembly
				.GetTypes()
				.Where(type => 
					!type.IsAbstract && 
					!type.IsInterface &&
					!type.IsGenericType &&
					!type.IsClass && 
					requiredBase.IsAssignableFrom(type))
				.OrderBy(type => type.Name)) //Order by name to make the order deterministic
			{
				typeToIDLookup[compType] = id;
				idToTypeLookup[id] = compType;

				//Increment id-counter
				id++;
				if(id >= 64)
					throw new Exception("[ComponentReflector] No more then '64' components are supported!");
			}

			//Store the count
			ComponentCount = id;
		}

		public TagID GetID<T>()
			where T : struct, ITag
		{
			return GetID(typeof(T));
		}

		public TagID GetID(Type type)
		{
			TagID result;
			if(!typeToIDLookup.TryGetValue(type, out result))
				throw new Exception($"[ComponentReflector] '{type.FullName}' is not a known component");
			return result;
		}

		public Type GetType(TagID id)
		{
			if(id >= ComponentCount)
				throw new Exception($"[ComponentReflector] '{id}' is higher then the component-count");
			return idToTypeLookup[id];
		}

		public bool IsTag(Type type)
		{
			return typeToIDLookup.ContainsKey(type);
		}

		public bool IsComponent(Type type)
		{
			return typeof(IComponent).IsAssignableFrom(type);
		}
    }
}