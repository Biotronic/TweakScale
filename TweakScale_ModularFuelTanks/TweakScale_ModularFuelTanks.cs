using System;
using System.Linq;
using TweakScale;

namespace TweakScale_ModularFuelTanks
{
    class TweakScaleModularFuelTanksUpdater : TweakScaleUpdater<RealFuels.ModuleFuelTanks>
    {
        public TweakScaleModularFuelTanksUpdater(RealFuels.ModuleFuelTanks pm)
            : base(pm)
        {
        }

        override public void OnRescale(ScalingFactor factor)
        {
            Module.ChangeVolume(BaseModule.volume * factor.absolute.cubic);
            foreach (PartResource f in Part.Resources)
            {
                f.amount /= factor.relative.cubic;
                f.maxAmount /= factor.relative.cubic;
            }
        }
    }
}
