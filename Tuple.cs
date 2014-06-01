using System;
using System.Collections;
using System.Collections.Generic;

namespace TweakScale
{
    static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 a, T2 b)
        {
            return new Tuple<T1, T2>(a, b);
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return (h1 << 5) + h1 ^ h2;
        }
    }

    public class Tuple<T1, T2> : IComparable
    {
        public T1 Item1
        {
            get;
            set;
        }

        public T2 Item2
        {
            get;
            set;
        }

        public Tuple(T1 a, T2 b)
        {
            Item1 = a;
            Item2 = b;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj, Comparer<object>.Default);
        }

        public bool Equals(object obj, IEqualityComparer cmp)
        {
            if (obj == null)
            {
                return false;
            }
            var other = obj as Tuple<T1, T2>;
            if (other == null)
            {
                return false;
            }
            return cmp.Equals(Item1, other.Item1) && cmp.Equals(Item2, other.Item2);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        public int GetHashCode(IEqualityComparer cmp)
        {
            return Tuple.CombineHashCodes(cmp.GetHashCode(Item1), cmp.GetHashCode(Item2));
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj, Comparer<object>.Default);
        }

        public int CompareTo(object obj, IComparer cmp)
        {
            if (obj == null)
            {
                return 1;
            }
            var other = obj as Tuple<T1, T2>;
            if (other == null)
            {
                throw new ArgumentException("Invalid comparison: " + this.GetType().FullName + " vs " + obj.GetType().FullName);
            }
            var result = cmp.Compare(Item1, other.Item1);
            if (result != 0)
            {
                return result;
            }
            result = cmp.Compare(Item2, other.Item2);
            return result;
        }
    }
}
