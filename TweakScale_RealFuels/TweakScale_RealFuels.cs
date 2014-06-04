using System;
using System.Linq;
using TweakScale;

namespace TweakScale_RealFuels
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    internal class MyEditorRegistrationAddon : TweakScale.RescalableRegistratorAddon
    {
        public override void OnStart()
        {
            TweakScale.TweakScaleUpdater.RegisterUpdater((RealFuels.ModuleFuelTanks mod) => new TweakScaleRealFuelUpdater(mod));
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class MyFlightRegistrationAddon : TweakScale.RescalableRegistratorAddon
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
}
