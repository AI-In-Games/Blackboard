using System;
using UnityEngine;

namespace AiInGames.Blackboard
{
    [Serializable]
    public struct BlackboardKeyData
    {
        [SerializeField] 
        string m_Name;
        
        [SerializeField] 
        string m_TypeName;

        int m_NameHash;
        Type m_CachedType;

        public string Name => m_Name;
        public int NameHash => m_NameHash;

        public Type ValueType
        {
            get
            {
                if (m_CachedType == null && !string.IsNullOrEmpty(m_TypeName))
                {
                    m_CachedType = Type.GetType(m_TypeName);
                }
                return m_CachedType;
            }
        }

        public BlackboardKeyData(string name, Type valueType)
        {
            this.m_Name = name;
            this.m_TypeName = valueType?.AssemblyQualifiedName ?? string.Empty;
            this.m_NameHash = name?.GetHashCode() ?? 0;
            this.m_CachedType = valueType;
        }

        public void Initialize()
        {
            m_NameHash = m_Name?.GetHashCode() ?? 0;
            if (!string.IsNullOrEmpty(m_TypeName))
            {
                m_CachedType = Type.GetType(m_TypeName);
            }
        }
    }
}
