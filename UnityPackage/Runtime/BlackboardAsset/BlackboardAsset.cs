using System.Collections.Generic;
using UnityEngine;

namespace AiInGames.Blackboard
{
    [CreateAssetMenu(fileName = "New Blackboard", menuName = "AI/Blackboard", order = 1)]
    public partial class BlackboardAsset : ScriptableObject
    {
        Blackboard m_Runtime;

        public IBlackboard Runtime
        {
            get
            {
                if (m_Runtime == null)
                    BuildRuntimeDictionaries();
                return m_Runtime;
            }
        }

        internal void SyncToRuntime(bool notifyChanges = false)
        {
            var keysToNotify = new List<string>();
            if (notifyChanges && m_Runtime != null)
            {
                if (m_Runtime.m_Values != null)
                    keysToNotify.AddRange(m_Runtime.m_Values.Keys);
            }

            BuildRuntimeDictionaries();

            if (notifyChanges && keysToNotify.Count > 0)
            {
                foreach (var key in keysToNotify)
                    m_Runtime.NotifyChange(key);
            }
        }

        internal void SyncAndNotifyKey(string keyName)
        {
            SyncToRuntime(notifyChanges: false);
            m_Runtime.NotifyChange(keyName);
        }
    }
}
