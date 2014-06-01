using System;

namespace TweakScale
{

    // For new-style (>v4.3) Real Fuels and Modular Fuel Tanks.
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
