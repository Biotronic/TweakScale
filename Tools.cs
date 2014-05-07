using System;
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

        public static float closest(float x, float[] values)
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
        /// Reads a value from the ConfigNode and magically converts it to the type you ask. Tested for float, boolean and double[]. Anything else is at your own risk.
        /// </summary>
        /// <typeparam name="T">The type to convert to. Usually inferred from <paramref name="defaultValue"/>.</typeparam>
        /// <param name="name">Name of the ConfigNode's field</param>
        /// <param name="defaultValue">The value to use when the ConfigNode doesn't contain what we want.</param>
        /// <returns>The value in the ConfigNode, or <paramref name="defaultValue"/> if no decent value is found there.</returns>
        public static T configValue<T>(ConfigNode config, string name, T defaultValue)
        {
            MonoBehaviour.print("Reading " + name + " from config");
            if (!config.HasValue(name))
            {
                MonoBehaviour.print("No " + name + " in config");
                return defaultValue;
            }
            string cfgValue = config.GetValue(name);
            try
            {
                var result = (T)Convert.ChangeType(cfgValue, typeof(T));
                MonoBehaviour.print("Result value: " + result.ToString());
                return result;
            }
            catch (InvalidCastException)
            {
                MonoBehaviour.print("Failed to convert string value \"" + cfgValue + "\" to type " + typeof(T).Name);
                return defaultValue;
            }
        }

        public static T[] configValue<T>(ConfigNode config, string name, T[] defaultValue)
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
                MonoBehaviour.print("Failed to convert string value \"" + cfgValue + "\" to type " + typeof(T[]).Name);
                return defaultValue;
            }
        }
    }
}
