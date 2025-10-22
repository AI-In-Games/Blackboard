using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace AiInGames.Blackboard.Tests
{
    [Serializable]
    public struct CustomPlayerData
    {
        public string PlayerName;
        public int Level;
        public float Health;
    }

    [Serializable]
    public class CustomPlayerDataValue : BlackboardValue<CustomPlayerData>
    {
        public override string GetDisplayName() => "Custom Player Data";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var container = new VisualElement();
            container.style.paddingLeft = 5;

            var nameField = new TextField("Name") { value = m_Value.PlayerName };
            nameField.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                nameField.RegisterValueChangedCallback(evt =>
                {
                    m_Value.PlayerName = evt.newValue;
                    onValueChanged(m_Value);
                });
            }
            container.Add(nameField);

            var levelField = new IntegerField("Level") { value = m_Value.Level };
            levelField.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                levelField.RegisterValueChangedCallback(evt =>
                {
                    m_Value.Level = evt.newValue;
                    onValueChanged(m_Value);
                });
            }
            container.Add(levelField);

            var healthField = new FloatField("Health") { value = m_Value.Health };
            healthField.SetEnabled(!readOnly);
            if (!readOnly && onValueChanged != null)
            {
                healthField.RegisterValueChangedCallback(evt =>
                {
                    m_Value.Health = evt.newValue;
                    onValueChanged(m_Value);
                });
            }
            container.Add(healthField);

            return container;
        }
#endif
    }
}
