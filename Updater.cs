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
        private static bool loadedInScene = false;

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

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class TweakScaleRegister : RescalableRegistratorAddon
    {
        override public void OnStart()
        {
            var genericRescalable = Tools.getAllTypes()
                .Where(IsGenericRescalable)
                .ToArray();

            foreach (var gen in genericRescalable)
            {
                var t = gen.GetInterfaces()
                    .First(a => a.IsGenericType &&
                    a.GetGenericTypeDefinition() == typeof(IRescalable<>));

                RegisterGenericRescalable(gen, t.GetGenericArguments()[0]);
            }
        }

        private static void RegisterGenericRescalable(Type resc, Type arg)
        {
            var c = resc.GetConstructor(new[] { arg });
            if (c != null)
            {
                Func<PartModule, IRescalable> creator = (PartModule pm) => (IRescalable)c.Invoke(new[] { pm });

                TweakScaleUpdater.RegisterUpdater(arg, creator);
            }
        }

        private static bool IsGenericRescalable(Type t)
        {
            return !t.IsGenericType && t.GetInterfaces()
                .Any(a => a.IsGenericType &&
                a.GetGenericTypeDefinition() == typeof(IRescalable<>));
        }
    }

    public abstract class TweakScaleUpdater : IRescalable
    {
        // Every kind of updater is registered here, and the correct kind of updater is created for each PartModule.
        static Dictionary<Type, Func<PartModule, IRescalable>> ctors = new Dictionary<Type, Func<PartModule, IRescalable>>();

        /// <summary>
        /// Registers an updater for partmodules the name <paramref name="moduleName"/>.
        /// </summary>
        /// <param name="moduleName">Name of the PartModule type to update.</param>
        /// <param name="creator">A function that creates an updater for this PartModule type.</param>
        static public void RegisterUpdater(Type pm, Func<PartModule, IRescalable> creator)
        {
            Tools.Logf("Registering updater for {0}", pm.FullName);
            ctors[pm] = creator;
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

        // Creates an updater for each modules attached to destination part.
        public static IEnumerable<IRescalable> createUpdaters(Part part)
        {
            foreach (var mod in part.Modules.Cast<PartModule>())
            {
                var updater = createUpdater(mod);
                if (updater != null)
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
            if (ctors.ContainsKey(module.GetType()))
            {
                return ctors[module.GetType()](module);
            }
            return null;
        }

        public abstract void OnRescale(ScalingFactor factor);
    }

    abstract public class TweakScaleUpdater<T> : TweakScaleUpdater, IRescalable<T> where T : PartModule
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