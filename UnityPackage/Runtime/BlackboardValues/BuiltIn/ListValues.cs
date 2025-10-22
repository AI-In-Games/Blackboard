using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace AiInGames.Blackboard
{
    [Serializable]
    public class IntListValue : BlackboardValue<List<int>>
    {
        public override string GetDisplayName() => "List<Int>";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var container = new VisualElement();
            var listView = new ListView
            {
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                headerTitle = "Elements",
                reorderable = !readOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            if (m_Value == null)
                m_Value = new List<int>();

            listView.itemsSource = m_Value;
            listView.makeItem = () => new IntegerField();
            listView.bindItem = (element, index) =>
            {
                var field = element as IntegerField;
                field.value = m_Value[index];
                field.SetEnabled(!readOnly);
                if (!readOnly)
                {
                    field.RegisterValueChangedCallback(evt =>
                    {
                        m_Value[index] = evt.newValue;
                        onValueChanged?.Invoke(m_Value);
                    });
                }
            };

            container.Add(listView);
            return container;
        }
#endif
    }

    [Serializable]
    public class FloatListValue : BlackboardValue<List<float>>
    {
        public override string GetDisplayName() => "List<Float>";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var container = new VisualElement();
            var listView = new ListView
            {
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                headerTitle = "Elements",
                reorderable = !readOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            if (m_Value == null)
                m_Value = new List<float>();

            listView.itemsSource = m_Value;
            listView.makeItem = () => new FloatField();
            listView.bindItem = (element, index) =>
            {
                var field = element as FloatField;
                field.value = m_Value[index];
                field.SetEnabled(!readOnly);
                if (!readOnly)
                {
                    field.RegisterValueChangedCallback(evt =>
                    {
                        m_Value[index] = evt.newValue;
                        onValueChanged?.Invoke(m_Value);
                    });
                }
            };

            container.Add(listView);
            return container;
        }
#endif
    }

    [Serializable]
    public class BoolListValue : BlackboardValue<List<bool>>
    {
        public override string GetDisplayName() => "List<Bool>";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var container = new VisualElement();
            var listView = new ListView
            {
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                headerTitle = "Elements",
                reorderable = !readOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            if (m_Value == null)
                m_Value = new List<bool>();

            listView.itemsSource = m_Value;
            listView.makeItem = () => new Toggle();
            listView.bindItem = (element, index) =>
            {
                var field = element as Toggle;
                field.value = m_Value[index];
                field.SetEnabled(!readOnly);
                if (!readOnly)
                {
                    field.RegisterValueChangedCallback(evt =>
                    {
                        m_Value[index] = evt.newValue;
                        onValueChanged?.Invoke(m_Value);
                    });
                }
            };

            container.Add(listView);
            return container;
        }
#endif
    }

    [Serializable]
    public class StringListValue : BlackboardValue<List<string>>
    {
        public override string GetDisplayName() => "List<String>";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var container = new VisualElement();
            var listView = new ListView
            {
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                headerTitle = "Elements",
                reorderable = !readOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            if (m_Value == null)
                m_Value = new List<string>();

            listView.itemsSource = m_Value;
            listView.makeItem = () => new TextField();
            listView.bindItem = (element, index) =>
            {
                var field = element as TextField;
                field.value = m_Value[index] ?? string.Empty;
                field.SetEnabled(!readOnly);
                if (!readOnly)
                {
                    field.RegisterValueChangedCallback(evt =>
                    {
                        m_Value[index] = evt.newValue;
                        onValueChanged?.Invoke(m_Value);
                    });
                }
            };

            container.Add(listView);
            return container;
        }
#endif
    }

    [Serializable]
    public class Vector3ListValue : BlackboardValue<List<Vector3>>
    {
        public override string GetDisplayName() => "List<Vector3>";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var container = new VisualElement();
            var listView = new ListView
            {
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                headerTitle = "Elements",
                reorderable = !readOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            if (m_Value == null)
                m_Value = new List<Vector3>();

            listView.itemsSource = m_Value;
            listView.makeItem = () => new Vector3Field();
            listView.bindItem = (element, index) =>
            {
                var field = element as Vector3Field;
                field.value = m_Value[index];
                field.SetEnabled(!readOnly);
                if (!readOnly)
                {
                    field.RegisterValueChangedCallback(evt =>
                    {
                        m_Value[index] = evt.newValue;
                        onValueChanged?.Invoke(m_Value);
                    });
                }
            };

            container.Add(listView);
            return container;
        }
#endif
    }

    [Serializable]
    public class GameObjectListValue : BlackboardValue<List<GameObject>>
    {
        public override string GetDisplayName() => "List<GameObject>";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var container = new VisualElement();
            var listView = new ListView
            {
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                headerTitle = "Elements",
                reorderable = !readOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            if (m_Value == null)
                m_Value = new List<GameObject>();

            listView.itemsSource = m_Value;
            listView.makeItem = () => new ObjectField { objectType = typeof(GameObject) };
            listView.bindItem = (element, index) =>
            {
                var field = element as ObjectField;
                field.value = m_Value[index];
                field.SetEnabled(!readOnly);
                if (!readOnly)
                {
                    field.RegisterValueChangedCallback(evt =>
                    {
                        m_Value[index] = evt.newValue as GameObject;
                        onValueChanged?.Invoke(m_Value);
                    });
                }
            };

            container.Add(listView);
            return container;
        }
#endif
    }

    [Serializable]
    public class TransformListValue : BlackboardValue<List<Transform>>
    {
        public override string GetDisplayName() => "List<Transform>";

#if UNITY_EDITOR
        public override VisualElement CreateInspectorElement(bool readOnly, Action<object> onValueChanged)
        {
            var container = new VisualElement();
            var listView = new ListView
            {
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                headerTitle = "Elements",
                reorderable = !readOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            if (m_Value == null)
                m_Value = new List<Transform>();

            listView.itemsSource = m_Value;
            listView.makeItem = () => new ObjectField { objectType = typeof(Transform) };
            listView.bindItem = (element, index) =>
            {
                var field = element as ObjectField;
                field.value = m_Value[index];
                field.SetEnabled(!readOnly);
                if (!readOnly)
                {
                    field.RegisterValueChangedCallback(evt =>
                    {
                        m_Value[index] = evt.newValue as Transform;
                        onValueChanged?.Invoke(m_Value);
                    });
                }
            };

            container.Add(listView);
            return container;
        }
#endif
    }
}
