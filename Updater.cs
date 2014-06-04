using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        ConfigNode[] _localModules;

        public TSGenericUpdater(Part part)
        {
            _part = part;
            _basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;
            _ts = part.Modules.OfType<TweakScale>().First();
            _localModules = _ts.moduleNode.GetNodes("MODULE");
        }

        private IEnumerable<ConfigNode> GetConfigs()
        {
            foreach (var node in GameDatabase.Instance.GetConfigs("TWEAKSCALEEXPONENTS").Select(a => a.config).Where(a => !a.HasValue("name")))
            {
                yield return node;
            }
            foreach (var node in _localModules.Where(a => !a.HasValue("name")))
            {
                yield return node;
            }
        }

        private IEnumerable<ConfigNode> GetConfigs(params string[] names)
        {
            foreach (var name in names)
            {
                foreach (var node in GameDatabase.Instance.GetConfigs("TWEAKSCALEEXPONENTS").Where(a => a.name == name).Select(a => a.config))
                {
                    yield return node;
                }
                foreach (var node in _localModules.Where(a => a.GetValue("name") == name))
                {
                    yield return node;
                }
            }
        }

        private IEnumerable<Tuple<PartModule, PartModule>> ModSets
        {
            get
            {
                return _part.Modules.OfType<PartModule>().Zip(_basePart.Modules.OfType<PartModule>());
            }
        }

        private void UpdateModule(ConfigNode.Value value, object mod, object baseMod, ScalingFactor factor)
        {
            var baseVal = MemberChanger<double>.CreateFromName(baseMod, value.name);
            var val = MemberChanger<double>.CreateFromName(mod, value.name);
            var modType = mod.GetType();

            if (val == null)
            {
                Tools.Logf("Non-existent Member {0} for PartModule type {1}", value.name, modType.FullName);
                return;
            }
            if (val.MemberType != typeof(float) && val.MemberType != typeof(double))
            {
                Tools.Logf("Member {0} for PartModule type {1} is of type {2}. Required: float or double", value.name, modType.FullName, val.MemberType.FullName);
                return;
            }

            if (value.value.Contains(","))
            {
                var values = value.value.Split(',').Select(a => (double)Convert.ChangeType(a, typeof(double))).ToArray();

                if (values.Length >= factor.index || factor.index < 0)
                {
                    val.Value =values[factor.index];
                }
                else
                {
                    Tools.Logf("Can't set value from array: Index out of bounds: {0}.", factor.index);
                }
            }
            else
            {
                double exp;
                if (!double.TryParse(value.value, out exp))
                {
                    Tools.Logf("Invalid value for exponent {0}: \"{1}\"", value.name, value.value);
                    return;
                }
                var newValue = baseVal.Value;
                newValue *= Math.Pow(factor.absolute.linear, exp);
                val.Value = newValue;
            }
        }

        private void UpdateSubItem(ConfigNode cfg, object mod, object baseMod, ScalingFactor factor)
        {
            var f1 = mod.GetType().GetField(cfg.name, BindingFlags.Instance | BindingFlags.Public);
            if (f1 == null)
                return;

            if (f1.FieldType.GetInterface("IEnumerable") != null)
            {
                var name = cfg.HasValue("name") ? cfg.GetValue("name") : "*";

                var ie1 = ((IEnumerable)f1.GetValue(mod)).Cast<object>();
                var ie2 = ((IEnumerable)f1.GetValue(baseMod)).Cast<object>();
                if (name == "*")
                {
                    foreach (var e in ie1.Zip(ie2))
                    {
                        UpdateFromCfgs(new[] { cfg }, e.Item1, e.Item2, factor);
                    }
                }
                else
                {
                    foreach (var e in ie1.Zip(ie2))
                    {
                        var et = e.Item1.GetType();
                        var n = et.GetField("name", BindingFlags.Instance | BindingFlags.Public);
                        if (n.FieldType != typeof(string))
                            continue;

                        string nn = (string)n.GetValue(e.Item1);
                        if (nn == name)
                        {
                            UpdateFromCfgs(new[] { cfg }, e.Item1, e.Item2, factor);
                        }
                    }
                }
            }
            else
            {
                UpdateFromCfgs(new[] { cfg }, f1.GetValue(mod), f1.GetValue(baseMod), factor);
            }
        }

        private void UpdateFromCfgs(IEnumerable<ConfigNode> cfgs, object mod, object baseMod, ScalingFactor factor)
        {
            if (mod == null || baseMod == null || cfgs == null)
                return;

            foreach (var cfg in cfgs)
            {
                foreach (ConfigNode.Value value in cfg.values)
                {
                    if (value.name == "name")
                        continue;
                    UpdateModule(value, mod, baseMod, factor);
                }

                foreach (ConfigNode node in cfg.nodes.OfType<ConfigNode>())
                {
                    UpdateSubItem(node, mod, baseMod, factor);
                }
            }
        }

        public void OnRescale(ScalingFactor factor)
        {
            var cfgs = GetConfigs();
            UpdateFromCfgs(cfgs, _part, _basePart, factor);

            foreach (var modSet in ModSets)
            {
                var mod = modSet.Item1;
                var baseMod = modSet.Item2;
                var modType = mod.GetType();

                cfgs = GetConfigs(modType.Name, modType.FullName);

                UpdateFromCfgs(cfgs, mod, baseMod, factor);
            }
        }
    }
}