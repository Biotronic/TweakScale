using System;
using System.Collections.Generic;

namespace TweakScale
{
    internal abstract class Enums<T> where T : class
    {
        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
        /// </summary>
        /// <typeparam name="TEnumType">The type of the enumeration.</typeparam>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <exception cref="System.ArgumentNullException">value is null</exception>
        /// <exception cref="System.ArgumentException">value is either an empty string or only contains white space.-or- value is a name, but not one of the named constants defined for the enumeration.</exception>
        /// <exception cref="System.OverflowException">value is outside the range of the underlying type of EnumType</exception>
        /// <returns>An object of type enumType whose value is represented by value.</returns>
        static public TEnumType Parse<TEnumType>(string value) where TEnumType : T
        {
            return (TEnumType)Enum.Parse(typeof(TEnumType), value);
        }
    }

    abstract class Enums : Enums<Enum>
    {
    }

    static partial class ExtensionMethods
    {
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> list)
        {
            var enumerator = list.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;
            var curr = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return curr;
                curr = enumerator.Current;
            }
        }

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> list, int n)
        {
            var enumerator = list.GetEnumerator();
            var buffer = new T[n];
            var idx = 0;
            while (enumerator.MoveNext() && idx < n)
            {
                buffer[idx] = enumerator.Current;
                idx++;
            }
            idx = 0;
            do
            {
                yield return buffer[idx];
                buffer[idx] = enumerator.Current;
                idx++;
                if (idx >= n)
                {
                    idx = 0;
                }
            }
            while (enumerator.MoveNext());
        }
    }
}
