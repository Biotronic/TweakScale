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
            Tools.Logf("Changing volume from {0} to {1}", Module.volume, BaseModule.volume * factor.absolute.cubic);
            Module.ChangeVolume(BaseModule.volume * factor.absolute.cubic);
            foreach (PartResource f in Part.Resources)
            {
                f.amount /= factor.relative.cubic;
                f.maxAmount /= factor.relative.cubic;
            }
        }
    }
}
