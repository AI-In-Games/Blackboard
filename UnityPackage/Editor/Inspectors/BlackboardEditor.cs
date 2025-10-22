using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AiInGames.Blackboard.Editor.Inspectors
{
    [CustomEditor(typeof(BlackboardAsset))]
    internal class BlackboardEditor : UnityEditor.Editor
    {
        VisualElement m_Root;
        VisualElement m_EntriesContainer;

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        void OnUndoRedo()
        {
            if (m_EntriesContainer != null)
            {
                var blackboard = target as BlackboardAsset;
                ForceReinitializeBlackboard(blackboard);
                RefreshEntries();
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            m_Root = new VisualElement();
            m_Root.styleSheets.Add(Resources.Load<StyleSheet>("Styles/BlackboardEditor"));

            m_Root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Open in Blackboard Debug Window", action =>
                {
                    var blackboard = target as BlackboardAsset;
                    if (blackboard != null)
                    {
                        BlackboardDebugWindow.ShowWindow(blackboard.Runtime);
                    }
                });
            }));

            CreateHeader();
            CreateParentField();
            CreateEntriesList();
            CreateAddButton();

            return m_Root;
        }

        void CreateHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("blackboard-header");

            var title = new Label("Blackboard");
            title.AddToClassList("blackboard-title");

            var subtitle = new Label("Key-Value Storage");
            subtitle.AddToClassList("blackboard-subtitle");

            header.Add(title);
            header.Add(subtitle);
            m_Root.Add(header);
        }

        void CreateParentField()
        {
            var parentContainer = new VisualElement();
            parentContainer.AddToClassList("parent-container");

            var parentField = new ObjectField("Parent Blackboard")
            {
                objectType = typeof(BlackboardAsset),
                value = serializedObject.FindProperty("m_ParentBlackboard").objectReferenceValue
            };

            parentField.RegisterValueChangedCallback(evt =>
            {
                var blackboard = target as BlackboardAsset;
                var newParent = evt.newValue as BlackboardAsset;

                // Validate parent assignment to prevent circular references
                if (newParent == blackboard)
                {
                    Debug.LogError("Blackboard cannot be its own parent");
                    parentField.SetValueWithoutNotify(evt.previousValue);
                    return;
                }

                if (newParent != null && WouldCreateCycle(blackboard, newParent))
                {
                    Debug.LogError("Cannot set parent: would create circular reference");
                    parentField.SetValueWithoutNotify(evt.previousValue);
                    return;
                }

                using (new UndoScope(target, "Change Parent Blackboard"))
                {
                    serializedObject.FindProperty("m_ParentBlackboard").objectReferenceValue = evt.newValue;
                    serializedObject.ApplyModifiedProperties();

                    blackboard.SyncToRuntime(notifyChanges: false);
                    EditorUtility.SetDirty(target);
                    RefreshEntries();
                }
            });

            parentContainer.Add(parentField);
            m_Root.Add(parentContainer);
        }

        void CreateAddButton()
        {
            var addButton = new Button(ShowAddMenu) { text = "+" };
            addButton.AddToClassList("add-key-button");
            m_Root.Add(addButton);
        }

        void ShowAddMenu()
        {
            var menu = new GenericMenu();

            foreach (var (displayName, type) in BlackboardEditorHelper.SupportedTypes)
            {
                var menuPath = displayName.StartsWith("List<")
                    ? $"List/{displayName.Replace("List<", "").Replace(">", "")}"
                    : displayName;
                menu.AddItem(new GUIContent(menuPath), false, () => AddNewKey(type));
            }

            menu.ShowAsContext();
        }

        void CreateEntriesList()
        {
            var listContainer = new VisualElement();
            listContainer.AddToClassList("entries-list-container");

            var listHeader = new Label("Keys");
            listHeader.AddToClassList("list-header");
            listContainer.Add(listHeader);

            m_EntriesContainer = new VisualElement();
            m_EntriesContainer.AddToClassList("entries-container");

            RefreshEntries();

            listContainer.Add(m_EntriesContainer);
            m_Root.Add(listContainer);
        }

        void RefreshEntries()
        {
            m_EntriesContainer.Clear();

            var blackboard = target as BlackboardAsset;
            var localEntries = BlackboardEditorHelper.GetAllEntries(blackboard, includeInherited: false).ToList();
            var inheritedEntries = new List<(string name, Type type, object value)>();

            if (blackboard.m_ParentBlackboard != null)
            {
                var allParentEntries = BlackboardEditorHelper.GetAllEntries(blackboard.m_ParentBlackboard, includeInherited: true).ToList();
                var localKeys = new HashSet<string>(localEntries.Select(e => e.name));

                inheritedEntries = allParentEntries.Where(e => !localKeys.Contains(e.name)).ToList();
            }

            if (localEntries.Count == 0 && inheritedEntries.Count == 0)
            {
                var emptyState = new Label("No keys defined");
                emptyState.AddToClassList("empty-state");
                m_EntriesContainer.Add(emptyState);
                return;
            }

            if (localEntries.Count > 0)
            {
                var localHeader = new Label("Local Keys");
                localHeader.AddToClassList("section-header");
                m_EntriesContainer.Add(localHeader);

                foreach (var (name, type, value) in localEntries)
                {
                    var entryElement = CreateEntryElement(name, type, value, readOnly: false);
                    m_EntriesContainer.Add(entryElement);
                }
            }

            if (inheritedEntries.Count > 0)
            {
                var inheritedHeader = new Label($"Inherited Keys ({blackboard.m_ParentBlackboard.name})");
                inheritedHeader.AddToClassList("section-header");
                inheritedHeader.AddToClassList("inherited-section-header");
                m_EntriesContainer.Add(inheritedHeader);

                foreach (var (name, type, value) in inheritedEntries)
                {
                    var entryElement = CreateEntryElement(name, type, value, readOnly: true, isInherited: true);
                    m_EntriesContainer.Add(entryElement);
                }
            }
        }

        VisualElement CreateEntryElement(string keyName, Type valueType, object value, bool readOnly = false, bool isInherited = false, bool focusName = false)
        {
            var entry = new VisualElement();
            entry.AddToClassList("blackboard-entry");
            if (isInherited)
            {
                entry.AddToClassList("blackboard-entry-inherited");
            }

            var headerRow = new VisualElement();
            headerRow.AddToClassList("entry-header");

            var currentKeyName = new List<string> { keyName };
            var pendingName = new List<string> { keyName };

            var keyField = new TextField { value = keyName };
            keyField.AddToClassList("key-name-edit");

            if (readOnly)
            {
                keyField.SetEnabled(false);
            }
            else
            {
                // Update pending name as user types (no undo)
                keyField.RegisterValueChangedCallback(evt =>
                {
                    pendingName[0] = evt.newValue;
                });

                // Only create undo entry when field loses focus
                keyField.RegisterCallback<BlurEvent>(evt =>
                {
                    var newName = pendingName[0];
                    var oldName = currentKeyName[0];

                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        keyField.SetValueWithoutNotify(oldName);
                        pendingName[0] = oldName;
                        return;
                    }

                    if (newName == oldName)
                    {
                        return;
                    }

                    var blackboard = target as BlackboardAsset;
                    var existingNames = BlackboardEditorHelper.GetAllEntries(blackboard, includeInherited: false).Select(e => e.name).ToList();
                    existingNames.Remove(oldName);

                    if (existingNames.Contains(newName))
                    {
                        Debug.LogError($"Key '{newName}' already exists in this blackboard");
                        keyField.SetValueWithoutNotify(oldName);
                        pendingName[0] = oldName;
                        return;
                    }

                    // Check if key exists in parent hierarchy
                    if (blackboard.m_ParentBlackboard != null && BlackboardEditorHelper.IsKeyInParent(blackboard.m_ParentBlackboard, newName))
                    {
                        Debug.LogError($"Cannot rename to '{newName}': key already exists in parent blackboard hierarchy");
                        keyField.SetValueWithoutNotify(oldName);
                        pendingName[0] = oldName;
                        return;
                    }

                    using (new UndoScope(target, "Rename Blackboard Key"))
                    {
                        RenameKey(oldName, newName, valueType);
                        currentKeyName[0] = newName;
                        serializedObject.Update();
                    }
                });
            }

            var typeLabel = new Label(BlackboardEditorHelper.GetDisplayName(valueType));
            typeLabel.AddToClassList("type-label");

            headerRow.Add(keyField);
            headerRow.Add(typeLabel);

            if (!readOnly)
            {
                var deleteButton = new Button(() => DeleteKey(currentKeyName[0])) { text = "×" };
                deleteButton.AddToClassList("delete-button");
                headerRow.Add(deleteButton);
            }

            entry.Add(headerRow);

            var valueField = CreateValueField(keyName, valueType, value, readOnly);
            if (valueField != null)
            {
                valueField.AddToClassList("value-field");
                entry.Add(valueField);
            }

            if (focusName)
            {
                keyField.schedule.Execute(() =>
                {
                    keyField.Focus();
                    keyField.SelectAll();
                }).ExecuteLater(100);
            }

            return entry;
        }

        VisualElement CreateValueField(string keyName, Type valueType, object value, bool readOnly = false)
        {
            var blackboard = target as BlackboardAsset;

            return BlackboardValueRenderer.CreateValueField(
                valueType,
                value,
                readOnly: readOnly,
                onValueChanged: readOnly ? null : newValue =>
                {
                    using (new UndoScope(target, "Change Blackboard Value"))
                    {
                        BlackboardEditorHelper.SetValue(blackboard, keyName, valueType, newValue);
                    }
                });
        }

        void AddNewKey(Type valueType)
        {
            var blackboard = target as BlackboardAsset;
            // Include inherited keys to avoid name collisions with parent blackboard keys
            var existingNames = BlackboardEditorHelper.GetAllEntries(blackboard, includeInherited: true).Select(e => e.name).ToArray();

            var baseName = $"New{valueType.Name}";
            var uniqueName = ObjectNames.GetUniqueName(existingNames, baseName);

            using (new UndoScope(target, "Add Blackboard Key"))
            {
                // No need for checkParentConflict since we already ensured uniqueness above
                BlackboardEditorHelper.SetValue(blackboard, uniqueName, valueType, GetDefaultValue(valueType), checkParentConflict: false);
                serializedObject.Update();
            }

            RefreshEntries();

            var newEntry = m_EntriesContainer.Children().LastOrDefault();
            if (newEntry != null)
            {
                var keyField = newEntry.Q<TextField>();
                if (keyField != null)
                {
                    keyField.schedule.Execute(() =>
                    {
                        keyField.Focus();
                        keyField.SelectAll();
                    }).ExecuteLater(100);
                }
            }
        }

        void DeleteKey(string keyName)
        {
            if (!EditorUtility.DisplayDialog("Delete Key", $"Delete key '{keyName}'?", "Delete", "Cancel"))
                return;

            var blackboard = target as BlackboardAsset;

            using (new UndoScope(target, "Delete Blackboard Key"))
            {
                var valuesProp = serializedObject.FindProperty("m_Values");
                for (int i = 0; i < valuesProp.arraySize; i++)
                {
                    var valueProp = valuesProp.GetArrayElementAtIndex(i);
                    var wrapper = valueProp.managedReferenceValue as BlackboardValue;

                    if (wrapper != null && wrapper.Key == keyName)
                    {
                        valuesProp.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }
            }

            serializedObject.Update();
            ForceReinitializeBlackboard(blackboard);
            RefreshEntries();
        }

        void ForceReinitializeBlackboard(BlackboardAsset blackboard)
        {
            BlackboardEditorHelper.ForceReinitialize(blackboard);
        }

        void RenameKey(string oldName, string newName, Type valueType)
        {
            var blackboard = target as BlackboardAsset;

            var valuesProp = serializedObject.FindProperty("m_Values");
            for (int i = 0; i < valuesProp.arraySize; i++)
            {
                var valueProp = valuesProp.GetArrayElementAtIndex(i);
                var wrapper = valueProp.managedReferenceValue as BlackboardValue;

                if (wrapper != null && wrapper.Key == oldName)
                {
                    var existingValue = wrapper.GetValue();
                    var newWrapper = BlackboardValuesFactory.CreateEntry(newName, valueType);
                    newWrapper.SetValue(existingValue);
                    valueProp.managedReferenceValue = newWrapper;

                    serializedObject.ApplyModifiedProperties();

                    blackboard.SyncAndNotifyKey(newName);

                    EditorUtility.SetDirty(blackboard);
                    break;
                }
            }
        }

        bool WouldCreateCycle(BlackboardAsset child, BlackboardAsset potentialParent)
        {
            var current = potentialParent;
            while (current != null)
            {
                if (current == child)
                    return true;
                current = current.m_ParentBlackboard;
            }
            return false;
        }

        internal static object GetDefaultValue(Type type)
        {
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(Vector3)) return Vector3.zero;
            if (type == typeof(List<int>)) return new List<int>();
            if (type == typeof(List<float>)) return new List<float>();
            if (type == typeof(List<bool>)) return new List<bool>();
            if (type == typeof(List<string>)) return new List<string>();
            if (type == typeof(List<Vector3>)) return new List<Vector3>();
            if (type == typeof(List<GameObject>)) return new List<GameObject>();
            if (type == typeof(List<Transform>)) return new List<Transform>();
            return null;
        }
    }
}
