using System;
using System.Collections.Generic;
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
        /// <param name="a">The first IEnumerable.</param>
        /// <param name="b">The second IEnumerable.</param>
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
        /// Filters an IEnumerable based on the values in another IEnumerable.
        /// </summary>
        /// <typeparam name="T1">The type of the elements in the first IEnumerable.</typeparam>
        /// <typeparam name="T2">The type of the elements in the second IEnumerable.</typeparam>
        /// <param name="a">The IEnumerable to be filtered</param>
        /// <param name="b">The IEnumerable acting as a selector.</param>
        /// <param name="filterFunc">The function to determine if an element in <paramref name="b"/> means the corresponing element in <paramref name="a"/> should be kept.</param>
        /// <returns>An IEnumerable the elements of which are chosen from <paramref name="a"/> where <paramref name="filterFunc"/> returns true for the corresponding element in <paramref name="b"/>.</returns>
        public static IEnumerable<T1> ZipFilter<T1, T2>(this IEnumerable<T1> a, IEnumerable<T2> b, Func<T2, bool> filterFunc)
        {
            return a.Zip(b).Where(e => filterFunc(e.Item2)).Select(e => e.Item1);
        }

        /// <summary>
        /// Repeats a value forever.
        /// </summary>
        /// <typeparam name="T">The type of the value to be repeated.</typeparam>
        /// <param name="a">The value to be repeated.</param>
        /// <returns>An IEnumerable&lt;T&gt; containing an infite number of the value <paramref name="a"/>"/></returns>
        public static IEnumerable<T> Repeat<T>(this T a)
        {
            while (true)
            {
                yield return a;
            }
        }
    }
}

