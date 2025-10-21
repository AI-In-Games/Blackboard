using System;
using UnityEditor;

namespace AiInGames.Blackboard.Editor
{
    internal readonly struct UndoScope : IDisposable
    {
        private readonly UnityEngine.Object target;

        public UndoScope(UnityEngine.Object target, string operationName)
        {
            this.target = target;
            Undo.RecordObject(target, operationName);
        }

        public void Dispose()
        {
            if (target != null)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
