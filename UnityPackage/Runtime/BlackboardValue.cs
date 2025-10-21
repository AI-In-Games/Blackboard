using System;
using UnityEngine;

namespace AiInGames.Blackboard
{
    [Serializable]
    public struct BlackboardValue
    {
        public enum ValueType : byte
        {
            None = 0,
            Int = 1,
            Float = 2,
            Bool = 3,
            Vector3 = 4,
            Object = 5
        }

        [SerializeField] private ValueType type;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private bool boolValue;
        [SerializeField] private Vector3 vector3Value;
        [SerializeField] private UnityEngine.Object objectValue;

        public ValueType Type => type;

        public static BlackboardValue FromInt(int value)
        {
            return new BlackboardValue
            {
                type = ValueType.Int,
                intValue = value
            };
        }

        public static BlackboardValue FromFloat(float value)
        {
            return new BlackboardValue
            {
                type = ValueType.Float,
                floatValue = value
            };
        }

        public static BlackboardValue FromBool(bool value)
        {
            return new BlackboardValue
            {
                type = ValueType.Bool,
                boolValue = value
            };
        }

        public static BlackboardValue FromVector3(Vector3 value)
        {
            return new BlackboardValue
            {
                type = ValueType.Vector3,
                vector3Value = value
            };
        }

        public static BlackboardValue FromObject(UnityEngine.Object value)
        {
            return new BlackboardValue
            {
                type = ValueType.Object,
                objectValue = value
            };
        }

        public int AsInt() => type == ValueType.Int ? intValue : default;
        public float AsFloat() => type == ValueType.Float ? floatValue : default;
        public bool AsBool() => type == ValueType.Bool ? boolValue : default;
        public Vector3 AsVector3() => type == ValueType.Vector3 ? vector3Value : default;
        public UnityEngine.Object AsObject() => type == ValueType.Object ? objectValue : null;

        public T As<T>()
        {
            return typeof(T) switch
            {
                Type t when t == typeof(int) => (T)(object)AsInt(),
                Type t when t == typeof(float) => (T)(object)AsFloat(),
                Type t when t == typeof(bool) => (T)(object)AsBool(),
                Type t when t == typeof(Vector3) => (T)(object)AsVector3(),
                Type t when typeof(UnityEngine.Object).IsAssignableFrom(t) => (T)(object)AsObject(),
                _ => default
            };
        }

        public bool TryGet<T>(out T value)
        {
            if (typeof(T) == typeof(int) && type == ValueType.Int)
            {
                value = (T)(object)intValue;
                return true;
            }
            if (typeof(T) == typeof(float) && type == ValueType.Float)
            {
                value = (T)(object)floatValue;
                return true;
            }
            if (typeof(T) == typeof(bool) && type == ValueType.Bool)
            {
                value = (T)(object)boolValue;
                return true;
            }
            if (typeof(T) == typeof(Vector3) && type == ValueType.Vector3)
            {
                value = (T)(object)vector3Value;
                return true;
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)) && type == ValueType.Object)
            {
                value = (T)(object)objectValue;
                return true;
            }

            value = default;
            return false;
        }
    }
}
