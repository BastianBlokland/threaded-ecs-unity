using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using TagID = System.Byte;

namespace ECS.Storage
{
    /// <summary>
    /// Class responsible for looking up all the tags in a given assembly and assigning them id's
    /// ID's are deterministic as long as the tag-types in the assembly are the same
    ///
    /// Thread-safety: NOT thread-safe
    /// </summary>
    public sealed class TagReflector
    {
        public int TagCount { get; }

        private readonly Dictionary<Type, TagID> typeToIDLookup;
        private readonly Type[] idToTypeLookup;

        public TagReflector(Assembly assembly)
        {
            typeToIDLookup = new Dictionary<Type, TagID>(TagID.MaxValue);
            idToTypeLookup = new Type[TagID.MaxValue];

            //Find all the tag types and add them to the lookups
            TagID id = 0;
            foreach(Type tagType in assembly
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    !type.IsGenericType &&
                    !type.IsClass &&
                    typeof(ITag).IsAssignableFrom(type))
                .OrderBy(type => type.Name)) //Order by name to make the order deterministic
            {
                typeToIDLookup[tagType] = id;
                idToTypeLookup[id] = tagType;

                id++;
                if(id >= TagMask.MAX_ENTRIES)
                    throw new Exception($"[{nameof(TagReflector)}]] No more then '64' tags are supported!");
            }

            //Store the count
            TagCount = id;
        }

        public TagID GetID<T>() where T : struct, ITag => GetID(typeof(T));

        public TagID GetID(Type type)
        {
            TagID result;
            if(!typeToIDLookup.TryGetValue(type, out result))
                throw new Exception($"[{nameof(TagReflector)}] '{type.FullName}' is not a known tag");
            return result;
        }

        public Type GetType(TagID id)
        {
            if(id >= TagCount)
                throw new Exception($"[{nameof(TagReflector)}] '{id}' is higher then the tag-count");
            return idToTypeLookup[id];
        }

        public TagMask GetMask<T>() where T : struct, ITag => new TagMask(GetID<T>());
        public TagMask GetMask(Type type) => new TagMask(GetID(type));
    }
}
