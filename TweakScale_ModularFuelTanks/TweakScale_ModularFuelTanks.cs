using System;
using System.Linq;
using TweakScale;

namespace TweakScale_ModularFuelTanks
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    internal class MyEditorRegistrationAddon : TweakScale.RescalableRegistratorAddon
    {
        public override void OnStart()
        {
            TweakScale.TweakScaleUpdater.RegisterUpdater((ModularFuelTanks.ModuleFuelTanks mod) => new TweakScaleModularFuelTanks4_3Updater(mod));
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class MyFlightRegistrationAddon : TweakScale.RescalableRegistratorAddon
    {
        public override void OnStart()
        {
            TweakScale.TweakScaleUpdater.RegisterUpdater((ModularFuelTanks.ModuleFuelTanks mod) => new TweakScaleModularFuelTanks4_3Updater(mod));
        }
    }

    // For old-style Modular Fuel Tanks.
    class TweakScaleModularFuelTanks4_3Updater : TweakScaleUpdater
    {
        public TweakScaleModularFuelTanks4_3Updater(PartModule pm)
            : base(pm)
        {
        }

        ModularFuelTanks.ModuleFuelTanks module
        {
            get
            {
                return (ModularFuelTanks.ModuleFuelTanks)_module;
            }
        }

        override public void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<ModularFuelTanks.ModuleFuelTanks>();
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
