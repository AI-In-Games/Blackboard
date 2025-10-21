namespace AiInGames.Blackboard
{
    public interface IReadOnlyBlackboard
    {
        bool TryGetValue<T>(string key, out T value);
        T GetValue<T>(string key, T defaultValue = default);
        bool HasKey<T>(string key);
        int KeyCount();
        void Subscribe(string key, System.Action callback);
        void Unsubscribe(string key, System.Action callback);
    }
}
