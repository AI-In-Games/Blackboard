using System;
using System.Collections.Generic;

namespace AiInGames.Blackboard.Editor
{
    internal class BlackboardWindowViewModel
    {
        public IBlackboard TargetBlackboard { get; private set; }

        public event Action OnDataChanged;

        public void SetTarget(IBlackboard blackboard)
        {
            TargetBlackboard = blackboard;
            NotifyDataChanged();
        }

        public IReadOnlyList<BlackboardEntryViewModel> GetEntries()
        {
            if (TargetBlackboard == null)
                return Array.Empty<BlackboardEntryViewModel>();

            var entries = new List<BlackboardEntryViewModel>();

            if (TargetBlackboard is BlackboardAsset blackboardAsset)
            {
                foreach (var entry in BlackboardEditorHelper.GetAllEntries(blackboardAsset, includeInherited: false))
                {
                    entries.Add(new BlackboardEntryViewModel
                    {
                        Name = entry.name,
                        Type = entry.type,
                        Value = entry.value
                    });
                }
            }
            else if (TargetBlackboard is Blackboard runtimeBlackboard)
            {
                foreach (var entry in runtimeBlackboard.GetAllEntries())
                {
                    entries.Add(new BlackboardEntryViewModel
                    {
                        Name = entry.key,
                        Type = entry.type,
                        Value = entry.value
                    });
                }
            }

            return entries;
        }

        public void SetValue(string keyName, Type valueType, object newValue)
        {
            if (TargetBlackboard == null) return;

            var blackboardAsset = TargetBlackboard as BlackboardAsset;
            if (blackboardAsset != null)
            {
                using (new UndoScope(blackboardAsset, "Modify Blackboard Value"))
                {
                    BlackboardEditorHelper.SetValue(blackboardAsset, keyName, valueType, newValue);
                }
            }
            else if (TargetBlackboard is Blackboard runtimeBlackboard)
            {
                runtimeBlackboard.SetValue(keyName, valueType, newValue);
            }

            NotifyDataChanged();
        }

        void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }
    }

    internal class BlackboardEntryViewModel
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }
    }
}
