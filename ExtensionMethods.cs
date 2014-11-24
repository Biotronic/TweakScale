using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TweakScale
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Enumerates two IEnumerables in lockstep.
        /// </summary>
        /// <typeparam name="T1">The type of the elements in the first IEnumerable.</typeparam>
        /// <typeparam name="T2">The type of the elements in the second IEnumerable.</typeparam>
        /// <param name="a">The first IEnumerable.</param>
        /// <param name="b">The second IEnumerable.</param>
        /// <returns>An IEnumerable containing Tuples of the elements in the two IEnumerables.</returns>
        public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> a, IEnumerable<T2> b)
        {
            return a.Zip(b, Tuple.Create);
        }

        /// <summary>
        /// Enumerates two IEnumerables in lockstep.
        /// </summary>
        /// <typeparam name="T1">The type of the elements in the first IEnumerable.</typeparam>
        /// <typeparam name="T2">The type of the elements in the second IEnumerable.</typeparam>
        /// <typeparam name="TResult">The type of elements in the resulting IEnumerable.</typeparam>
        /// <param name="a">The first IEnumerable.</param>
        /// <param name="b">The second IEnumerable.</param>
        /// <param name="fn">The function that creates the elements that will be in the result.</param>
        /// <returns></returns>
        public static IEnumerable<TResult> Zip<T1, T2, TResult>(this IEnumerable<T1> a, IEnumerable<T2> b, Func<T1, T2, TResult> fn)
        {
            var v1 = a.GetEnumerator();
            var v2 = b.GetEnumerator();

            while (v1.MoveNext() && v2.MoveNext())
            {
                yield return fn(v1.Current, v2.Current);
            }
        }

        /// <summary>
        /// Enumerates two IEnumerables in lockstep.
        /// </summary>
        /// <param name="a">The first IEnumerable.</param>
        /// <param name="b">The second IEnumerable.</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<object, object>> Zip(this IEnumerable a, IEnumerable b)
        {
            return a.Zip(b, Tuple.Create);
        }

        /// <summary>
        /// Enumerates two IEnumerables in lockstep.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="a">The first IEnumerable.</param>
        /// <param name="b">The second IEnumerable.</param>
        /// <param name="fn">The function that creates the elements that will be in the result.</param>
        /// <returns></returns>
        public static IEnumerable<TResult> Zip<TResult>(this IEnumerable a, IEnumerable b, Func<object, object, TResult> fn)
        {
            var v1 = a.GetEnumerator();
            var v2 = b.GetEnumerator();

            while (v1.MoveNext() && v2.MoveNext())
            {
                yield return fn(v1.Current, v2.Current);
            }
        }

        /// <summary>
        /// Because there is no IEnumerable.Count.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static int StupidCount(this IEnumerable a)
        {
            return a.Cast<object>().Count();
        }

        /// <summary>
        /// Looks up a non-public field by its type.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="t">The type on which to perform the lookup.</param>
        /// <returns>The first field of the given type, or null.</returns>
        public static FieldInfo GetNonPublicFieldByType<T>(this Type t)
        {
            var f = t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            return f.FirstOrDefault(e => e.FieldType == typeof (T));
        }
        
        /// <summary>
        /// Checks if a method is overridden.
        /// </summary>
        /// <param name="m">The method to check.</param>
        /// <returns>True if the method is an override, else false.</returns>
        public static bool IsOverride(this MethodInfo m)
        {
            return m.GetBaseDefinition() == m;
        }

        /// <summary>
        /// Filters an IEnumerable based on the values in another IEnumerable.
        /// </summary>
        /// <typeparam name="T1">The type of the elements in the first IEnumerable.</typeparam>
        /// <typeparam name="T2">The type of the elements in the second IEnumerable.</typeparam>
        /// <param name="a">The IEnumerable to be filtered</param>
        /// <param name="b">The IEnumerable acting as destination selector.</param>
        /// <param name="filterFunc">The function to determine if an element in <paramref name="b"/> means the corresponing element in <paramref name="a"/> should be kept.</param>
        /// <returns>An IEnumerable the elements of which are chosen from <paramref name="a"/> where <paramref name="filterFunc"/> returns true for the corresponding element in <paramref name="b"/>.</returns>
        public static IEnumerable<T1> ZipFilter<T1, T2>(this IEnumerable<T1> a, IEnumerable<T2> b, Func<T2, bool> filterFunc)
        {
            return a.Zip(b).Where(e => filterFunc(e.Item2)).Select(e => e.Item1);
        }

        /// <summary>
        /// Repeats destination exponentValue forever.
        /// </summary>
        /// <typeparam name="T">The type of the exponentValue to be repeated.</typeparam>
        /// <param name="a">The exponentValue to be repeated.</param>
        /// <returns>An IEnumerable&lt;T&gt; containing an infite number of the exponentValue <paramref name="a"/>"/></returns>
        public static IEnumerable<T> Repeat<T>(this T a)
        {
            while (true)
            {
                yield return a;
            }
// ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        /// Creates destination copy of destination dictionary.
        /// </summary>
        /// <typeparam name="TK">The key type.</typeparam>
        /// <typeparam name="TV">The exponentValue type.</typeparam>
        /// <param name="source">The dictionary to copy.</param>
        /// <returns>A copy of <paramref name="source"/>.</returns>
        public static Dictionary<TK, TV> Clone<TK, TV>(this Dictionary<TK, TV> source)
        {
            return source.AsEnumerable().ToDictionary(a => a.Key, a => a.Value);
        }

        /// <summary>
        /// Checks if <paramref name="source"/> contains duplicate exponentValue.
        /// </summary>
        /// <typeparam name="T">The type of elements in <paramref name="source"/>.</typeparam>
        /// <param name="source">The list of values to check for duplicates.</param>
        /// <returns>True if <paramref name="source"/> contains duplicates, otherwise false.</returns>
        public static bool ContainsDuplicates<T>(this IEnumerable<T> source)
        {
            var tmp = new HashSet<T>();
            return source.Any(item => !tmp.Add(item));
        }

        /// <summary>
        /// Returns the duplicate values in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of values in <paramref name="source"/>.</typeparam>
        /// <param name="source">The list to check for duplicates.</param>
        /// <returns>A list of each duplicate exponentValue in <paramref name="source"/>, but only one of each modExp.</returns>
        public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> source)
        {
            return source
                .GroupBy(a => a)
                .Where(a => a.StupidCount() > 1)
                .Select(a => a.Key);
        }

        /// <summary>
        /// Creates a HashSet containing all the items in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of items in the HashSet</typeparam>
        /// <param name="source">The items to add to the HashSet.</param>
        /// <returns>A HashSet contaiting all the items in <paramref name="source"/>.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            var result = new HashSet<T>();
            foreach (var item in source)
            {
                result.Add(item);
            }
            return result;
        }

        public static T Match<T, TNullable>(this TNullable? value, Func<TNullable, T> hasValue, Func<T> noValue) where TNullable : struct
        {
            if (value.HasValue)
                return hasValue(value.Value);
            return noValue();
        }

        public static void Match<TNullable>(this TNullable? value, Action<TNullable> hasValue, Action noValue) where TNullable : struct
        {
            if (value.HasValue)
                hasValue(value.Value);
            else
                noValue();
        }

        public static void Match<TNullable>(this TNullable? value, Action<TNullable> hasValue) where TNullable : struct
        {
            if (value.HasValue)
                hasValue(value.Value);
        }

        public static T? Match<T, TNullable>(this TNullable? value, Func<TNullable, T> hasValue) where TNullable : struct where T : struct
        {
            if (value.HasValue)
                return hasValue(value.Value);
            return null;
        }
    }

    public static class ConvertEx
    {
        /// <summary>
        /// Returns an object of tyep <typeparamref name="TTo"/> and whose exponentValue is equivalent to <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TTo">The type of object to return.</typeparam>
        /// <typeparam name="TFrom">The type of the object to convert.</typeparam>
        /// <param name="value">The object to convert.</param>
        /// <returns>An object whose type is <typeparamref name="TTo"/> and whose exponentValue is equivalent to <paramref name="value"/>. -or- A null reference (Nothing in Visual Basic), if <paramref name="value"/> is null and <typeparamref name="TTo"/> is not destination exponentValue type.</returns>
        /// <exception cref="System.InvalidCastException">This conversion is not supported. -or-<paramref name="value"/> is null and <typeparamref name="TTo"/> is destination exponentValue type.</exception>
        /// <exception cref="System.FormatException"><paramref name="value"/> is not in destination format recognized by <typeparamref name="TTo"/>.</exception>
        /// <exception cref="System.OverflowException"><paramref name="value"/> represents destination number that is out of the range of <typeparamref name="TTo"/>.</exception>
        public static TTo ChangeType<TTo, TFrom>(TFrom value) where TFrom : IConvertible
        {
            return (TTo)Convert.ChangeType(value, typeof(TTo));
        }

        /// <summary>
        /// Returns an object of tyep <typeparamref name="T"/> and whose exponentValue is equivalent to <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="value">The object to convert.</param>
        /// <returns>An object whose type is <typeparamref name="T"/> and whose exponentValue is equivalent to <paramref name="value"/>. -or- A null reference (Nothing in Visual Basic), if <paramref name="value"/> is null and <typeparamref name="T"/> is not destination exponentValue type.</returns>
        /// <exception cref="System.InvalidCastException">This conversion is not supported. -or-<paramref name="value"/> is null and <typeparamref name="T"/> is destination exponentValue type.-or-<paramref name="value"/> does not implement the System.IConvertible interface.</exception>
        /// <exception cref="System.FormatException"><paramref name="value"/> is not in destination format recognized by <typeparamref name="T"/>.</exception>
        /// <exception cref="System.OverflowException"><paramref name="value"/> represents destination number that is out of the range of <typeparamref name="T"/>.</exception>
        public static T ChangeType<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}

