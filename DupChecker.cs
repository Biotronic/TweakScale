using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TweakScale
{
    /// <summary>
    /// Checks if there are multiple 
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class DupChecker : MonoBehaviour
    {
        private static int _version = 2;
        void Start()
        {
            int[] fields =
                getAllTypes()
                .Where(t => t.Name == "DupChecker")
                .Select(t => t.GetField("_version", BindingFlags.Static | BindingFlags.NonPublic))
                .Where(f => f != null)
                .Where(f => f.FieldType == typeof(int))
                .Select(f => (int)f.GetValue(null))
                .ToArray();
            // Let the latest version of the checker execute.
            if (_version != fields.Max()) { return; }
            if (_version == int.MaxValue) { return; }

            Tools.Logf("Running DupChecker version {0} from '{1}'", _version, Tools.KSPRelativePath(Assembly.GetExecutingAssembly().CodeBase));

            // Other checkers will see this version and not run.
            // This accomplishes the same as an explicit "ran" flag with fewer moving parts.
            _version = int.MaxValue;

            var scales = AssemblyLoader
                .loadedAssemblies
                .Where(a => a.assembly.GetType("TweakScale.TweakScale") != null || a.assembly.GetType("GoodspeedTweakScale") != null)
                .ToArray();

            if (scales.Length <= 1)
                return; // How the **** did this run with no Scale.dll loaded?

            foreach (var scale in scales.Where(IsNotCanonicalTweakScale))
            {
                RemoveAssembly(scale);
            }
        }

        static bool IsNotCanonicalTweakScale(AssemblyLoader.LoadedAssembly asm)
        {
            return !Tools.KSPRelativePath(asm.assembly.CodeBase).StartsWith("GameData\\TweakScale", StringComparison.InvariantCultureIgnoreCase);
        }

        void RemoveAssembly(AssemblyLoader.LoadedAssembly asm)
        {
            for (int i = 0; i < AssemblyLoader.loadedAssemblies.Count; ++i)
            {
                if (AssemblyLoader.loadedAssemblies[i] == asm)
                {
                    AssemblyLoader.loadedAssemblies.RemoveAt(i);
                    break;
                }
            }   
            asm.Unload();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> getAllTypes()
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
