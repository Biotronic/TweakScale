using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TweakScale
{
    public interface IRescalable
    {
        void OnRescale(ScalingFactor factor);
    }

    public abstract class TweakScaleUpdater : IRescalable
    {
        // Every kind of updater is registered here, and the correct kind of updater is created for each PartModule.
        static Dictionary<string, Func<PartModule, IRescalable>> ctors = new Dictionary<string, Func<PartModule, IRescalable>>();
        static TweakScaleUpdater()
        {
            // Initialize above array.
            // Modular Fuel Tanks/Real Fuels:
            //ctors["ModularFuelTanks.ModuleFuelTanks"] = a => new TweakScaleModularFuelTanks4_3Updater(a);
            //ctors["RealFuels.ModuleFuelTanks"] = a => new TweakScaleRealFuelUpdater(a);
        }

        /// <summary>
        /// Registers an updater for partmodules the name <paramref name="moduleName"/>.
        /// </summary>
        /// <param name="moduleName">Name of the PartModule type to update.</param>
        /// <param name="creator">A function that creates an updater for this PartModule type.</param>
        static public void RegisterUpdater(string moduleName, Func<PartModule, IRescalable> creator)
        {
            ctors[moduleName] = creator;
        }

        /// <summary>
        /// Registers an updater for modules of the type <typeparamref name="ModuleType"/>.
        /// </summary>
        /// <typeparam name="ModuleType">The type of PartModule to update.</typeparam>
        /// <param name="creator">A function that creates an updater for this PartModule type.</param>
        static public void RegisterUpdater<ModuleType>(Func<ModuleType, IRescalable> creator) where ModuleType : PartModule
        {
            MonoBehaviour.print("Registering module updater: " + typeof(ModuleType).FullName);
            ctors[typeof(ModuleType).FullName] = a => a is ModuleType ? creator(a as ModuleType) : null;
            ctors[typeof(ModuleType).Name] = a => a is ModuleType ? creator(a as ModuleType) : null;
        }

        protected PartModule _module;

        protected Part Part
        {
            get
            {
                return _module.part;
            }
        }

        protected Part BasePart
        {
            get
            {
                return PartLoader.getPartInfoByName(Part.partInfo.name).partPrefab;
            }
        }

        protected T GetBaseModule<T>()
        {
            return BasePart.Modules.OfType<T>().First();
        }

        private TweakScaleUpdater()
        { }

        public TweakScaleUpdater(PartModule module)
        {
            _module = module;
        }

        // Creates an updater for each module attached to a part.
        public static IEnumerable<IRescalable> createUpdaters(Part part)
        {
            foreach (var mod in part.Modules.Cast<PartModule>())
            {
                var updater = createUpdater(mod);
                if ((object)updater != null)
                {
                    yield return updater;
                }
            }
            yield return new TSGenericUpdater(part);
        }

        private static IRescalable createUpdater(PartModule module)
        {
            if (module is IRescalable)
            {
                return module as IRescalable;
            }
            var name = module.GetType().FullName;
            if (ctors.ContainsKey(name))
            {
                return ctors[name](module);
            }
            name = module.GetType().Name;
            if (ctors.ContainsKey(name))
            {
                return ctors[name](module);
            }
            return null;
        }

        public abstract void OnRescale(ScalingFactor factor);
    }

    class TSGenericUpdater : IRescalable
    {
        Part _part;
        Part _basePart;

        public TSGenericUpdater(Part part)
        {
            _part = part;
            _basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;
        }

        private ConfigNode GetConfig(string name)
        {
            return GameDatabase.Instance.GetConfigs("TWEAKSCALEEXPONENTS").Where(a => a.name == name).Select(a=>a.config).FirstOrDefault();
        }

        private IEnumerable<Tuple<PartModule, PartModule>> ModSets
        {
            get
            {
                return _part.Modules.OfType<PartModule>().Zip(_basePart.Modules.OfType<PartModule>());
            }
        }


        public void OnRescale(ScalingFactor factor)
        {
            foreach (var modSet in ModSets)
            {
                var mod = modSet.Item1;
                var baseMod = modSet.Item2;
                var modType = mod.GetType();

                var cfg = GetConfig(modType.FullName);

                if (cfg != null)
                {
                    foreach (var value in cfg.values.OfType<ConfigNode.Value>())
                    {
                        if (value.name == "name")
                            continue;
                        double exp;
                        if (!double.TryParse(value.value, out exp))
                        {
                            MonoBehaviour.print(String.Format("Invalid value for exponent {0}: \"{1}\"", value.name, value.value));
                            continue;
                        }

                        var field = modType.GetField(value.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        var prop = modType.GetProperty(value.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (field != null)
                        {
                            if (field.FieldType == typeof(float))
                            {
                                float newValue = (float)field.GetValue(baseMod);
                                newValue = newValue * (float)Math.Pow(factor.absolute.linear, exp);
                                field.SetValue(mod, newValue);
                            }
                            else if (field.FieldType == typeof(double))
                            {
                                double newValue = (double)field.GetValue(baseMod);
                                newValue = newValue * Math.Pow(factor.absolute.linear, exp);
                                field.SetValue(mod, newValue);
                            }
                            else
                            {
                                MonoBehaviour.print(String.Format("Field {0} for PartModule type {1} is of type {2}. Required: float or bool", value.name, modType.FullName, field.FieldType.FullName));
                                continue;
                            }
                        }
                        else if (prop != null)
                        {
                            if (prop.PropertyType == typeof(float))
                            {
                                float newValue = (float)prop.GetValue(baseMod, null);
                                newValue = newValue * (float)Math.Pow(factor.absolute.linear, exp);
                                prop.SetValue(mod, newValue, null);
                            }
                            else if (prop.PropertyType == typeof(double))
                            {
                                double newValue = (double)prop.GetValue(baseMod, null);
                                newValue = newValue * Math.Pow(factor.absolute.linear, exp);
                                prop.SetValue(mod, newValue, null);
                            }
                            else
                            {
                                MonoBehaviour.print(String.Format("Property {0} for PartModule type {1} is of type {2}. Required: float or bool", value.name, modType.FullName, prop.PropertyType.FullName));
                                continue;
                            }
                        }
                        else
                        {
                            MonoBehaviour.print(String.Format("Non-existent field {0} for PartModule type {1}", value.name, modType.FullName));
                            continue;
                        }
                    }
                }
            }
        }
    }
}