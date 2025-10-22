using System;
using System.Collections.Generic;

namespace AiInGames.Blackboard
{
    public interface IReadOnlyBlackboard
    {
        IReadOnlyBlackboard Parent { get; }
        bool TryGetValue<T>(string key, out T value);
        T GetValue<T>(string key, T defaultValue = default);
        bool HasKey<T>(string key);
        void Subscribe(string key, Action callback);
        void Unsubscribe(string key, Action callback);
        event Action<string> OnAnyValueChanged;
        IEnumerable<(string key, Type type, object value)> GetAllEntries();
    }
}
