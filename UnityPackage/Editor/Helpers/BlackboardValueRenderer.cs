using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AiInGames.Blackboard.Editor
{
    /// <summary>
    /// Shared utility for rendering blackboard value fields in editor UIs.
    /// Supports both editable (design-time) and read-only (runtime) rendering.
    /// </summary>
    internal static class BlackboardValueRenderer
    {
        /// <summary>
        /// Creates a visual element for displaying/editing a blackboard value.
        /// </summary>
        /// <param name="valueType">The type of the value to render</param>
        /// <param name="value">The current value</param>
        /// <param name="readOnly">If true, creates a read-only field for runtime viewing</param>
        /// <param name="onValueChanged">Callback invoked when value changes (only for editable fields)</param>
        public static VisualElement CreateValueField(Type valueType, object value, bool readOnly = false, Action<object> onValueChanged = null)
        {
            VisualElement field = null;

            if (valueType == typeof(int))
            {
                var intField = new IntegerField { value = value != null ? (int)value : 0 };
                intField.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    intField.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = intField;
            }
            else if (valueType == typeof(float))
            {
                var floatField = new FloatField { value = value != null ? (float)value : 0f };
                floatField.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    floatField.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = floatField;
            }
            else if (valueType == typeof(bool))
            {
                var boolField = new Toggle { value = value != null && (bool)value };
                boolField.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    boolField.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = boolField;
            }
            else if (valueType == typeof(string))
            {
                var stringField = new TextField { value = value as string ?? string.Empty };
                stringField.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    stringField.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = stringField;
            }
            else if (valueType == typeof(Vector2))
            {
                var vec2Field = new Vector2Field { value = value != null ? (Vector2)value : Vector2.zero };
                vec2Field.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    vec2Field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = vec2Field;
            }
            else if (valueType == typeof(Vector3))
            {
                var vec3Field = new Vector3Field { value = value != null ? (Vector3)value : Vector3.zero };
                vec3Field.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    vec3Field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = vec3Field;
            }
            else if (valueType == typeof(Vector4))
            {
                var vec4Field = new Vector4Field { value = value != null ? (Vector4)value : Vector4.zero };
                vec4Field.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    vec4Field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = vec4Field;
            }
            else if (valueType == typeof(Quaternion))
            {
                var quat = value != null ? (Quaternion)value : Quaternion.identity;
                var vec4Field = new Vector4Field("Value (Quaternion)")
                {
                    value = new Vector4(quat.x, quat.y, quat.z, quat.w)
                };
                vec4Field.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    vec4Field.RegisterValueChangedCallback(evt =>
                        onValueChanged(new Quaternion(evt.newValue.x, evt.newValue.y, evt.newValue.z, evt.newValue.w)));
                }
                field = vec4Field;
            }
            else if (valueType == typeof(Color))
            {
                var colorField = new ColorField { value = value != null ? (Color)value : Color.white };
                colorField.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    colorField.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = colorField;
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
            {
                var objectField = new ObjectField
                {
                    objectType = valueType,
                    value = value as UnityEngine.Object
                };
                objectField.SetEnabled(!readOnly);
                if (!readOnly && onValueChanged != null)
                {
                    objectField.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
                }
                field = objectField;
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
            {
                field = CreateListField(valueType, value, readOnly, onValueChanged);
            }
            else if (valueType.IsArray)
            {
                field = CreateArrayField(valueType, value, readOnly, onValueChanged);
            }
            else
            {
                field = new Label($"Unsupported type: {valueType?.Name ?? "null"}");
            }

            return field;
        }

        private static VisualElement CreateListField(Type listType, object value, bool readOnly, Action<object> onValueChanged)
        {
            var elementType = listType.GetGenericArguments()[0];
            var list = value as IList;
            if (list == null)
            {
                list = (IList)Activator.CreateInstance(listType);
                onValueChanged?.Invoke(list);
            }

            var container = new VisualElement();
            container.AddToClassList("list-container");

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 4;

            var countLabel = new Label($"Count: {list.Count}");
            countLabel.AddToClassList("list-count-label");
            header.Add(countLabel);

            container.Add(header);

            var itemsContainer = new VisualElement();
            itemsContainer.AddToClassList("list-items-container");
            container.Add(itemsContainer);

            void RefreshListUI()
            {
                itemsContainer.Clear();
                countLabel.text = $"Count: {list.Count}";

                for (int i = 0; i < list.Count; i++)
                {
                    var index = i;
                    var item = list[index];

                    var itemRow = new VisualElement();
                    itemRow.style.flexDirection = FlexDirection.Row;
                    itemRow.style.marginBottom = 2;

                    var indexLabel = new Label($"[{index}]");
                    indexLabel.style.minWidth = 40;
                    indexLabel.style.marginRight = 4;
                    itemRow.Add(indexLabel);

                    var itemField = CreateValueField(elementType, item, readOnly, newValue =>
                    {
                        // Clone the list before modifying for proper undo support
                        var newList = CloneList(list, listType);
                        newList[index] = newValue;
                        onValueChanged?.Invoke(newList);

                        // Update reference but don't refresh (value changes are immediate in UI)
                        list = newList;
                    });
                    itemField.style.flexGrow = 1;
                    itemRow.Add(itemField);

                    if (!readOnly)
                    {
                        var removeButton = new Button(() =>
                        {
                            // Clone the list before modifying for proper undo support
                            var newList = CloneList(list, listType);
                            newList.RemoveAt(index);
                            onValueChanged?.Invoke(newList);

                            // Update reference and refresh UI
                            list = newList;
                            RefreshListUI();
                        })
                        { text = "×" };
                        removeButton.AddToClassList("list-remove-button");
                        removeButton.style.width = 20;
                        itemRow.Add(removeButton);
                    }

                    itemsContainer.Add(itemRow);
                }
            }

            if (!readOnly)
            {
                var addButton = new Button(() =>
                {
                    // Clone the list before modifying for proper undo support
                    var newList = CloneList(list, listType);
                    var defaultValue = GetDefaultValueForType(elementType);
                    newList.Add(defaultValue);
                    onValueChanged?.Invoke(newList);

                    // Update reference and refresh UI
                    list = newList;
                    RefreshListUI();
                })
                { text = "+" };
                addButton.AddToClassList("list-add-button");
                header.Add(addButton);
            }

            RefreshListUI();

            return container;
        }

        private static VisualElement CreateArrayField(Type arrayType, object value, bool readOnly, Action<object> onValueChanged)
        {
            var elementType = arrayType.GetElementType();
            var array = value as Array;

            if (array == null)
            {
                array = Array.CreateInstance(elementType, 0);
                onValueChanged?.Invoke(array);
            }

            var container = new VisualElement();
            container.AddToClassList("list-container");

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 4;

            var countLabel = new Label($"Length: {array.Length}");
            countLabel.AddToClassList("list-count-label");
            header.Add(countLabel);

            container.Add(header);

            var itemsContainer = new VisualElement();
            itemsContainer.AddToClassList("list-items-container");
            container.Add(itemsContainer);

            void RefreshArrayUI()
            {
                itemsContainer.Clear();
                countLabel.text = $"Length: {array.Length}";

                for (int i = 0; i < array.Length; i++)
                {
                    var index = i;
                    var item = array.GetValue(index);

                    var itemRow = new VisualElement();
                    itemRow.style.flexDirection = FlexDirection.Row;
                    itemRow.style.marginBottom = 2;

                    var indexLabel = new Label($"[{index}]");
                    indexLabel.style.minWidth = 40;
                    indexLabel.style.marginRight = 4;
                    itemRow.Add(indexLabel);

                    var itemField = CreateValueField(elementType, item, readOnly, newValue =>
                    {
                        if (!readOnly)
                        {
                            // Clone array before modifying
                            var newArray = Array.CreateInstance(elementType, array.Length);
                            Array.Copy(array, newArray, array.Length);
                            newArray.SetValue(newValue, index);
                            onValueChanged?.Invoke(newArray);
                            array = newArray;
                        }
                    });
                    itemField.style.flexGrow = 1;
                    itemRow.Add(itemField);

                    itemsContainer.Add(itemRow);
                }
            }

            RefreshArrayUI();

            return container;
        }

        private static IList CloneList(IList sourceList, Type listType)
        {
            var newList = (IList)Activator.CreateInstance(listType);
            foreach (var item in sourceList)
            {
                newList.Add(item);
            }
            return newList;
        }

        private static object GetDefaultValueForType(Type type)
        {
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(Vector2)) return Vector2.zero;
            if (type == typeof(Vector3)) return Vector3.zero;
            if (type == typeof(Vector4)) return Vector4.zero;
            if (type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }

        /// <summary>
        /// Creates a complete entry element with key name, type label, and value field.
        /// Used for runtime viewing in the debug window.
        /// </summary>
        public static VisualElement CreateRuntimeEntryElement(string keyName, int hash, Type valueType, object value)
        {
            var entry = new VisualElement();
            entry.AddToClassList("blackboard-entry");

            var foldout = new Foldout
            {
                text = string.IsNullOrEmpty(keyName) ? $"Key Hash: {hash}" : $"{keyName} (hash: {hash})",
                value = true
            };

            var contentContainer = new VisualElement();
            contentContainer.style.paddingLeft = 15;

            var typeLabel = new Label($"Type: {BlackboardEditorHelper.GetDisplayName(valueType)}");
            typeLabel.AddToClassList("type-label");
            contentContainer.Add(typeLabel);

            var valueField = CreateValueField(valueType, value, readOnly: true);
            valueField.AddToClassList("value-field");
            contentContainer.Add(valueField);

            foldout.Add(contentContainer);
            entry.Add(foldout);

            return entry;
        }
    }
}
