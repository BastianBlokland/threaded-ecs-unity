using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CompID = System.Byte;

namespace ECS.Storage
{
    public class ComponentReflector
    {
		public readonly int ComponentCount;

		private readonly Dictionary<Type, CompID> typeToIDLookup;
		private readonly Type[] idToTypeLookup;

		public ComponentReflector(Assembly assembly)
		{
			//Create lookups
			typeToIDLookup = new Dictionary<Type, CompID>(CompID.MaxValue);
			idToTypeLookup = new Type[CompID.MaxValue];
			
			//Find all the component types and add them to the lookups
			Type requiredBase = typeof(IComponent);
			CompID id = 0;
			bool full = false;
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
				if(full)
					throw new Exception("[ComponentReflector] No more then '256' components are supported!");

				typeToIDLookup[compType] = id;
				idToTypeLookup[id] = compType;

				//Increment id-counter
				if(id == 255)
					full = true;
				else
					id++;
			}

			//Store the count
			ComponentCount = id;
		}

		public CompID GetID<T>()
			where T : struct, IComponent
		{
			return GetID(typeof(T));
		}

		public CompID GetID(Type type)
		{
			CompID result;
			if(!typeToIDLookup.TryGetValue(type, out result))
				throw new Exception(string.Format("[ComponentReflector] '{0}' is not a known component", type.FullName));
			return result;
		}

		public Type GetType(CompID id)
		{
			if(id >= ComponentCount)
				throw new Exception(string.Format("[ComponentReflector] '{0}' is higher then the component-count", id));
			return idToTypeLookup[id];
		}

		public bool IsComponent(Type type)
		{
			return typeToIDLookup.ContainsKey(type);
		}
    }
}