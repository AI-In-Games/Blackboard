using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AiInGames.Blackboard
{
    public class Blackboard : IBlackboard
    {
        internal Dictionary<string, object> m_Values;
        internal Dictionary<string, Type> m_Types;

        Dictionary<string, Action> m_ChangeListeners;

        IReadOnlyBlackboard m_ParentBlackboard;
        public event Action<string> OnAnyValueChanged;

        public IReadOnlyBlackboard Parent
        {
            get => m_ParentBlackboard;
            set
            {
                if (value == this)
                {
                    UnityEngine.Debug.LogError("Blackboard cannot be its own parent");
                    return;
                }

                if (value != null && WouldCreateCycle(this, value))
                {
                    UnityEngine.Debug.LogError("Cannot set parent: would create circular reference");
                    return;
                }

                m_ParentBlackboard = value;
            }
        }

        static bool WouldCreateCycle(IReadOnlyBlackboard child, IReadOnlyBlackboard potentialParent)
        {
            var current = potentialParent;
            while (current != null)
            {
                if (current == child)
                    return true;

                current = current.Parent;
            }
            return false;
        }

        public Blackboard(Dictionary<string, object> values = null, Dictionary<string, Type> types = null)
        {
            m_Values = values ?? new Dictionary<string, object>();
            m_Types = types ?? new Dictionary<string, Type>();
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (m_Values.TryGetValue(key, out var obj) && obj is T typedValue)
            {
                value = typedValue;
                return true;
            }

            if (m_ParentBlackboard != null)
                return m_ParentBlackboard.TryGetValue(key, out value);

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue<T>(string key, T defaultValue = default)
        {
            return TryGetValue(key, out T value) ? value : defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            if (!m_Values.TryGetValue(key, out var oldValue) || !Equals(oldValue, value))
            {
                m_Values[key] = value;
                m_Types[key] = typeof(T);
                NotifyChange(key);
            }
        }

        internal void SetValue(string key, Type type, object value)
        {
            if (!m_Values.TryGetValue(key, out var oldValue) || !Equals(oldValue, value))
            {
                m_Values[key] = value;
                m_Types[key] = type;
                NotifyChange(key);
            }
        }

        public bool HasKey<T>(string key)
        {
            if (m_Types.TryGetValue(key, out var storedType) && storedType == typeof(T))
                return true;

            if (m_ParentBlackboard != null)
                return m_ParentBlackboard.HasKey<T>(key);

            return false;
        }

        public bool Remove<T>(string key)
        {
            if (m_Types.TryGetValue(key, out var storedType) && storedType == typeof(T))
            {
                var removed = m_Values.Remove(key) && m_Types.Remove(key);
                if (removed)
                    m_ChangeListeners?.Remove(key);
                return removed;
            }
            return false;
        }

        public void ClearAll()
        {
            m_Values.Clear();
            m_Types.Clear();
            m_ChangeListeners?.Clear();
        }

        public void Subscribe(string key, Action callback)
        {
            m_ChangeListeners ??= new Dictionary<string, Action>();

            if (m_ChangeListeners.TryGetValue(key, out var existing))
                m_ChangeListeners[key] = existing + callback;
            else
                m_ChangeListeners[key] = callback;
        }

        public void Unsubscribe(string key, Action callback)
        {
            if (m_ChangeListeners != null && m_ChangeListeners.TryGetValue(key, out var existing))
                m_ChangeListeners[key] = existing - callback;
        }

        internal void NotifyChange(string key)
        {
            if (m_ChangeListeners != null && m_ChangeListeners.TryGetValue(key, out var callback))
                callback?.Invoke();

            OnAnyValueChanged?.Invoke(key);
        }

        public IEnumerable<(string key, Type type, object value)> GetAllEntries()
        {
            foreach (var kvp in m_Values)
            {
                if (m_Types.TryGetValue(kvp.Key, out var type))
                    yield return (kvp.Key, type, kvp.Value);
            }
        }
    }
}
