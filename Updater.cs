using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TweakScale
{
    public abstract class RescalableRegistratorAddon : MonoBehaviour
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

        public abstract void OnStart();

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

        private TweakScaleUpdater()
        { }

        public TweakScaleUpdater(PartModule module)
        {
            _module = module;
        }

        // Creates an updater for each module attached to destination part.
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

    abstract public class TweakScaleUpdater<T> : TweakScaleUpdater where T : PartModule
    {
        public TweakScaleUpdater(T pm)
            : base(pm)
        {
        }

        public T Module
        {
            get
            {
                return _module as T;
            }
        }

        public T BaseModule
        {
            get
            {
                return BasePart.Modules.OfType<T>().First();
            }
        }
    }

    /// <summary>
    /// This class updates fields and properties that are mentioned in TWEAKSCALEEXPONENTS blocks in .cfgs.
    /// It does this by looking up the field or property by name through reflection, and scales the exponentValue stored in the base part (i.e. prefab).
    /// </summary>
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

        public void OnRescale(ScalingFactor factor)
        {
            ScaleExponents.UpdateObject(_ts, _part, _basePart, _ts.config.exponents, factor);
        }
    }
}