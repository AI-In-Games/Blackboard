using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#else
using System.Reflection;
#endif

namespace AiInGames.Blackboard
{
    public static class BlackboardValuesFactory
    {
        static Dictionary<Type, Type> s_ValueTypeToEntryType;

        static BlackboardValuesFactory()
        {
            DiscoverEntryTypes();
        }

        static void DiscoverEntryTypes()
        {
            s_ValueTypeToEntryType = new Dictionary<Type, Type>();
            var entryTypes = GetEntryTypes();
            PopulateEntryTypeMap(entryTypes);
        }

        static IEnumerable<Type> GetEntryTypes()
        {
#if UNITY_EDITOR
            return TypeCache.GetTypesDerivedFrom<BlackboardValue>();
#else
            var entryTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (!type.IsAbstract && typeof(BlackboardValue).IsAssignableFrom(type))
                        {
                            entryTypes.Add(type);
                        }
                    }
                }
                catch
                {
                }
            }
            return entryTypes;
#endif
        }

        static void PopulateEntryTypeMap(IEnumerable<Type> entryTypes)
        {
            foreach (var entryType in entryTypes)
            {
                if (entryType.IsAbstract)
                    continue;

                if (Activator.CreateInstance(entryType) is BlackboardValue instance)
                {
                    var valueType = instance.GetValueType();
                    s_ValueTypeToEntryType[valueType] = entryType;
                }
            }
        }

        public static BlackboardValue CreateEntry(string key, Type valueType)
        {
            if (s_ValueTypeToEntryType.TryGetValue(valueType, out var entryType))
            {
                var entry = Activator.CreateInstance(entryType) as BlackboardValue;
                if (entry != null)
                {
                    entry.Key = key;
                }
                return entry;
            }

            return null;
        }

        public static BlackboardValue CreateEntry(Type valueType)
        {
            return CreateEntry(null, valueType);
        }

        public static bool SupportsType(Type valueType)
        {
            return s_ValueTypeToEntryType.ContainsKey(valueType);
        }

        public static IEnumerable<Type> GetAllSupportedTypes()
        {
            return s_ValueTypeToEntryType.Keys;
        }

        public static string GetDisplayName(Type valueType)
        {
            if (valueType == null)
                return "Unknown";

            if (s_ValueTypeToEntryType.TryGetValue(valueType, out var entryType))
            {
                var instance = Activator.CreateInstance(entryType) as BlackboardValue;
                if (instance != null)
                {
                    return instance.GetDisplayName();
                }
            }

            return valueType.Name;
        }
    }
}
