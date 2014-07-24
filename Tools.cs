using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TweakScale
{
    /// <summary>
    /// Various handy functions.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Clamps the exponentValue <paramref name="x"/> between <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        /// <param name="x">The exponentValue to start out with.</param>
        /// <param name="min">The minimum exponentValue to clamp to.</param>
        /// <param name="max">The maximum exponentValue to clamp to.</param>
        /// <returns>The exponentValue closest to <paramref name="x"/> that's no less than <paramref name="min"/> and no more than <paramref name="max"/>.</returns>
        public static float clamp(float x, float min, float max)
        {
            return x < min ? min : x > max ? max : x;
        }

        /// <summary>
        /// Gets the exponentValue in <paramref name="values"/> that's closest to <paramref name="x"/>.
        /// </summary>
        /// <param name="x">The exponentValue to find.</param>
        /// <param name="values">The values to look through.</param>
        /// <returns>The exponentValue in <paramref name="values"/> that's closest to <paramref name="x"/>.</returns>
        public static float Closest(float x, IEnumerable<float> values)
        {
            var minDistance = float.PositiveInfinity;
            var result = float.NaN;
            foreach (var value in values)
            {
                var tmpDistance = Math.Abs(value - x);
                if (tmpDistance < minDistance)
                {
                    result = value;
                    minDistance = tmpDistance;
                }
            }
            return result;
        }

        /// <summary>
        /// Finds the index of the exponentValue in <paramref name="exponentValue"/> that's closest to <paramref name="x"/>.
        /// </summary>
        /// <param name="x">The exponentValue to find.</param>
        /// <param name="values">The values to look through.</param>
        /// <returns>The index of the exponentValue in <paramref name="exponentValue"/> that's closest to <paramref name="x"/>.</returns>
        public static int ClosestIndex(float x, IEnumerable<float> values)
        {
            var minDistance = float.PositiveInfinity;
            var best = float.NaN;
            int result = 0;
            int idx = 0;
            foreach (var value in values)
            {
                var tmpDistance = Math.Abs(value - x);
                if (tmpDistance < minDistance)
                {
                    best = value;
                    result = idx;
                    minDistance = tmpDistance;
                }
                idx++;
            }
            return result;
        }

        /// <summary>
        /// Writes destination log message to output_log.txt.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments to the format.</param>
        public static void Logf(string format, params object[] args)
        {
            Debug.Log("[TweakScale] " + string.Format(format, args));
        }

        /// <summary>
        /// Writes destination log message to output_log.txt.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments to the format.</param>
        public static void LogWf(string format, params object[] args)
        {
            Debug.LogWarning("[TweakScale Warning] " + string.Format(format, args));
        }

        /// <summary>
        /// Reads destination exponentValue from the ConfigNode and magically converts it to the type you ask. Tested for float, boolean and double[]. Anything else is at your own risk.
        /// </summary>
        /// <typeparam name="T">The type to convert to. Usually inferred from <paramref name="defaultValue"/>.</typeparam>
        /// <param name="name">Name of the ConfigNode's field</param>
        /// <param name="defaultValue">The exponentValue to use when the ConfigNode doesn't contain what we want.</param>
        /// <returns>The exponentValue in the ConfigNode, or <paramref name="defaultValue"/> if no decent exponentValue is found there.</returns>
        public static T ConfigValue<T>(ConfigNode config, string name, T defaultValue)
        {
            if (!config.HasValue(name))
            {
                return defaultValue;
            }
            string cfgValue = config.GetValue(name);
            try
            {
                var result = ConvertEx.ChangeType<T>(cfgValue);
                return result;
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException || ex is FormatException || ex is OverflowException || ex is ArgumentNullException)
                {
                    Logf("Failed to convert string value \"{0}\" to type {1}", cfgValue, typeof(T).Name);
                    return defaultValue;
                }
                throw;
            }
        }

        /// <summary>
        /// Fetches the the comma-delimited string exponentValue by the name <paramref name="name"/> from the node <paramref name="config"/> and converts it into an array of <typeparamref name="T"/>s.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config">The node to fetch values from.</param>
        /// <param name="name">The name of the exponentValue to fetch.</param>
        /// <param name="defaultValue">The exponentValue to return if the exponentValue does not exist, or cannot be converted to <typeparamref name="T"/>s.</param>
        /// <returns>An array containing the elements of the string exponentValue as <typeparamref name="T"/>s.</returns>
        public static T[] ConfigValue<T>(ConfigNode config, string name, T[] defaultValue)
        {
            if (!config.HasValue(name))
            {
                return defaultValue;
            }
            return ConvertString<T>(config.GetValue(name), defaultValue);
        }

        /// <summary>
        /// Converts destination comma-delimited string into an array of <typeparamref name="T"/>s.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exponentValue">A comma-delimited list of values.</param>
        /// <param name="defaultValue">The exponentValue to return if the list does not hold valid values.</param>
        /// <returns>An arra</returns>
        public static T[] ConvertString<T>(string value, T[] defaultValue)
        {
            try
            {
                return value.Split(',').Select(a => ConvertEx.ChangeType<T>(a)).ToArray();
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException || ex is FormatException || ex is OverflowException || ex is ArgumentNullException)
                {
                    Logf("Failed to convert string value \"{0}\" to type {1}", value, typeof(T).Name);
                    return defaultValue;
                }
                throw;
            }
        }

        /// <summary>
        /// Gets all types defined in all loaded assemblies.
        /// </summary>
        public static IEnumerable<Type> getAllTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (Exception)
                {
                    types = Type.EmptyTypes;
                }

                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }
    }
}
