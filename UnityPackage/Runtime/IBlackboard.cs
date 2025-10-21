namespace AiInGames.Blackboard
{
    public interface IBlackboard : IReadOnlyBlackboard
    {
        void SetValue<T>(string key, T value);
        bool Remove<T>(string key);
        void ClearAll();
    }
}
