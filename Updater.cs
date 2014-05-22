using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TweakScale
{
    public class RescalableRegistratorAddon : MonoBehaviour
    {
        private static bool loadedInScene;

        public void Start()
        {
            if (loadedInScene)
            {
                Destroy(gameObject);
                return;
            }
            loadedInScene = true;
            OnStart();
        }

        public virtual void OnStart()
        {
        }

        public void Update()
        {
            loadedInScene = false;
            Destroy(gameObject);
        }
    }

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
        TweakScale _ts;

        public TSGenericUpdater(Part part)
        {
            _part = part;
            _basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;
            _ts = part.Modules.OfType<TweakScale>().First();
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

        private T GetValue<T>(FieldInfo field, PropertyInfo prop, object source)
        {
            if (field != null)
                return (T)Convert.ChangeType(field.GetValue(source), typeof(T));
            else
                return (T)Convert.ChangeType(prop.GetValue(source, null), typeof(T));
        }

        private void SetValue<T>(FieldInfo field, PropertyInfo prop, object source, T value)
        {
            if (field != null)
                field.SetValue(source, Convert.ChangeType(value, field.FieldType));
            else
                prop.SetValue(source, Convert.ChangeType(value, prop.PropertyType), null);
        }

        private bool CheckType(Type needle, params Type[] haystack)
        {
            foreach (var straw in haystack)
                if (straw == needle)
                    return true;
            return false;
        }

        private void UpdateModule(ConfigNode.Value value, PartModule mod, PartModule baseMod, ScalingFactor factor)
        {
            var modType = mod.GetType();
            var field = modType.GetField(value.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var prop = modType.GetProperty(value.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field == null && prop == null)
            {
                Tools.LogErrorMessageF("Non-existent field {0} for PartModule type {1}", value.name, modType.FullName);
                return;
            }
            if ((field != null && !CheckType(field.FieldType, typeof(float), typeof(double))) ||
                (prop != null && !CheckType(prop.PropertyType, typeof(float), typeof(double))))
            {
                Tools.LogErrorMessageF("Field {0} for PartModule type {1} is of type {2}. Required: float or double", value.name, modType.FullName, field.FieldType.FullName);
                return;
            }

            if (value.value.Contains(","))
            {
                var values = value.value.Split(',').Select(a => (double)Convert.ChangeType(a, typeof(double))).ToArray();

                if (values.Length >= factor.index || factor.index < 0)
                {
                    SetValue(field, prop, mod, values[factor.index]);
                }
                else
                {
                    Tools.LogErrorMessageF("Can't set value from array: Index out of bounds: {0}.", factor.index);
                }
            }
            else
            {
                double exp;
                if (!double.TryParse(value.value, out exp))
                {
                    Tools.LogErrorMessageF("Invalid value for exponent {0}: \"{1}\"", value.name, value.value);
                    return;
                }
                var newValue = GetValue<double>(field, prop, baseMod);
                newValue *= Math.Pow(factor.absolute.linear, exp);
                SetValue(field, prop, mod, newValue);
            }
        }

        private void UpdateFromCfg(ConfigNode cfg, PartModule mod, PartModule baseMod, ScalingFactor factor)
        {

            if (cfg != null)
            {
                foreach (var value in cfg.values.OfType<ConfigNode.Value>())
                {
                    if (value.name == "name")
                        continue;
                    UpdateModule(value, mod, baseMod, factor);
                }
            }
        }

        public void OnRescale(ScalingFactor factor)
        {
            foreach (var modSet in ModSets)
            {
                var mod = modSet.Item1;
                var baseMod = modSet.Item2;
                var modType = mod.GetType();

                var localModules = _ts.moduleNode.GetNodes("MODULE");

                UpdateFromCfg(GetConfig(modType.FullName), mod, baseMod, factor);
                UpdateFromCfg(localModules.Where(a => a.GetValue("name") == modType.FullName).FirstOrDefault(), mod, baseMod, factor);
                if (modType.FullName != modType.Name)
                {
                    UpdateFromCfg(GetConfig(modType.Name), mod, baseMod, factor);
                    UpdateFromCfg(localModules.Where(a => a.GetValue("name") == modType.Name).FirstOrDefault(), mod, baseMod, factor);
                }
            }
        }
    }
}