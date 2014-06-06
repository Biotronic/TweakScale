using System;
using System.Linq;
using TweakScale;

namespace TweakScale_RealFuels
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    internal class MyEditorRegistrationAddon : TweakScale.RescalableRegistratorAddon
    {
        public override void OnStart()
        {
            TweakScale.TweakScaleUpdater.RegisterUpdater((RealFuels.ModuleFuelTanks mod) => new TweakScaleRealFuelUpdater(mod));
        }
    }

    class TweakScaleRealFuelUpdater : TweakScaleUpdater
    {
        public TweakScaleRealFuelUpdater(PartModule pm)
            : base(pm)
        {
        }

        RealFuels.ModuleFuelTanks module
        {
            get
            {
                return (RealFuels.ModuleFuelTanks)_module;
            }
        }

        override public void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<RealFuels.ModuleFuelTanks>();
            module.basemass = baseModule.basemass * factor.absolute.cubic;
            module.basemassPV = baseModule.basemassPV * factor.absolute.cubic;
            module.volume = baseModule.volume * factor.absolute.cubic;
            try
            {
                module.UpdateMass();
            }
            catch (Exception)
            {
                // Just silently ignore this one...
                // I have a reason: this is the only module that seems to misbehave when scaled too early.
            }
        }
    }
    /*
    class TweakScale_ModuleEngineConfigs : RealFuels.ModuleEngineConfigs, IRescalable
    {
        ScalingFactor _factor;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            print("OHAI from " + this.GetType().Name);
        }

        public void OnRescale(ScalingFactor factor)
        {
            _factor = factor;
        }

        public override void SetConfiguration(string newConfiguration = null)
        {
            base.SetConfiguration(newConfiguration);

            configMaxThrust *= _factor.absolute.quadratic;
            configMinThrust *= _factor.absolute.quadratic;
            configMassMult *= _factor.absolute.cubic;
        }
    }

    class TweakScale_ModuleHybridEngine : RealFuels.ModuleHybridEngine, IRescalable
    {
        ScalingFactor _factor;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            print("OHAI from " + this.GetType().Name);
        }

        public void OnRescale(ScalingFactor factor)
        {
            _factor = factor;
        }

        public override void SetConfiguration(string newConfiguration = null)
        {
            base.SetConfiguration(newConfiguration);

            configMaxThrust *= _factor.absolute.quadratic;
            configMinThrust *= _factor.absolute.quadratic;
            configMassMult *= _factor.absolute.cubic;
        }
    }

    class TweakScale_ModuleHybridEngines : RealFuels.ModuleHybridEngines, IRescalable
    {
        ScalingFactor _factor;
        ModuleEngines _engine = null;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            print("OHAI from " + this.GetType().Name);
        }

        public void OnRescale(ScalingFactor factor)
        {
            _factor = factor;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (this.ActiveEngine != _engine)
            {
                _engine = this.ActiveEngine;

                _engine.maxThrust *= _factor.absolute.quadratic;
                _engine.minThrust *= _factor.absolute.quadratic;
            }
        }
    }
    */
}
