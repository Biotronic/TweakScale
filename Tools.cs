using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweakScale
{
    static class Tools
    {
        public static float clamp(float x, float min, float max)
        {
            return x < min ? min : x > max ? max : x;
        }

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

        public static void LogErrorMessageF(string format, params object[] args)
        {
            MonoBehaviour.print(string.Format(format, args));
        }

        /// <summary>
        /// Reads a value from the ConfigNode and magically converts it to the type you ask. Tested for float, boolean and double[]. Anything else is at your own risk.
        /// </summary>
        /// <typeparam name="T">The type to convert to. Usually inferred from <paramref name="defaultValue"/>.</typeparam>
        /// <param name="name">Name of the ConfigNode's field</param>
        /// <param name="defaultValue">The value to use when the ConfigNode doesn't contain what we want.</param>
        /// <returns>The value in the ConfigNode, or <paramref name="defaultValue"/> if no decent value is found there.</returns>
        public static T ConfigValue<T>(ConfigNode config, string name, T defaultValue)
        {
            if (!config.HasValue(name))
            {
                return defaultValue;
            }
            string cfgValue = config.GetValue(name);
            try
            {
                var result = (T)Convert.ChangeType(cfgValue, typeof(T));
                return result;
            }
            catch (InvalidCastException)
            {
                LogErrorMessageF("Failed to convert string value \"{0}\" to type {1}", cfgValue, typeof(T).Name);
                return defaultValue;
            }
        }

        public static T[] ConfigValue<T>(ConfigNode config, string name, T[] defaultValue)
        {
            if (!config.HasValue(name))
            {
                return defaultValue;
            }
            string cfgValue = config.GetValue(name);
            try
            {
                return cfgValue.Split(',').Select(a => (T)Convert.ChangeType(a, typeof(T))).ToArray();
            }
            catch (InvalidCastException)
            {
                LogErrorMessageF("Failed to convert string value \"{0}\" to type {1}", cfgValue, typeof(T).Name);
                return defaultValue;
            }
        }

        public static T[] ConvertString<T>(string value, T[] defaultValue)
        {
            try
            {
                return value.Split(',').Select(a => (T)Convert.ChangeType(a, typeof(T))).ToArray();
            }
            catch (InvalidCastException)
            {
                LogErrorMessageF("Failed to convert string value \"{0}\" to type {1}", value, typeof(T[]).Name);
                return defaultValue;
            }
        }
    }
}
