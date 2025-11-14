using System;

namespace Capabilities
{
    /// <summary>
    /// 发起者
    /// </summary>
    public struct Instigator : IEquatable<Instigator>
    {
        public object reference;

        public Instigator(object reference)
        {
            this.reference = reference;
        }

        public bool Equals(Instigator other)
        {
            return Equals(reference, other.reference);
        }

        public override bool Equals(object obj)
        {
            return obj is Instigator other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (reference != null ? reference.GetHashCode() : 0);
        }
        //
        // // == 和 != 运算符重载
        // public static bool operator ==(Instigator left, Instigator right)
        // {
        //     return left.Equals(right);
        // }
        //
        // public static bool operator !=(Instigator left, Instigator right)
        // {
        //     return !left.Equals(right);
        // }


        // 隐式转换 从object -> Instigator

        public static Instigator FromObject(object reference)
        {
            return new Instigator(reference);
        }
    }
}