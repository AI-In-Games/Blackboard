using System;
using System.Collections.Generic;
using UnityEngine;

namespace AiInGames.Blackboard
{
    public partial class BlackboardAsset : ISerializationCallbackReceiver
    {
        [SerializeReference]
        internal List<BlackboardValue> m_Values = new List<BlackboardValue>();

        [SerializeField]
        internal BlackboardAsset m_ParentBlackboard;

        public void OnBeforeSerialize()
        {
            if (m_Runtime != null)
                SyncRuntimeToValues();
        }

        void SyncRuntimeToValues()
        {
            foreach (var kvp in m_Runtime.m_Values)
            {
                var valueType = m_Runtime.m_Types[kvp.Key];
                var entry = FindOrCreateValue(kvp.Key, valueType);
                entry.SetValue(kvp.Value);
            }
        }

        BlackboardValue FindOrCreateValue(string keyName, System.Type valueType)
        {
            foreach (var value in m_Values)
            {
                if (value != null && value.Key == keyName)
                    return value;
            }

            var newEntry = BlackboardValuesFactory.CreateEntry(keyName, valueType);
            if (newEntry != null)
            {
                m_Values.Add(newEntry);
            }
            return newEntry;
        }

        public void OnAfterDeserialize()
        {
            BuildRuntimeDictionaries();
        }

        void BuildRuntimeDictionaries()
        {
            Dictionary<string, object> values = null;
            Dictionary<string, Type> types = null;

            if (m_Values.Count > 0)
            {
                values = new Dictionary<string, object>(m_Values.Count);
                types = new Dictionary<string, Type>(m_Values.Count);

                foreach (var entry in m_Values)
                {
                    if (entry == null)
                        continue;

                    values[entry.Key] = entry.GetValue();
                    types[entry.Key] = entry.GetValueType();
                }
            }

            if (m_Runtime == null)
                m_Runtime = new Blackboard(values, types);
            else
            {
                m_Runtime.m_Values = values ?? new Dictionary<string, object>();
                m_Runtime.m_Types = types ?? new Dictionary<string, Type>();
            }

            // Set parent (accessing Runtime property ensures parent is initialized)
            m_Runtime.Parent = m_ParentBlackboard != null ? m_ParentBlackboard.Runtime : null;
        }
    }
}
