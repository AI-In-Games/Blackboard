using System;
using System.Collections.Generic;
using UnityEngine;

namespace AiInGames.Blackboard
{
    [Serializable]
    internal class BlackboardEntry
    {
        [SerializeField] BlackboardKeyData m_Key;

        // Unity serializes these fields directly - no boxing, no manual parsing
        // Internal access for zero-allocation scene load in RebuildDictionaries
        [SerializeField] internal int m_IntValue;
        [SerializeField] internal float m_FloatValue;
        [SerializeField] internal bool m_BoolValue;
        [SerializeField] internal string m_StringValue;
        [SerializeField] internal Vector3 m_Vector3Value;
        [SerializeField] internal GameObject m_GameObjectValue;
        [SerializeField] internal Transform m_TransformValue;

        [SerializeField] internal List<int> m_IntList;
        [SerializeField] internal List<float> m_FloatList;
        [SerializeField] internal List<bool> m_BoolList;
        [SerializeField] internal List<string> m_StringList;
        [SerializeField] internal List<Vector3> m_Vector3List;
        [SerializeField] internal List<GameObject> m_GameObjectList;
        [SerializeField] internal List<Transform> m_TransformList;

        public BlackboardKeyData key => m_Key;

        public BlackboardEntry(BlackboardKeyData key, object value)
        {
            m_Key = key;
            SetValue(value);
        }

        public void Initialize()
        {
            m_Key.Initialize();
        }

        public void SetValue(object value)
        {
            if (value == null || m_Key.ValueType == null)
                return;

            // Store in appropriate field using pattern matching
            switch (value)
            {
                case int v: m_IntValue = v; break;
                case float v: m_FloatValue = v; break;
                case bool v: m_BoolValue = v; break;
                case string v: m_StringValue = v; break;
                case Vector3 v: m_Vector3Value = v; break;
                case GameObject v: m_GameObjectValue = v; break;
                case Transform v: m_TransformValue = v; break;
                case List<int> v: m_IntList = v; break;
                case List<float> v: m_FloatList = v; break;
                case List<bool> v: m_BoolList = v; break;
                case List<string> v: m_StringList = v; break;
                case List<Vector3> v: m_Vector3List = v; break;
                case List<GameObject> v: m_GameObjectList = v; break;
                case List<Transform> v: m_TransformList = v; break;
            }
        }

        public object GetValue()
        {
            if (m_Key.ValueType == null)
                return null;

            // Return appropriate field based on type
            if (m_Key.ValueType == typeof(int)) return m_IntValue;
            if (m_Key.ValueType == typeof(float)) return m_FloatValue;
            if (m_Key.ValueType == typeof(bool)) return m_BoolValue;
            if (m_Key.ValueType == typeof(string)) return m_StringValue;
            if (m_Key.ValueType == typeof(Vector3)) return m_Vector3Value;
            if (m_Key.ValueType == typeof(GameObject)) return m_GameObjectValue;
            if (m_Key.ValueType == typeof(Transform)) return m_TransformValue;
            if (m_Key.ValueType == typeof(List<int>)) return m_IntList ?? new List<int>();
            if (m_Key.ValueType == typeof(List<float>)) return m_FloatList ?? new List<float>();
            if (m_Key.ValueType == typeof(List<bool>)) return m_BoolList ?? new List<bool>();
            if (m_Key.ValueType == typeof(List<string>)) return m_StringList ?? new List<string>();
            if (m_Key.ValueType == typeof(List<Vector3>)) return m_Vector3List ?? new List<Vector3>();
            if (m_Key.ValueType == typeof(List<GameObject>)) return m_GameObjectList ?? new List<GameObject>();
            if (m_Key.ValueType == typeof(List<Transform>)) return m_TransformList ?? new List<Transform>();

            return null;
        }
    }
}
