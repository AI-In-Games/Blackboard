using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AiInGames.Blackboard.Editor
{
    public class BlackboardDebugWindow : EditorWindow
    {
        BlackboardWindowViewModel m_ViewModel;
        VisualElement m_EntriesContainer;
        Label m_StatusLabel;

        public static BlackboardDebugWindow ShowWindow(IBlackboard blackboard)
        {
            var blackboardName = (blackboard as UnityEngine.Object)?.name ?? "Unknown";
            var window = GetWindow<BlackboardDebugWindow>($"Blackboard: {blackboardName}");
            window.Show();
            window.EnsureViewModelInitialized();
            window.m_ViewModel.SetTarget(blackboard);
            return window;
        }

        public void Refresh()
        {
            RefreshEntries();
        }

        void CreateGUI()
        {
            EnsureViewModelInitialized();

            var root = rootVisualElement;

            var stylesheet = Resources.Load<StyleSheet>("Styles/BlackboardEditor");
            if (stylesheet != null)
            {
                root.styleSheets.Add(stylesheet);
            }

            CreateHeader(root);
            CreateToolbar(root);
            CreateEntriesContainer(root);
        }

        void OnDestroy()
        {
            if (m_ViewModel != null)
            {
                m_ViewModel.OnDataChanged -= RefreshEntries;
            }
        }

        void EnsureViewModelInitialized()
        {
            if (m_ViewModel != null) return;

            m_ViewModel = new BlackboardWindowViewModel();
            m_ViewModel.OnDataChanged += RefreshEntries;
        }

        void CreateHeader(VisualElement root)
        {
            var header = new VisualElement();
            header.AddToClassList("debug-window-header");

            var title = new Label("Runtime Blackboard Inspector");
            title.AddToClassList("debug-window-title");

            m_StatusLabel = new Label();
            m_StatusLabel.AddToClassList("debug-window-status");

            UpdateStatusLabel();

            header.Add(title);
            header.Add(m_StatusLabel);
            root.Add(header);
        }

        void UpdateStatusLabel()
        {
            if (m_StatusLabel == null || m_ViewModel == null) return;

            m_StatusLabel.ClearClassList();
            m_StatusLabel.AddToClassList("debug-window-status");

            if (m_ViewModel.TargetBlackboard != null)
            {
                m_StatusLabel.text = "Blackboard selected";
                m_StatusLabel.AddToClassList("debug-window-status-active");
            }
            else
            {
                m_StatusLabel.text = "No blackboard selected";
                m_StatusLabel.AddToClassList("debug-window-status-inactive");
            }
        }

        void CreateToolbar(VisualElement root)
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("debug-window-toolbar");

            var refreshButton = new Button(RefreshEntries) { text = "Refresh" };
            refreshButton.AddToClassList("toolbar-button");
            toolbar.Add(refreshButton);

            var spacer = new VisualElement();
            spacer.AddToClassList("toolbar-spacer");
            toolbar.Add(spacer);

            root.Add(toolbar);
        }

        void CreateEntriesContainer(VisualElement root)
        {
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;

            m_EntriesContainer = new VisualElement();
            m_EntriesContainer.AddToClassList("entries-container");

            scrollView.Add(m_EntriesContainer);
            root.Add(scrollView);
        }

        void RefreshEntries()
        {
            if (m_EntriesContainer == null || m_ViewModel == null) return;

            m_EntriesContainer.Clear();
            UpdateStatusLabel();

            if (m_ViewModel.TargetBlackboard == null)
            {
                var helpBox = new HelpBox("No blackboard selected.", HelpBoxMessageType.Info);
                m_EntriesContainer.Add(helpBox);
                return;
            }

            var entries = m_ViewModel.GetEntries();

            if (entries.Count == 0)
            {
                var emptyLabel = new Label("Blackboard is empty");
                emptyLabel.AddToClassList("empty-message");
                m_EntriesContainer.Add(emptyLabel);
                return;
            }

            var countLabel = new Label($"Entries: {entries.Count}");
            countLabel.AddToClassList("entry-count-label");
            m_EntriesContainer.Add(countLabel);

            foreach (var entry in entries)
            {
                var entryElement = CreateEditableRuntimeEntry(entry);
                m_EntriesContainer.Add(entryElement);
            }
        }

        VisualElement CreateEditableRuntimeEntry(BlackboardEntryViewModel entry)
        {
            var container = new VisualElement();
            container.AddToClassList("blackboard-entry");

            var foldout = new Foldout
            {
                text = string.IsNullOrEmpty(entry.Name) ? "Unnamed Key" : entry.Name,
                value = true
            };

            var contentContainer = new VisualElement();
            contentContainer.style.paddingLeft = 15;

            var typeLabel = new Label($"Type: {BlackboardEditorHelper.GetDisplayName(entry.Type)}");
            typeLabel.AddToClassList("type-label");
            contentContainer.Add(typeLabel);

            var valueField = BlackboardValueRenderer.CreateValueField(
                entry.Type,
                entry.Value,
                readOnly: false,
                onValueChanged: (newValue) => m_ViewModel.SetValue(entry.Name, entry.Type, newValue));

            valueField.AddToClassList("value-field");
            contentContainer.Add(valueField);

            foldout.Add(contentContainer);
            container.Add(foldout);

            return container;
        }
    }
}
