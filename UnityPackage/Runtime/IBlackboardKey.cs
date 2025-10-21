using System;

namespace AiInGames.Blackboard
{
    /// <summary>
    /// Interface for blackboard keys.
    /// Allows different key implementations while maintaining type safety.
    /// </summary>
    public interface IBlackboardKey<T>
    {
        int Hash { get; }
        string Name { get; }
        Type ValueType { get; }
    }
}
