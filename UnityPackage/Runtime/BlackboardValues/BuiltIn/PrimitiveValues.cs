using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace AiInGames.Blackboard
{
    [Serializable]
    public class IntValue : BlackboardValue<int>
    {
        public override string GetDisplayName() => "Int";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var field = new IntegerField { value = m_Value };
            field.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    m_Value = evt.newValue;
                    onValueChanged(evt.newValue);
                });
            }
            return field;
        }
#endif
    }

    [Serializable]
    public class FloatValue : BlackboardValue<float>
    {
        public override string GetDisplayName() => "Float";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var field = new FloatField { value = m_Value };
            field.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    m_Value = evt.newValue;
                    onValueChanged(evt.newValue);
                });
            }
            return field;
        }
#endif
    }

    [Serializable]
    public class BoolValue : BlackboardValue<bool>
    {
        public override string GetDisplayName() => "Bool";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var field = new Toggle { value = m_Value };
            field.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    m_Value = evt.newValue;
                    onValueChanged(evt.newValue);
                });
            }
            return field;
        }
#endif
    }

    [Serializable]
    public class StringValue : BlackboardValue<string>
    {
        public override string GetDisplayName() => "String";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var field = new TextField { value = m_Value ?? string.Empty, isDelayed = true };
            field.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    m_Value = evt.newValue;
                    onValueChanged(evt.newValue);
                });
            }
            return field;
        }
#endif
    }

    [Serializable]
    public class Vector3Value : BlackboardValue<Vector3>
    {
        public override string GetDisplayName() => "Vector3";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var field = new Vector3Field { value = m_Value };
            field.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    m_Value = evt.newValue;
                    onValueChanged(evt.newValue);
                });
            }
            return field;
        }
#endif
    }

    [Serializable]
    public class GameObjectValue : BlackboardValue<GameObject>
    {
        public override string GetDisplayName() => "GameObject";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var field = new ObjectField
            {
                objectType = typeof(GameObject),
                value = m_Value
            };
            field.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    m_Value = evt.newValue as GameObject;
                    onValueChanged(evt.newValue);
                });
            }
            return field;
        }
#endif
    }

    [Serializable]
    public class TransformValue : BlackboardValue<Transform>
    {
        public override string GetDisplayName() => "Transform";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var field = new ObjectField
            {
                objectType = typeof(Transform),
                value = m_Value
            };
            field.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                field.RegisterValueChangedCallback(evt =>
                {
                    m_Value = evt.newValue as Transform;
                    onValueChanged(evt.newValue);
                });
            }
            return field;
        }
#endif
    }
}
