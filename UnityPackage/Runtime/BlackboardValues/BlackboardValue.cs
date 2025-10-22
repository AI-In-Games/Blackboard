using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace AiInGames.Blackboard
{
    [Serializable]
    public abstract class BlackboardValue
    {
        [SerializeField] internal string m_Key;

        public string Key
        {
            get => m_Key;
            set => m_Key = value;
        }

        public abstract Type GetValueType();
        public abstract object GetValue();
        public abstract void SetValue(object value);

        public virtual string GetDisplayName()
        {
            return GetValueType().Name;
        }

#if UNITY_EDITOR
        public virtual VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            return null;
        }
#endif
    }

    [Serializable]
    public abstract class BlackboardValue<T> : BlackboardValue
    {
        [SerializeField] internal T m_Value;

        public override Type GetValueType() => typeof(T);
        public override object GetValue() => m_Value;
        public override void SetValue(object value) => m_Value = (T)value;
    }
}
