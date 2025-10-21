using System;
using System.Collections.Generic;
using System.Linq;

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

            var blackboard = TargetBlackboard as Blackboard;
            if (blackboard == null)
                return Array.Empty<BlackboardEntryViewModel>();

            return BlackboardEditorHelper.GetAllEntries(blackboard, includeInherited: true)
                .Select(e => new BlackboardEntryViewModel
                {
                    Name = e.name,
                    Type = e.type,
                    Value = e.value
                })
                .ToList();
        }

        public void SetValue(string keyName, Type valueType, object newValue)
        {
            if (TargetBlackboard == null) return;

            var blackboard = TargetBlackboard as Blackboard;
            if (blackboard != null)
            {
                BlackboardEditorHelper.SetValue(blackboard, keyName, valueType, newValue);
            }

            NotifyDataChanged();
        }

        public string GetStatusMessage()
        {
            if (TargetBlackboard != null)
                return "Blackboard selected";

            return "No blackboard selected";
        }

        public StatusType GetStatusType()
        {
            if (TargetBlackboard != null)
                return StatusType.Active;

            return StatusType.Inactive;
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

    internal enum StatusType
    {
        Active,
        Inactive
    }
}
