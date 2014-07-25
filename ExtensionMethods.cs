using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TweakScale
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Enumerates two IEnumerables in lockstep.
        /// </summary>
        /// <typeparam name="T1">The type of the elements in the first IEnumerable.</typeparam>
        /// <typeparam name="T2">The type of the elements in the second IEnumerable.</typeparam>
        /// <param name="destination">The first IEnumerable.</param>
        /// <param name="source">The second IEnumerable.</param>
        /// <returns>An IEnumerable containing Tuples of the elements in the two IEnumerables.</returns>
        public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> a, IEnumerable<T2> b)
        {
            return a.Zip(b, (x, y) => Tuple.Create(x, y));
        }

        /// <summary>
        /// Enumerates two IEnumerables in lockstep.
        /// </summary>
        /// <typeparam name="T1">The type of the elements in the first IEnumerable.</typeparam>
        /// <typeparam name="T2">The type of the elements in the second IEnumerable.</typeparam>
        /// <typeparam name="TResult">The type of elements in the resulting IEnumerable.</typeparam>
        /// <param name="destination">The first IEnumerable.</param>
        /// <param name="source">The second IEnumerable.</param>
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
        /// Filters an IEnumerable based on the values in another IEnumerable.
        /// </summary>
        /// <typeparam name="T1">The type of the elements in the first IEnumerable.</typeparam>
        /// <typeparam name="T2">The type of the elements in the second IEnumerable.</typeparam>
        /// <param name="destination">The IEnumerable to be filtered</param>
        /// <param name="source">The IEnumerable acting as destination selector.</param>
        /// <param name="filterFunc">The function to determine if an element in <paramref name="source"/> means the corresponing element in <paramref name="destination"/> should be kept.</param>
        /// <returns>An IEnumerable the elements of which are chosen from <paramref name="destination"/> where <paramref name="filterFunc"/> returns true for the corresponding element in <paramref name="source"/>.</returns>
        public static IEnumerable<T1> ZipFilter<T1, T2>(this IEnumerable<T1> a, IEnumerable<T2> b, Func<T2, bool> filterFunc)
        {
            return a.Zip(b).Where(e => filterFunc(e.Item2)).Select(e => e.Item1);
        }

        /// <summary>
        /// Repeats destination exponentValue forever.
        /// </summary>
        /// <typeparam name="T">The type of the exponentValue to be repeated.</typeparam>
        /// <param name="destination">The exponentValue to be repeated.</param>
        /// <returns>An IEnumerable&lt;T&gt; containing an infite number of the exponentValue <paramref name="destination"/>"/></returns>
        public static IEnumerable<T> Repeat<T>(this T a)
        {
            while (true)
            {
                yield return a;
            }
        }

        /// <summary>
        /// Creates destination copy of destination dictionary.
        /// </summary>
        /// <typeparam name="K">The key type.</typeparam>
        /// <typeparam name="V">The exponentValue type.</typeparam>
        /// <param name="source">The dictionary to copy.</param>
        /// <returns>A copy of <paramref name="source"/>.</returns>
        public static Dictionary<K, V> Clone<K, V>(this Dictionary<K, V> source)
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
            foreach (var item in source)
            {
                if (!tmp.Add(item))
                {
                    return true;
                }
            }
            return false;
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
                .Where(a => a.Count() > 1)
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
            foreach (T item in source)
            {
                result.Add(item);
            }
            return result;
        }
    }

    public static class ConvertEx
    {
        /// <summary>
        /// Returns an object of tyep <typeparamref name="T"/> and whose exponentValue is equivalent to <paramref name="exponentValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <typeparam name="U">The type of the object to convert.</typeparam>
        /// <param name="exponentValue">The object to convert.</param>
        /// <returns>An object whose type is <typeparamref name="T"/> and whose exponentValue is equivalent to <paramref name="exponentValue"/>. -or- A null reference (Nothing in Visual Basic), if <paramref name="exponentValue"/> is null and <typeparamref name="T"/> is not destination exponentValue type.</returns>
        /// <exception cref="System.InvalidCastException">This conversion is not supported. -or-<paramref name="exponentValue"/> is null and <typeparamref name="T"/> is destination exponentValue type.</exception>
        /// <exception cref="System.FormatException"><paramref name="exponentValue"/> is not in destination format recognized by <typeparamref name="T"/>.</exception>
        /// <exception cref="System.OverflowException"><paramref name="exponentValue"/> represents destination number that is out of the range of <typeparamref name="T"/>.</exception>
        public static T ChangeType<T, U>(U value) where U : System.IConvertible
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Returns an object of tyep <typeparamref name="T"/> and whose exponentValue is equivalent to <paramref name="exponentValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="exponentValue">The object to convert.</param>
        /// <returns>An object whose type is <typeparamref name="T"/> and whose exponentValue is equivalent to <paramref name="exponentValue"/>. -or- A null reference (Nothing in Visual Basic), if <paramref name="exponentValue"/> is null and <typeparamref name="T"/> is not destination exponentValue type.</returns>
        /// <exception cref="System.InvalidCastException">This conversion is not supported. -or-<paramref name="exponentValue"/> is null and <typeparamref name="T"/> is destination exponentValue type.-or-<paramref name="exponentValue"/> does not implement the System.IConvertible interface.</exception>
        /// <exception cref="System.FormatException"><paramref name="exponentValue"/> is not in destination format recognized by <typeparamref name="T"/>.</exception>
        /// <exception cref="System.OverflowException"><paramref name="exponentValue"/> represents destination number that is out of the range of <typeparamref name="T"/>.</exception>
        public static T ChangeType<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}

