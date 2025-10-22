using System;
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
            var entry = BlackboardValuesFactory.CreateEntry(valueType);
            if (entry == null)
                return new Label($"Unsupported type: {valueType?.Name ?? "null"}");

            if (value != null || (valueType.IsValueType && value == null))
                entry.SetValue(value ?? Activator.CreateInstance(valueType));

            return entry.CreateInspectorElement(readOnly, onValueChanged)
                ?? new Label($"No inspector for: {valueType.Name}");
        }
    }
}
