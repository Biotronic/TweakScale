using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TweakScale
{
    public abstract class RescalableRegistratorAddon : MonoBehaviour
    {
        private static bool _loadedInScene;

        public void Start()
        {
            if (_loadedInScene)
            {
                Destroy(gameObject);
                return;
            }
            _loadedInScene = true;
            OnStart();
        }

        public abstract void OnStart();

        public void Update()
        {
            _loadedInScene = false;
            Destroy(gameObject);
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class TweakScaleRegister : RescalableRegistratorAddon
    {
        override public void OnStart()
        {
            var genericRescalable = Tools.GetAllTypes()
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
            if (c == null)
                return;
            Func<PartModule, IRescalable> creator = pm => (IRescalable)c.Invoke(new object[] { pm });

            TweakScaleUpdater.RegisterUpdater(arg, creator);
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
        static readonly Dictionary<Type, Func<PartModule, IRescalable>> Ctors = new Dictionary<Type, Func<PartModule, IRescalable>>();

        /// <summary>
        /// Registers an updater for partmodules of type <paramref name="pm"/>.
        /// </summary>
        /// <param name="pm">Type of the PartModule type to update.</param>
        /// <param name="creator">A function that creates an updater for this PartModule type.</param>
        static public void RegisterUpdater(Type pm, Func<PartModule, IRescalable> creator)
        {
            Ctors[pm] = creator;
        }

        // Creates an updater for each modules attached to destination part.
        public static IEnumerable<IRescalable> CreateUpdaters(Part part)
        {
            var myUpdaters = part
                .Modules.Cast<PartModule>()
                .Select(CreateUpdater)
                .Where(updater => updater != null);
            foreach (var updater in myUpdaters)
            {
                yield return updater;
            }
            yield return new TSGenericUpdater(part);
            yield return new EmitterUpdater(part);
        }

        private static IRescalable CreateUpdater(PartModule module)
        {
            if (module is IRescalable)
            {
                return module as IRescalable;
            }
            return Ctors.ContainsKey(module.GetType()) ? Ctors[module.GetType()](module) : null;
        }
    }

    /// <summary>
    /// This class updates mmpfxField and properties that are mentioned in TWEAKSCALEEXPONENTS blocks in .cfgs.
    /// It does this by looking up the mmpfxField or property by name through reflection, and scales the exponentValue stored in the base part (i.e. prefab).
    /// </summary>
    class TSGenericUpdater : IRescalable
    {
        private readonly Part _part;
        private readonly Part _basePart;
        private readonly TweakScale _ts;

        public TSGenericUpdater(Part part)
        {
            _part = part;
            _basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;
            _ts = part.Modules.OfType<TweakScale>().First();
        }

        public void OnRescale(ScalingFactor factor)
        {
            ScaleExponents.UpdateObject(_part, _basePart, _ts.ScaleType.Exponents, factor);
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
            public readonly float MinSize, MaxSize, Shape1D;
            public readonly Vector2 Shape2D;
            public readonly Vector3 Shape3D, LocalVelocity, Force;

            public EmitterData(KSPParticleEmitter pe)
            {
                MinSize = pe.minSize;
                MaxSize = pe.maxSize;
                LocalVelocity = pe.localVelocity;
                Shape1D = pe.shape1D;
                Shape2D = pe.shape2D;
                Shape3D = pe.shape3D;
                Force = pe.force;
            }
        }

        readonly Part _part;
        readonly TweakScale _ts;
        bool _rescale = true;
        readonly Dictionary<KSPParticleEmitter, EmitterData> _scales = new Dictionary<KSPParticleEmitter, EmitterData>();

        public EmitterUpdater(Part part)
        {
            _part = part;
            _ts = part.Modules.OfType<TweakScale>().First();
        }

        public void OnRescale(ScalingFactor factor)
        {
            _rescale = true;
        }

        private static FieldInfo _mmpFxField;
        private static FieldInfo _mpFxField;

        private void UpdateParticleEmitter(KSPParticleEmitter pe)
        {
            if (pe == null)
            {
                return;
            }
            var factor = _ts.ScalingFactor;

            if (!_scales.ContainsKey(pe))
            {
                _scales[pe] = new EmitterData(pe);
            }
            var ed = _scales[pe];

            pe.minSize = ed.MinSize * factor.absolute.linear;
            pe.maxSize = ed.MaxSize * factor.absolute.linear;
            pe.shape1D = ed.Shape1D * factor.absolute.linear;
            pe.shape2D = ed.Shape2D * factor.absolute.linear;
            pe.shape3D = ed.Shape3D * factor.absolute.linear;

            pe.force = ed.Force * factor.absolute.linear;

            pe.localVelocity = ed.LocalVelocity * factor.absolute.linear;
        }

        private static void GetFieldInfos()
        {
            if (_mmpFxField == null)
                _mmpFxField = typeof(ModelMultiParticleFX).GetNonPublicFieldByType<List<KSPParticleEmitter>>();
            if (_mpFxField == null)
                _mpFxField = typeof(ModelParticleFX).GetNonPublicFieldByType<KSPParticleEmitter>();
        }

        public void OnUpdate()
        {
            if (!_rescale)
                return;
            GetFieldInfos();

            var fxn = _part.GetComponents<EffectBehaviour>();
            _rescale = fxn.Length != 0;
            foreach (var fx in fxn)
            {
                if (fx is ModelMultiParticleFX)
                {
                    var p = _mmpFxField.GetValue(fx) as List<KSPParticleEmitter>;
                    if (p == null)
                        continue;
                    foreach (var pe in p)
                    {
                        UpdateParticleEmitter(pe);
                    }
                    _rescale = false;
                }
                else if (fx is ModelParticleFX)
                {
                    var pe = _mpFxField.GetValue(fx) as KSPParticleEmitter;
                    UpdateParticleEmitter(pe);
                    _rescale = false;
                }
            }
        }
    }
}