using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace AiInGames.Blackboard
{
    [CreateAssetMenu(fileName = "New Blackboard", menuName = "AI/Blackboard", order = 1)]
    public class Blackboard : ScriptableObject, IBlackboard, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<BlackboardEntry> m_Entries = new List<BlackboardEntry>();

        [SerializeField]
        Blackboard m_ParentBlackboard;

        // Cached counts for exact dictionary capacity on deserialization (no counting pass needed)
        [SerializeField] int m_IntCount;
        [SerializeField] int m_FloatCount;
        [SerializeField] int m_BoolCount;
        [SerializeField] int m_StringCount;
        [SerializeField] int m_Vector3Count;
        [SerializeField] int m_GameObjectCount;
        [SerializeField] int m_TransformCount;
        [SerializeField] int m_IntListCount;
        [SerializeField] int m_FloatListCount;
        [SerializeField] int m_BoolListCount;
        [SerializeField] int m_StringListCount;
        [SerializeField] int m_Vector3ListCount;
        [SerializeField] int m_GameObjectListCount;
        [SerializeField] int m_TransformListCount;

        // Type-specific dictionaries - ZERO boxing
        Dictionary<string, int> m_IntValues;
        Dictionary<string, float> m_FloatValues;
        Dictionary<string, bool> m_BoolValues;
        Dictionary<string, string> m_StringValues;
        Dictionary<string, Vector3> m_Vector3Values;
        Dictionary<string, GameObject> m_GameObjectValues;
        Dictionary<string, Transform> m_TransformValues;
        Dictionary<string, List<int>> m_IntListValues;
        Dictionary<string, List<float>> m_FloatListValues;
        Dictionary<string, List<bool>> m_BoolListValues;
        Dictionary<string, List<string>> m_StringListValues;
        Dictionary<string, List<Vector3>> m_Vector3ListValues;
        Dictionary<string, List<GameObject>> m_GameObjectListValues;
        Dictionary<string, List<Transform>> m_TransformListValues;

        Dictionary<string, Action> m_ChangeListeners;

        bool m_IsInitialized;

        public Blackboard Parent
        {
            get => m_ParentBlackboard;
            set
            {
                if (value == this)
                {
                    Debug.LogError("Blackboard cannot be its own parent");
                    return;
                }

                if (value != null && WouldCreateCycle(value))
                {
                    Debug.LogError("Cannot set parent: would create circular reference");
                    return;
                }

                m_ParentBlackboard = value;
            }
        }

        bool WouldCreateCycle(Blackboard potentialParent)
        {
            var current = potentialParent;
            while (current != null)
            {
                if (current == this)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        public void OnBeforeSerialize()
        {
            // Cache type counts for fast deserialization
            if (m_Entries == null || m_Entries.Count == 0)
            {
                m_IntCount = m_FloatCount = m_BoolCount = m_StringCount = 0;
                m_Vector3Count = m_GameObjectCount = m_TransformCount = 0;
                m_IntListCount = m_FloatListCount = m_BoolListCount = m_StringListCount = 0;
                m_Vector3ListCount = m_GameObjectListCount = m_TransformListCount = 0;
                return;
            }

            m_IntCount = m_FloatCount = m_BoolCount = m_StringCount = 0;
            m_Vector3Count = m_GameObjectCount = m_TransformCount = 0;
            m_IntListCount = m_FloatListCount = m_BoolListCount = m_StringListCount = 0;
            m_Vector3ListCount = m_GameObjectListCount = m_TransformListCount = 0;

            foreach (var entry in m_Entries)
            {
                if (entry == null || entry.key.ValueType == null) continue;

                var valueType = entry.key.ValueType;
                if (valueType == typeof(int)) m_IntCount++;
                else if (valueType == typeof(float)) m_FloatCount++;
                else if (valueType == typeof(bool)) m_BoolCount++;
                else if (valueType == typeof(string)) m_StringCount++;
                else if (valueType == typeof(Vector3)) m_Vector3Count++;
                else if (valueType == typeof(GameObject)) m_GameObjectCount++;
                else if (valueType == typeof(Transform)) m_TransformCount++;
                else if (valueType == typeof(List<int>)) m_IntListCount++;
                else if (valueType == typeof(List<float>)) m_FloatListCount++;
                else if (valueType == typeof(List<bool>)) m_BoolListCount++;
                else if (valueType == typeof(List<string>)) m_StringListCount++;
                else if (valueType == typeof(List<Vector3>)) m_Vector3ListCount++;
                else if (valueType == typeof(List<GameObject>)) m_GameObjectListCount++;
                else if (valueType == typeof(List<Transform>)) m_TransformListCount++;
            }
        }

        public void OnAfterDeserialize()
        {
            RebuildDictionaries();
        }

        internal void RebuildDictionaries(bool notifyChanges = false)
        {
            // Lazy initialization - only allocate dictionaries for types that are actually used
            m_IntValues = null;
            m_FloatValues = null;
            m_BoolValues = null;
            m_StringValues = null;
            m_Vector3Values = null;
            m_GameObjectValues = null;
            m_TransformValues = null;
            m_IntListValues = null;
            m_FloatListValues = null;
            m_BoolListValues = null;
            m_StringListValues = null;
            m_Vector3ListValues = null;
            m_GameObjectListValues = null;
            m_TransformListValues = null;

            m_ChangeListeners ??= new Dictionary<string, Action>();
            m_IsInitialized = true;

            if (m_Entries == null || m_Entries.Count == 0)
                return;

            // Use cached counts from serialization - no counting pass needed!
            foreach (var entry in m_Entries)
            {
                entry.Initialize();
                if (entry.key.ValueType == null)
                    continue;

                var key = entry.key.Name;
                var valueType = entry.key.ValueType;

                // Read type-specific fields directly - ZERO boxing during scene load
                // Dictionaries created with exact capacity from cached counts
                if (valueType == typeof(int))
                {
                    m_IntValues ??= new Dictionary<string, int>(m_IntCount);
                    m_IntValues[key] = entry.m_IntValue;
                }
                else if (valueType == typeof(float))
                {
                    m_FloatValues ??= new Dictionary<string, float>(m_FloatCount);
                    m_FloatValues[key] = entry.m_FloatValue;
                }
                else if (valueType == typeof(bool))
                {
                    m_BoolValues ??= new Dictionary<string, bool>(m_BoolCount);
                    m_BoolValues[key] = entry.m_BoolValue;
                }
                else if (valueType == typeof(string))
                {
                    m_StringValues ??= new Dictionary<string, string>(m_StringCount);
                    m_StringValues[key] = entry.m_StringValue;
                }
                else if (valueType == typeof(Vector3))
                {
                    m_Vector3Values ??= new Dictionary<string, Vector3>(m_Vector3Count);
                    m_Vector3Values[key] = entry.m_Vector3Value;
                }
                else if (valueType == typeof(GameObject))
                {
                    m_GameObjectValues ??= new Dictionary<string, GameObject>(m_GameObjectCount);
                    m_GameObjectValues[key] = entry.m_GameObjectValue;
                }
                else if (valueType == typeof(Transform))
                {
                    m_TransformValues ??= new Dictionary<string, Transform>(m_TransformCount);
                    m_TransformValues[key] = entry.m_TransformValue;
                }
                else if (valueType == typeof(List<int>))
                {
                    m_IntListValues ??= new Dictionary<string, List<int>>(m_IntListCount);
                    m_IntListValues[key] = entry.m_IntList;
                }
                else if (valueType == typeof(List<float>))
                {
                    m_FloatListValues ??= new Dictionary<string, List<float>>(m_FloatListCount);
                    m_FloatListValues[key] = entry.m_FloatList;
                }
                else if (valueType == typeof(List<bool>))
                {
                    m_BoolListValues ??= new Dictionary<string, List<bool>>(m_BoolListCount);
                    m_BoolListValues[key] = entry.m_BoolList;
                }
                else if (valueType == typeof(List<string>))
                {
                    m_StringListValues ??= new Dictionary<string, List<string>>(m_StringListCount);
                    m_StringListValues[key] = entry.m_StringList;
                }
                else if (valueType == typeof(List<Vector3>))
                {
                    m_Vector3ListValues ??= new Dictionary<string, List<Vector3>>(m_Vector3ListCount);
                    m_Vector3ListValues[key] = entry.m_Vector3List;
                }
                else if (valueType == typeof(List<GameObject>))
                {
                    m_GameObjectListValues ??= new Dictionary<string, List<GameObject>>(m_GameObjectListCount);
                    m_GameObjectListValues[key] = entry.m_GameObjectList;
                }
                else if (valueType == typeof(List<Transform>))
                {
                    m_TransformListValues ??= new Dictionary<string, List<Transform>>(m_TransformListCount);
                    m_TransformListValues[key] = entry.m_TransformList;
                }

                if (notifyChanges)
                {
                    NotifyChange(key);
                }
            }
        }

        void OnEnable()
        {
            EnsureInitialized();
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            EnsureInitialized();

            // Try local dictionaries using UnsafeUtility.As for zero-allocation lookups
            if (typeof(T) == typeof(int) && m_IntValues != null)
            {
                if (m_IntValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<int, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(float) && m_FloatValues != null)
            {
                if (m_FloatValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<float, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(bool) && m_BoolValues != null)
            {
                if (m_BoolValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<bool, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(string) && m_StringValues != null)
            {
                if (m_StringValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<string, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(Vector3) && m_Vector3Values != null)
            {
                if (m_Vector3Values.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<Vector3, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(GameObject) && m_GameObjectValues != null)
            {
                if (m_GameObjectValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<GameObject, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(Transform) && m_TransformValues != null)
            {
                if (m_TransformValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<Transform, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(List<int>) && m_IntListValues != null)
            {
                if (m_IntListValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<List<int>, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(List<float>) && m_FloatListValues != null)
            {
                if (m_FloatListValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<List<float>, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(List<bool>) && m_BoolListValues != null)
            {
                if (m_BoolListValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<List<bool>, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(List<string>) && m_StringListValues != null)
            {
                if (m_StringListValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<List<string>, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(List<Vector3>) && m_Vector3ListValues != null)
            {
                if (m_Vector3ListValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<List<Vector3>, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(List<GameObject>) && m_GameObjectListValues != null)
            {
                if (m_GameObjectListValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<List<GameObject>, T>(ref v);
                    return true;
                }
            }
            else if (typeof(T) == typeof(List<Transform>) && m_TransformListValues != null)
            {
                if (m_TransformListValues.TryGetValue(key, out var v))
                {
                    value = UnsafeUtility.As<List<Transform>, T>(ref v);
                    return true;
                }
            }

            var current = m_ParentBlackboard;
            while (current != null)
            {
                if (current.TryGetValue(key, out value))
                    return true;
                current = current.Parent;
            }

            value = default;
            return false;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            return TryGetValue(key, out T value) ? value : defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            EnsureInitialized();

            // typeof + UnsafeUtility.As avoids boxing, single-pass checks for changes during set
            var temp = value;
            bool valueChanged = false;

            if (typeof(T) == typeof(int))
            {
                m_IntValues ??= new Dictionary<string, int>();
                var newVal = UnsafeUtility.As<T, int>(ref temp);
                if (!m_IntValues.TryGetValue(key, out var oldVal) || oldVal != newVal)
                {
                    m_IntValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                m_FloatValues ??= new Dictionary<string, float>();
                var newVal = UnsafeUtility.As<T, float>(ref temp);
                if (!m_FloatValues.TryGetValue(key, out var oldVal) || oldVal != newVal)
                {
                    m_FloatValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(bool))
            {
                m_BoolValues ??= new Dictionary<string, bool>();
                var newVal = UnsafeUtility.As<T, bool>(ref temp);
                if (!m_BoolValues.TryGetValue(key, out var oldVal) || oldVal != newVal)
                {
                    m_BoolValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                m_StringValues ??= new Dictionary<string, string>();
                var newVal = UnsafeUtility.As<T, string>(ref temp);
                if (!m_StringValues.TryGetValue(key, out var oldVal) || oldVal != newVal)
                {
                    m_StringValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(Vector3))
            {
                m_Vector3Values ??= new Dictionary<string, Vector3>();
                var newVal = UnsafeUtility.As<T, Vector3>(ref temp);
                if (!m_Vector3Values.TryGetValue(key, out var oldVal) || oldVal != newVal)
                {
                    m_Vector3Values[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(GameObject))
            {
                m_GameObjectValues ??= new Dictionary<string, GameObject>();
                var newVal = UnsafeUtility.As<T, GameObject>(ref temp);
                if (!m_GameObjectValues.TryGetValue(key, out var oldVal) || oldVal != newVal)
                {
                    m_GameObjectValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(Transform))
            {
                m_TransformValues ??= new Dictionary<string, Transform>();
                var newVal = UnsafeUtility.As<T, Transform>(ref temp);
                if (!m_TransformValues.TryGetValue(key, out var oldVal) || oldVal != newVal)
                {
                    m_TransformValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(List<int>))
            {
                m_IntListValues ??= new Dictionary<string, List<int>>();
                var newVal = UnsafeUtility.As<T, List<int>>(ref temp);
                if (!m_IntListValues.TryGetValue(key, out var oldVal) || !ReferenceEquals(oldVal, newVal))
                {
                    m_IntListValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(List<float>))
            {
                m_FloatListValues ??= new Dictionary<string, List<float>>();
                var newVal = UnsafeUtility.As<T, List<float>>(ref temp);
                if (!m_FloatListValues.TryGetValue(key, out var oldVal) || !ReferenceEquals(oldVal, newVal))
                {
                    m_FloatListValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(List<bool>))
            {
                m_BoolListValues ??= new Dictionary<string, List<bool>>();
                var newVal = UnsafeUtility.As<T, List<bool>>(ref temp);
                if (!m_BoolListValues.TryGetValue(key, out var oldVal) || !ReferenceEquals(oldVal, newVal))
                {
                    m_BoolListValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(List<string>))
            {
                m_StringListValues ??= new Dictionary<string, List<string>>();
                var newVal = UnsafeUtility.As<T, List<string>>(ref temp);
                if (!m_StringListValues.TryGetValue(key, out var oldVal) || !ReferenceEquals(oldVal, newVal))
                {
                    m_StringListValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(List<Vector3>))
            {
                m_Vector3ListValues ??= new Dictionary<string, List<Vector3>>();
                var newVal = UnsafeUtility.As<T, List<Vector3>>(ref temp);
                if (!m_Vector3ListValues.TryGetValue(key, out var oldVal) || !ReferenceEquals(oldVal, newVal))
                {
                    m_Vector3ListValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(List<GameObject>))
            {
                m_GameObjectListValues ??= new Dictionary<string, List<GameObject>>();
                var newVal = UnsafeUtility.As<T, List<GameObject>>(ref temp);
                if (!m_GameObjectListValues.TryGetValue(key, out var oldVal) || !ReferenceEquals(oldVal, newVal))
                {
                    m_GameObjectListValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else if (typeof(T) == typeof(List<Transform>))
            {
                m_TransformListValues ??= new Dictionary<string, List<Transform>>();
                var newVal = UnsafeUtility.As<T, List<Transform>>(ref temp);
                if (!m_TransformListValues.TryGetValue(key, out var oldVal) || !ReferenceEquals(oldVal, newVal))
                {
                    m_TransformListValues[key] = newVal;
                    valueChanged = true;
                }
            }
            else
            {
                throw new ArgumentException($"Unsupported type: {typeof(T).Name}");
            }

            if (valueChanged)
            {
                NotifyChange(key);
            }
        }

        public bool HasKey<T>(string key)
        {
            EnsureInitialized();

            if (typeof(T) == typeof(int) && m_IntValues != null && m_IntValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(float) && m_FloatValues != null && m_FloatValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(bool) && m_BoolValues != null && m_BoolValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(string) && m_StringValues != null && m_StringValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(Vector3) && m_Vector3Values != null && m_Vector3Values.ContainsKey(key)) return true;
            if (typeof(T) == typeof(GameObject) && m_GameObjectValues != null && m_GameObjectValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(Transform) && m_TransformValues != null && m_TransformValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(List<int>) && m_IntListValues != null && m_IntListValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(List<float>) && m_FloatListValues != null && m_FloatListValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(List<bool>) && m_BoolListValues != null && m_BoolListValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(List<string>) && m_StringListValues != null && m_StringListValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(List<Vector3>) && m_Vector3ListValues != null && m_Vector3ListValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(List<GameObject>) && m_GameObjectListValues != null && m_GameObjectListValues.ContainsKey(key)) return true;
            if (typeof(T) == typeof(List<Transform>) && m_TransformListValues != null && m_TransformListValues.ContainsKey(key)) return true;

            var current = m_ParentBlackboard;
            while (current != null)
            {
                if (current.HasKey<T>(key))
                    return true;
                current = current.Parent;
            }

            return false;
        }

        public int KeyCount()
        {
            EnsureInitialized();
            return (m_IntValues?.Count ?? 0) + (m_FloatValues?.Count ?? 0) + (m_BoolValues?.Count ?? 0) +
                   (m_StringValues?.Count ?? 0) + (m_Vector3Values?.Count ?? 0) + (m_GameObjectValues?.Count ?? 0) +
                   (m_TransformValues?.Count ?? 0) + (m_IntListValues?.Count ?? 0) + (m_FloatListValues?.Count ?? 0) +
                   (m_BoolListValues?.Count ?? 0) + (m_StringListValues?.Count ?? 0) + (m_Vector3ListValues?.Count ?? 0) +
                   (m_GameObjectListValues?.Count ?? 0) + (m_TransformListValues?.Count ?? 0);
        }

        public bool Remove<T>(string key)
        {
            EnsureInitialized();

            bool removed = false;
            if (typeof(T) == typeof(int) && m_IntValues != null) removed = m_IntValues.Remove(key);
            else if (typeof(T) == typeof(float) && m_FloatValues != null) removed = m_FloatValues.Remove(key);
            else if (typeof(T) == typeof(bool) && m_BoolValues != null) removed = m_BoolValues.Remove(key);
            else if (typeof(T) == typeof(string) && m_StringValues != null) removed = m_StringValues.Remove(key);
            else if (typeof(T) == typeof(Vector3) && m_Vector3Values != null) removed = m_Vector3Values.Remove(key);
            else if (typeof(T) == typeof(GameObject) && m_GameObjectValues != null) removed = m_GameObjectValues.Remove(key);
            else if (typeof(T) == typeof(Transform) && m_TransformValues != null) removed = m_TransformValues.Remove(key);
            else if (typeof(T) == typeof(List<int>) && m_IntListValues != null) removed = m_IntListValues.Remove(key);
            else if (typeof(T) == typeof(List<float>) && m_FloatListValues != null) removed = m_FloatListValues.Remove(key);
            else if (typeof(T) == typeof(List<bool>) && m_BoolListValues != null) removed = m_BoolListValues.Remove(key);
            else if (typeof(T) == typeof(List<string>) && m_StringListValues != null) removed = m_StringListValues.Remove(key);
            else if (typeof(T) == typeof(List<Vector3>) && m_Vector3ListValues != null) removed = m_Vector3ListValues.Remove(key);
            else if (typeof(T) == typeof(List<GameObject>) && m_GameObjectListValues != null) removed = m_GameObjectListValues.Remove(key);
            else if (typeof(T) == typeof(List<Transform>) && m_TransformListValues != null) removed = m_TransformListValues.Remove(key);

            if (removed)
            {
                NotifyChange(key);
            }

            return removed;
        }

        public void ClearAll()
        {
            EnsureInitialized();

            m_IntValues?.Clear();
            m_FloatValues?.Clear();
            m_BoolValues?.Clear();
            m_StringValues?.Clear();
            m_Vector3Values?.Clear();
            m_GameObjectValues?.Clear();
            m_TransformValues?.Clear();
            m_IntListValues?.Clear();
            m_FloatListValues?.Clear();
            m_BoolListValues?.Clear();
            m_StringListValues?.Clear();
            m_Vector3ListValues?.Clear();
            m_GameObjectListValues?.Clear();
            m_TransformListValues?.Clear();
        }

        public void Subscribe(string key, Action callback)
        {
            if (callback == null) return;

            EnsureInitialized();

            m_ChangeListeners.TryGetValue(key, out var existing);
            m_ChangeListeners[key] = existing + callback;
        }

        public void Unsubscribe(string key, Action callback)
        {
            if (callback == null) return;

            EnsureInitialized();

            if (m_ChangeListeners.TryGetValue(key, out var existing))
            {
                var updated = existing - callback;

                if (updated == null)
                {
                    m_ChangeListeners.Remove(key);
                }
                else
                {
                    m_ChangeListeners[key] = updated;
                }
            }
        }

        void NotifyChange(string key)
        {
            if (!m_ChangeListeners.TryGetValue(key, out var callback))
                return;

            // GetInvocationList allocates array - acceptable trade-off for exception safety
            if (callback != null)
            {
                foreach (Action handler in callback.GetInvocationList())
                {
                    try
                    {
                        handler?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Blackboard change listener exception:", this);
                        Debug.LogException(ex, this);
                    }
                }
            }
        }

        void EnsureInitialized()
        {
            if (m_IsInitialized)
                return;

            RebuildDictionaries();
        }
    }
}
