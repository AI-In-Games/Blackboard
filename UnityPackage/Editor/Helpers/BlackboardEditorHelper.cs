using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AiInGames.Blackboard.Editor
{
    /// <summary>
    /// Editor helper for manipulating Blackboard serialized data.
    /// Uses SerializedProperty for proper Unity editor integration.
    /// </summary>
    internal static class BlackboardEditorHelper
    {
        public static readonly (string displayName, Type type)[] SupportedTypes = new[]
        {
            ("Int", typeof(int)),
            ("Float", typeof(float)),
            ("Bool", typeof(bool)),
            ("String", typeof(string)),
            ("Vector3", typeof(Vector3)),
            ("GameObject", typeof(GameObject)),
            ("Transform", typeof(Transform)),
            ("List<Int>", typeof(System.Collections.Generic.List<int>)),
            ("List<Float>", typeof(System.Collections.Generic.List<float>)),
            ("List<Bool>", typeof(System.Collections.Generic.List<bool>)),
            ("List<String>", typeof(System.Collections.Generic.List<string>)),
            ("List<Vector3>", typeof(System.Collections.Generic.List<Vector3>)),
            ("List<GameObject>", typeof(System.Collections.Generic.List<GameObject>)),
            ("List<Transform>", typeof(System.Collections.Generic.List<Transform>))
        };

        const string k_EntriesKey = "m_Entries";
        const string k_KeyKey = "m_Key";
        const string k_NameKey = "m_Name";
        const string k_ParentBlackboard = "m_ParentBlackboard";
        const string k_ValuesKey = "m_Values";
        const string k_TypesKey = "m_Types";

        public static string GetDisplayName(Type type)
        {
            if (type == null) return "Unknown";

            foreach (var (displayName, t) in SupportedTypes)
            {
                if (t == type)
                    return displayName;
            }

            return type.Name;
        }

        public static void SetValue(Blackboard blackboard, string keyName, Type valueType, object value, bool checkParentConflict = false)
        {
            // Check if key exists in parent blackboard
            if (checkParentConflict && blackboard.Parent != null)
            {
                if (IsKeyInParent(blackboard.Parent, keyName))
                {
                    UnityEngine.Debug.LogError($"Cannot create key '{keyName}': key already exists in parent blackboard hierarchy");
                    return;
                }
            }

            // Convert arrays to lists for serialization (Unity can serialize List<T> but not T[])
            if (valueType.IsArray && value != null && value is System.Array array)
            {
                var elementType = valueType.GetElementType();
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = System.Activator.CreateInstance(listType) as System.Collections.IList;

                foreach (var item in array)
                {
                    list.Add(item);
                }

                value = list;
            }

            // Get m_Entries list via reflection
            var entriesField = blackboard.GetType().GetField(k_EntriesKey,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var entriesList = entriesField.GetValue(blackboard) as System.Collections.Generic.List<BlackboardEntry>;

            if (entriesList == null)
            {
                entriesList = new System.Collections.Generic.List<BlackboardEntry>();
                entriesField.SetValue(blackboard, entriesList);
            }

            // Find or create entry
            BlackboardEntry existingEntry = null;
            object oldValue = null;
            bool hadValue = false;

            foreach (var entry in entriesList)
            {
                if (entry.key.Name == keyName)
                {
                    existingEntry = entry;
                    oldValue = entry.GetValue();
                    hadValue = true;
                    break;
                }
            }

            // Check if value actually changed
            bool valueChanged = !hadValue || !AreValuesEqual(oldValue, value, valueType);

            if (existingEntry != null)
            {
                // Update existing entry
                existingEntry.SetValue(value);
            }
            else
            {
                // Create new entry
                var keyData = new BlackboardKeyData(keyName, valueType);
                var newEntry = new BlackboardEntry(keyData, value);
                entriesList.Add(newEntry);
            }

            // Rebuild runtime dictionaries and fire change notifications only if value changed
            blackboard.RebuildDictionaries(notifyChanges: valueChanged);

            EditorUtility.SetDirty(blackboard);
        }

        public static IEnumerable<(string name, Type type, object value)> GetAllEntries(Blackboard blackboard, bool includeInherited = false)
        {
            var visited = new System.Collections.Generic.HashSet<Blackboard>();
            return GetAllEntriesInternal(blackboard, includeInherited, visited);
        }

        static IEnumerable<(string name, Type type, object value)> GetAllEntriesInternal(Blackboard blackboard, bool includeInherited, System.Collections.Generic.HashSet<Blackboard> visited)
        {
            if (blackboard == null || !visited.Add(blackboard))
                yield break;

            var blackboardType = blackboard.GetType();

            // Get the entries list to know what keys exist in the asset
            var entriesListField = blackboardType.GetField(k_EntriesKey,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var entriesList = entriesListField?.GetValue(blackboard) as System.Collections.Generic.List<BlackboardEntry>;

            // Get runtime dictionaries
            var valuesField = blackboardType.GetField(k_ValuesKey,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var valuesDictionary = valuesField?.GetValue(blackboard) as System.Collections.Generic.Dictionary<string, object>;

            var typesField = blackboardType.GetField(k_TypesKey,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var typesDictionary = typesField?.GetValue(blackboard) as System.Collections.Generic.Dictionary<string, Type>;

            // Track which keys we've already yielded
            var yieldedKeys = new System.Collections.Generic.HashSet<string>();

            // First, yield entries from the asset (these have proper names)
            if (entriesList != null)
            {
                foreach (var entry in entriesList)
                {
                    if (entry == null || entry.key.ValueType == null)
                        continue;

                    var name = entry.key.Name;
                    var type = entry.key.ValueType;
                    object value;

                    // During play mode, use runtime dictionary if available
                    if (valuesDictionary != null && valuesDictionary.ContainsKey(name))
                    {
                        value = valuesDictionary[name];
                        // Update type from runtime dictionary if available
                        if (typesDictionary != null && typesDictionary.ContainsKey(name))
                        {
                            type = typesDictionary[name];
                        }
                    }
                    else
                    {
                        // Fall back to serialized value
                        value = entry.GetValue();
                    }

                    // Convert List<T> back to T[] if the original type was an array
                    if (type.IsArray && value != null && value is System.Collections.IList list)
                    {
                        var elementType = type.GetElementType();
                        var array = System.Array.CreateInstance(elementType, list.Count);
                        for (int i = 0; i < list.Count; i++)
                        {
                            array.SetValue(list[i], i);
                        }
                        value = array;
                    }

                    yieldedKeys.Add(name);
                    yield return (name, type, value);
                }
            }

            // Second, yield any runtime-only entries (not in asset, but set at runtime)
            if (valuesDictionary != null && typesDictionary != null)
            {
                foreach (var kvp in valuesDictionary)
                {
                    var name = kvp.Key;

                    // Skip if already yielded from asset entries
                    if (yieldedKeys.Contains(name))
                        continue;

                    var value = kvp.Value;
                    var type = typesDictionary.ContainsKey(name) ? typesDictionary[name] : value?.GetType();

                    if (type != null)
                    {
                        yield return (name, type, value);
                    }
                }
            }

            // Finally, yield inherited entries from parent if requested
            if (includeInherited && blackboard.Parent != null)
            {
                foreach (var entry in GetAllEntriesInternal(blackboard.Parent, includeInherited: true, visited))
                {
                    // Only yield parent entries if not overridden locally
                    if (!yieldedKeys.Contains(entry.name))
                    {
                        yield return entry;
                    }
                }
            }
        }

        public static void ForceReinitialize(Blackboard blackboard)
        {
            // Rebuild runtime dictionaries from current serialized state
            EditorUtility.SetDirty(blackboard);
            var serializedObject = new SerializedObject(blackboard);
            serializedObject.Update();

            blackboard.RebuildDictionaries(notifyChanges: false);
        }

        public static bool IsKeyInParent(Blackboard parent, string keyName)
        {
            var current = parent;
            while (current != null)
            {
                // Directly check m_Entries without recursion
                var entriesField = current.GetType().GetField(k_EntriesKey,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entriesList = entriesField?.GetValue(current) as System.Collections.Generic.List<BlackboardEntry>;

                if (entriesList != null)
                {
                    foreach (var entry in entriesList)
                    {
                        if (entry != null && entry.key.Name == keyName)
                            return true;
                    }
                }

                current = current.Parent;
            }
            return false;
        }

        static bool AreValuesEqual(object oldValue, object newValue, Type valueType)
        {
            if (oldValue == null && newValue == null)
                return true;

            if (oldValue == null || newValue == null)
                return false;

            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var oldList = oldValue as System.Collections.IList;
                var newList = newValue as System.Collections.IList;

                if (oldList == null || newList == null)
                    return oldList == newList;

                if (oldList.Count != newList.Count)
                    return false;

                for (int i = 0; i < oldList.Count; i++)
                {
                    if (!System.Collections.Generic.EqualityComparer<object>.Default.Equals(oldList[i], newList[i]))
                        return false;
                }

                return true;
            }

            return System.Collections.Generic.EqualityComparer<object>.Default.Equals(oldValue, newValue);
        }
    }
}
