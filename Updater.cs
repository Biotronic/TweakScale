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

    static class TweakScaleUpdater
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
            ctors[pm] = creator;
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
            yield return new EmitterUpdater(part);
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
    }

    /// <summary>
    /// This class updates mmpfxField and properties that are mentioned in TWEAKSCALEEXPONENTS blocks in .cfgs.
    /// It does this by looking up the mmpfxField or property by name through reflection, and scales the exponentValue stored in the base part (i.e. prefab).
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
            ScaleExponents.UpdateObject(_part, _basePart, _ts.config.exponents, factor);
        }
    }

    interface IUpdateable
    {
        void OnUpdate();
    }
    
    class EmitterUpdater : IRescalable, IUpdateable
    {
        private struct EmitterData
        {
            public float minSize, maxSize, shape1D;
            public Vector2 shape2D;
            public Vector3 shape3D, localVelocity, force;

            public EmitterData(KSPParticleEmitter pe)
            {
                minSize = pe.minSize;
                maxSize = pe.maxSize;
                localVelocity = pe.localVelocity;
                shape1D = pe.shape1D;
                shape2D = pe.shape2D;
                shape3D = pe.shape3D;
                force = pe.force;
            }
        }

        Part _part;
        Part _basePart;
        TweakScale _ts;
        bool _rescale = true;
        Dictionary<KSPParticleEmitter, EmitterData> _scales = new Dictionary<KSPParticleEmitter, EmitterData>();

        public EmitterUpdater(Part part)
        {
            _part = part;
            _basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;
            _ts = part.Modules.OfType<TweakScale>().First();
        }

        public void OnRescale(ScalingFactor factor)
        {
            _rescale = true;
        }

        private static FieldInfo mmpFxField = null;
        private static FieldInfo mpFxField = null;

        private void UpdateParticleEmitter(KSPParticleEmitter pe)
        {
            if (pe == null)
            {
                return;
            }
            var factor = _ts.scalingFactor;

            if (!_scales.ContainsKey(pe))
            {
                _scales[pe] = new EmitterData(pe);
            }
            var ed = _scales[pe];

            pe.minSize = ed.minSize * factor.absolute.linear;
            pe.maxSize = ed.maxSize * factor.absolute.linear;
            pe.shape1D = ed.shape1D * factor.absolute.linear;
            pe.shape2D = ed.shape2D * factor.absolute.linear;
            pe.shape3D = ed.shape3D * factor.absolute.linear;

            pe.force = ed.force * factor.absolute.linear;

            pe.localVelocity = ed.localVelocity * factor.absolute.linear;
        }

        private static void GetFieldInfos()
        {
            if (mmpFxField == null)
                mmpFxField = typeof(ModelMultiParticleFX).GetNonPublicFieldByType<List<KSPParticleEmitter>>();
            if (mpFxField == null)
                mpFxField = typeof(ModelParticleFX).GetNonPublicFieldByType<KSPParticleEmitter>();
        }

        public void OnUpdate()
        {
            if (_rescale)
            {
                GetFieldInfos();

                var fxn = _part.GetComponents<EffectBehaviour>();
                _rescale = fxn.Length != 0;
                foreach (var fx in fxn)
                {
                    if (fx is ModelMultiParticleFX)
                    {
                        var p = mmpFxField.GetValue(fx) as List<KSPParticleEmitter>;
                        if (p != null)
                        {
                            foreach (var pe in p)
                            {
                                UpdateParticleEmitter(pe);
                            }
                            _rescale = false;
                        }
                    }
                    else if (fx is ModelParticleFX)
                    {
                        var pe = mpFxField.GetValue(fx) as KSPParticleEmitter;
                        UpdateParticleEmitter(pe);
                        _rescale = false;
                    }
                }
            }
        }
    }
}