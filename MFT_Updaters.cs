using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        override public void preUpdate(ScalingFactor factor)
        {
            module.basemass = module.basemass * factor.relative.cubic;
            module.basemassPV = module.basemassPV * factor.relative.cubic;
            module.volume *= factor.relative.cubic;
            module.UpdateMass();
        }

        override public void postUpdate(ScalingFactor factor)
        {
            module.UpdateMass();
            module.UpdateTweakableMenu();
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

        override public void preUpdate(ScalingFactor factor)
        {
            module.basemass = module.basemass * factor.relative.cubic;
            module.basemassPV = module.basemassPV * factor.relative.cubic;
            module.volume *= factor.relative.cubic;
            module.UpdateMass();
        }

        override public void postUpdate(ScalingFactor factor)
        {
            module.UpdateMass();
        }
    }
}
