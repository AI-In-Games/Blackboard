using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AiInGames.Blackboard.Editor
{
    /// <summary>
    /// Editor helper for manipulating Blackboard serialized data.
    /// Uses SerializedProperty for proper Unity editor integration.
    /// </summary>
    internal static class BlackboardEditorHelper
    {
        static class FieldNames
        {
            static readonly BlackboardAsset s_Blackboard = default;
            public static readonly string Values = nameof(s_Blackboard.m_Values);
        }

        /// <summary>
        /// Gets all supported types by querying the BlackboardCustomValueFactory.
        /// This automatically includes any custom types that users have added.
        /// </summary>
        public static IEnumerable<(string displayName, Type type)> SupportedTypes
        {
            get
            {
                var types = BlackboardValuesFactory.GetAllSupportedTypes();
                foreach (var type in types.OrderBy(t => GetSortOrder(t)).ThenBy(t => GetDisplayName(t)))
                {
                    yield return (GetDisplayName(type), type);
                }
            }
        }

        static int GetSortOrder(Type type)
        {
            // Primitives first, then Unity types, then lists, then custom types
            if (type == typeof(int) || type == typeof(float) || type == typeof(bool) || type == typeof(string))
                return 0;
            if (type == typeof(Vector3) || type == typeof(GameObject) || type == typeof(Transform))
                return 1;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return 2;
            return 3;
        }

        public static string GetDisplayName(Type type)
        {
            return BlackboardValuesFactory.GetDisplayName(type);
        }

        public static void SetValue(BlackboardAsset blackboard, string keyName, Type valueType, object value, bool checkParentConflict = false, bool rebuildDictionaries = true)
        {
            // Check if key exists in parent blackboard
            if (checkParentConflict && blackboard.m_ParentBlackboard != null)
            {
                if (IsKeyInParent(blackboard.m_ParentBlackboard, keyName))
                {
                    Debug.LogError($"Cannot create key '{keyName}': key already exists in parent blackboard hierarchy");
                    return;
                }
            }

            // Convert arrays to lists for serialization (Unity can serialize List<T> but not T[])
            if (valueType.IsArray && value is System.Array array)
            {
                var elementType = valueType.GetElementType();
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = System.Activator.CreateInstance(listType) as System.Collections.IList;

                foreach (var item in array)
                {
                    list.Add(item);
                }

                value = list;
                valueType = listType;
            }

            var serializedObject = new SerializedObject(blackboard);
            serializedObject.Update();

            var valuesProp = serializedObject.FindProperty(FieldNames.Values);
            if (valuesProp == null)
            {
                Debug.LogError("Failed to find m_Values property");
                return;
            }

            // Check if value changed by reading directly from m_Values (inline to avoid duplicate SerializedObject)
            object oldValue = null;
            bool hadValue = false;
            for (int i = 0; i < valuesProp.arraySize; i++)
            {
                var currentProp = valuesProp.GetArrayElementAtIndex(i);
                var wrapper = currentProp.managedReferenceValue as BlackboardValue;

                if (wrapper != null && wrapper.Key == keyName)
                {
                    oldValue = wrapper.GetValue();
                    hadValue = true;
                    break;
                }
            }

            bool valueChanged = !hadValue || !Equals(oldValue, value);

            // If value hasn't changed, skip all modification operations
            if (!valueChanged && hadValue)
            {
                return;
            }

            var valueProp = FindOrCreateValueProperty(valuesProp, keyName, valueType);
            SetValueProperty(valueProp, value);

            serializedObject.ApplyModifiedProperties();

            // Directly update the runtime dictionary for this specific key
            if (rebuildDictionaries)
            {
                blackboard.SyncAndNotifyKey(keyName);
            }

            EditorUtility.SetDirty(blackboard);
        }

        static SerializedProperty FindOrCreateValueProperty(SerializedProperty valuesProp, string keyName, Type valueType)
        {
            // Look for existing wrapper with this key
            int existingIndex = -1;
            for (int i = 0; i < valuesProp.arraySize; i++)
            {
                var valueProp = valuesProp.GetArrayElementAtIndex(i);
                var wrapper = valueProp.managedReferenceValue as BlackboardValue;

                if (wrapper != null && wrapper.Key == keyName)
                {
                    existingIndex = i;
                    break;
                }
            }

            // If found, return the existing one
            if (existingIndex >= 0)
            {
                return valuesProp.GetArrayElementAtIndex(existingIndex);
            }

            var newEntry = BlackboardValuesFactory.CreateEntry(keyName, valueType);
            if (newEntry == null)
            {
                Debug.LogError($"Failed to create entry for type {valueType}. Make sure a BlackboardCustomValue subclass exists for this type.");
                return null;
            }

            int newIndex = valuesProp.arraySize;
            valuesProp.arraySize++;
            var newValueProp = valuesProp.GetArrayElementAtIndex(newIndex);

            newValueProp.managedReferenceValue = newEntry;

            return newValueProp;
        }

        static void SetValueProperty(SerializedProperty valueProp, object value)
        {
            if (valueProp == null)
                return;

            var entry = valueProp.managedReferenceValue as BlackboardValue;
            if (entry == null)
                return;

            var valueType = value?.GetType() ?? entry.GetValueType();

            // Always create a new entry - uniform behavior for all types
            // This is simpler and cleaner than branching between primitives and complex types
            var newEntry = BlackboardValuesFactory.CreateEntry(entry.Key, valueType);
            newEntry.SetValue(value);
            valueProp.managedReferenceValue = newEntry;
        }

        static bool TryGetCurrentValue(BlackboardAsset blackboardAsset, string keyName, out object value)
        {
            var serializedObject = new SerializedObject(blackboardAsset);
            serializedObject.Update();

            var valuesProp = serializedObject.FindProperty(FieldNames.Values);
            if (valuesProp != null)
            {
                for (int i = 0; i < valuesProp.arraySize; i++)
                {
                    var valueProp = valuesProp.GetArrayElementAtIndex(i);
                    var wrapper = valueProp.managedReferenceValue as BlackboardValue;

                    if (wrapper != null && wrapper.Key == keyName)
                    {
                        value = wrapper.GetValue();
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        public static IEnumerable<(string name, Type type, object value)> GetAllEntries(BlackboardAsset blackboard, bool includeInherited = false)
        {
            var visited = new HashSet<BlackboardAsset>();
            return GetAllEntriesInternal(blackboard, includeInherited, visited);
        }

        static IEnumerable<(string name, Type type, object value)> GetAllEntriesInternal(BlackboardAsset blackboard, bool includeInherited, HashSet<BlackboardAsset> visited)
        {
            if (blackboard == null || !visited.Add(blackboard))
                yield break;

            var yieldedKeys = new HashSet<string>();
            var serializedObject = new SerializedObject(blackboard);
            serializedObject.Update();

            var valuesProp = serializedObject.FindProperty(FieldNames.Values);
            if (valuesProp != null && valuesProp.isArray)
            {
                for (int i = 0; i < valuesProp.arraySize; i++)
                {
                    var valueProp = valuesProp.GetArrayElementAtIndex(i);
                    var wrapper = valueProp.managedReferenceValue as BlackboardValue;

                    if (wrapper != null && !string.IsNullOrEmpty(wrapper.Key))
                    {
                        yieldedKeys.Add(wrapper.Key);
                        yield return (wrapper.Key, wrapper.GetValueType(), wrapper.GetValue());
                    }
                }
            }

            // Yield inherited entries from parent if requested
            if (includeInherited && blackboard.m_ParentBlackboard != null)
            {
                foreach (var entry in GetAllEntriesInternal(blackboard.m_ParentBlackboard, includeInherited: true, visited))
                {
                    if (!yieldedKeys.Contains(entry.name))
                    {
                        yield return entry;
                    }
                }
            }
        }

        public static void ForceReinitialize(BlackboardAsset blackboard)
        {
            EditorUtility.SetDirty(blackboard);
            blackboard.SyncToRuntime(notifyChanges: false);
        }

        public static bool IsKeyInParent(BlackboardAsset parent, string keyName)
        {
            var current = parent;
            while (current != null)
            {
                // Check if key exists in this blackboard
                if (TryGetCurrentValue(current, keyName, out _))
                    return true;

                current = current.m_ParentBlackboard;
            }
            return false;
        }

        /// <summary>
        /// Syncs runtime dictionary changes back to the serialized asset.
        /// Useful when:
        /// - Creating assets via AssetDatabase.CreateAsset with runtime modifications
        /// - Test scenarios where runtime is modified directly
        /// - Debug utilities that need to persist runtime state
        /// In normal inspector usage, SetValue updates serialized data directly and is preferred.
        /// </summary>
        public static void SaveRuntimeDataToAsset(BlackboardAsset blackboardAsset)
        {
            if (blackboardAsset == null || blackboardAsset.Runtime == null)
                return;

            var serializedObject = new SerializedObject(blackboardAsset);
            serializedObject.Update();

            var valuesProp = serializedObject.FindProperty(FieldNames.Values);
            if (valuesProp == null)
            {
                Debug.LogError("Failed to find m_Values property");
                return;
            }

            // Clear existing entries
            valuesProp.ClearArray();

            // Add all runtime entries using non-generic enumeration
            foreach (var (key, type, value) in blackboardAsset.Runtime.GetAllEntries())
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                int index = valuesProp.arraySize;
                valuesProp.arraySize++;

                var entry = BlackboardValuesFactory.CreateEntry(key, type);
                entry.SetValue(value);

                valuesProp.GetArrayElementAtIndex(index).managedReferenceValue = entry;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(blackboardAsset);
        }

        public static VisualElement CreateValueField(Type valueType, object value, bool readOnly = false, Action<object> onValueChanged = null)
        {
            var entry = BlackboardValuesFactory.CreateEntry(valueType);
            if (entry == null)
                return new Label($"Unsupported type: {valueType?.Name ?? "null"}");

            if (value != null || (valueType.IsValueType && value == null))
                entry.SetValue(value ?? Activator.CreateInstance(valueType));

            return entry.CreateInspectorElement(readOnly, onValueChanged)
                   ?? new Label($"No inspector for: {valueType.Name}");
        }
    }
}
