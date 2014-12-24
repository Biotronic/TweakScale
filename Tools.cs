using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TweakScale.Annotations;
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
        public static float Clamp(float x, float min, float max)
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
        /// Finds the index of the exponentValue in <paramref name="values"/> that's closest to <paramref name="x"/>.
        /// </summary>
        /// <param name="x">The exponentValue to find.</param>
        /// <param name="values">The values to look through.</param>
        /// <returns>The index of the exponentValue in <paramref name="values"/> that's closest to <paramref name="x"/>.</returns>
        public static int ClosestIndex(float x, IEnumerable<float> values)
        {
            var minDistance = float.PositiveInfinity;
            int result = 0;
            int idx = 0;
            foreach (var value in values)
            {
                var tmpDistance = Math.Abs(value - x);
                if (tmpDistance < minDistance)
                {
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
        [StringFormatMethod("format")]
        public static void Logf(string format, params object[] args)
        {
            Debug.Log("[TweakScale] " + string.Format(format, args.Select(a => a.PreFormat()).ToArray()) + Environment.NewLine + StackTraceUtility.ExtractStackTrace());
        }

        /// <summary>
        /// Writes destination log message to output_log.txt.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments to the format.</param>
        [StringFormatMethod("format")]
        public static void LogWf(string format, params object[] args)
        {
            Debug.LogWarning("[TweakScale Warning] " + string.Format(format, args.Select(a => a.PreFormat()).ToArray()) + Environment.NewLine + StackTraceUtility.ExtractStackTrace());
        }

        /// <summary>
        /// Formats certain types to make them more readable.
        /// </summary>
        /// <param name="obj">The object to format.</param>
        /// <returns>A more readable representation of <paramref name="obj"/>>.</returns>
        public static object PreFormat(this object obj)
        {
            if (obj == null)
            {
                return "null";
            }
            if (obj is IEnumerable)
            {
                if (obj.GetType().GetMethod("ToString", new Type[] { }).IsOverride())
                {
                    var e = obj as IEnumerable;
                    return string.Format("[{0}]", string.Join(", ", e.Cast<object>().Select(a => a.PreFormat().ToString()).ToArray()));
                }
            }
            return obj;
        }

        /// <summary>
        /// Reads destination exponentValue from the ConfigNode and magically converts it to the type you ask. Tested for float, boolean and double[]. Anything else is at your own risk.
        /// </summary>
        /// <typeparam name="T">The type to convert to. Usually inferred from <paramref name="defaultValue"/>.</typeparam>
        /// <param name="config">ScaleType node from which to read values.</param>
        /// <param name="name">Name of the ConfigNode's field.</param>
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
                    LogWf("Failed to convert string value \"{0}\" to type {1}", cfgValue, typeof(T).Name);
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
            return ConvertString(config.GetValue(name), defaultValue);
        }

        /// <summary>
        /// Converts destination comma-delimited string into an array of <typeparamref name="T"/>s.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">A comma-delimited list of values.</param>
        /// <param name="defaultValue">The exponentValue to return if the list does not hold valid values.</param>
        /// <returns>An arra</returns>
        public static T[] ConvertString<T>(string value, T[] defaultValue)
        {
            try
            {
                return value.Split(',').Select(ConvertEx.ChangeType<T>).ToArray();
            }
            catch (Exception ex)
            {
                if (!(ex is InvalidCastException) && !(ex is FormatException) && !(ex is OverflowException) &&
                    !(ex is ArgumentNullException))
                    throw;
                LogWf("Failed to convert string value \"{0}\" to type {1}", value, typeof(T).Name);
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets all types defined in all loaded assemblies.
        /// </summary>
        public static IEnumerable<Type> GetAllTypes()
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

        public static bool HasParent(this Part p)
        {
            return (object) p.parent != null;
        }

        public static string ToString_rec(this object obj, int depth = 0)
        {
            if (obj == null)
                return "(null)";

            var result = new StringBuilder("(");
            var tt = obj.GetType();

            Func<object, string> fmt = a => a == null ? "(null)" :  depth == 0 ? a.ToString() : a.ToString_rec();

            foreach (var field in tt.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                result.AppendFormat("{0}: {1}, ", field.Name, fmt(field.GetValue(obj)));
            }

            foreach (var field in tt.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    result.AppendFormat("{0}: {1}, ", field.Name, fmt(field.GetValue(obj, null)));
                }
                catch (Exception)
                {
                }
            }

            result.Append(")");

            return result.ToString();
        }
    }
}
