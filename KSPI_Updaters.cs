using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweakScale
{

    class TweakScaleSolarSailUpdater : TweakScaleUpdater
    {
        public TweakScaleSolarSailUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.ModuleSolarSail module
        {
            get
            {
                return (FNPlugin.ModuleSolarSail)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.surfaceArea = (float)(module.surfaceArea * factor.absolute.quadratic);
        }
    }

    class TweakScaleMicrowavePowerReceiverUpdater : TweakScaleUpdater
    {
        public TweakScaleMicrowavePowerReceiverUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.MicrowavePowerReceiver module
        {
            get
            {
                return (FNPlugin.MicrowavePowerReceiver)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.collectorArea = (float)(module.collectorArea * factor.absolute.quadratic);
        }
    }

    class TweakScaleISRUScoopUpdater : TweakScaleUpdater
    {
        public TweakScaleISRUScoopUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.ISRUScoop module
        {
            get
            {
                return (FNPlugin.ISRUScoop)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.scoopair = (float)(module.scoopair * factor.absolute.quadratic);
        }
    }

    class TweakScaleAtmosphericIntakeUpdater : TweakScaleUpdater
    {
        public TweakScaleAtmosphericIntakeUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.AtmosphericIntake module
        {
            get
            {
                return (FNPlugin.AtmosphericIntake)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.area = (float)(module.area * factor.absolute.quadratic);
        }
    }

    class TweakScaleFNRadiatorUpdater : TweakScaleUpdater
    {
        public TweakScaleFNRadiatorUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.FNRadiator module
        {
            get
            {
                return (FNPlugin.FNRadiator)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.radiatorArea = (float)(module.radiatorArea * factor.absolute.quadratic);
        }
    }

    class TweakScaleAlcubierreDriveUpdater : TweakScaleUpdater
    {
        public TweakScaleAlcubierreDriveUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.AlcubierreDrive module
        {
            get
            {
                return (FNPlugin.AlcubierreDrive)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.effectSize1 = (float)(module.effectSize1 * factor.absolute.linear);
            module.effectSize2 = (float)(module.effectSize2 * factor.absolute.linear);
        }
    }

    class TweakScaleFNNozzleControllerUpdater : TweakScaleUpdater
    {
        public TweakScaleFNNozzleControllerUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.FNNozzleController module
        {
            get
            {
                return (FNPlugin.FNNozzleController)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.radius = (float)factor.absolute.linear;
        }
    }

    class TweakScaleAntimatterStorageTankUpdater : TweakScaleUpdater
    {
        public TweakScaleAntimatterStorageTankUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.AntimatterStorageTank module
        {
            get
            {
                return (FNPlugin.AntimatterStorageTank)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.chargeNeeded = (float)(module.chargeNeeded * factor.absolute.quadratic);
        }
    }

    class TweakScaleFNGeneratorUpdater : TweakScaleUpdater
    {
        public TweakScaleFNGeneratorUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.FNGenerator module
        {
            get
            {
                return (FNPlugin.FNGenerator)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.radius = (float)factor.absolute.linear;
            module.maxThermalPower = (float)(module.maxThermalPower * factor.absolute.cubic);
        }
    }

    class TweakScaleElectricEngineControllerUpdater : TweakScaleUpdater
    {
        public TweakScaleElectricEngineControllerUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.ElectricEngineController module
        {
            get
            {
                return (FNPlugin.ElectricEngineController)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.maxPower = (float)(module.maxPower * Math.Pow(factor.absolute.linear, Math.Log(6) / Math.Log(2)));
        }
    }
    /*
    class TweakScaleFNNuclearReactorUpdater : TweakScaleUpdater
    {
        public TweakScaleFNNuclearReactorUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.FNNuclearReactor module
        {
            get
            {
                return (FNPlugin.FNNuclearReactor)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.radius = (float)factor.absolute.linear;
            // This is not right, but it's an approximation. *sigh*
            module.ThermalPower = (float)(module.ThermalPower * Math.Pow(factor.absolute.linear, Math.Log(80 / 3) / Math.Log(2)));
            module.resourceRate = (float)(module.resourceRate * Math.Pow(factor.absolute.linear, 3.4099964449619539041298010520704));
        }
    }
    
    class TweakScaleFNAmatCatFissionFusionReactorUpdater : TweakScaleUpdater
    {
        public TweakScaleFNAmatCatFissionFusionReactorUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.FNAmatCatFissionFusionReactor module
        {
            get
            {
                return (FNPlugin.FNAmatCatFissionFusionReactor)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.radius = (float)factor.absolute.linear;
            // This is not right, but it's an approximation. *sigh*
            module.ThermalPower = (float)(module.ThermalPower * Math.Pow(factor.absolute.linear, Math.Log(80 / 3) / Math.Log(2)));
            module.resourceRate = (float)(module.resourceRate * Math.Pow(factor.absolute.linear, 3.4099964449619539041298010520704));
        }
    }
    
    class TweakScaleFNPFissionReactorUpdater : TweakScaleUpdater
    {
        public TweakScaleFNPFissionReactorUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.FNPFissionReactor module
        {
            get
            {
                return (FNPlugin.FNPFissionReactor)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.radius = (float)factor.absolute.linear;
            // This is not right, but it's an approximation. *sigh*
            module.ThermalPower = (float)(module.ThermalPower * Math.Pow(factor.absolute.linear, Math.Log(80 / 3) / Math.Log(2)));
            module.resourceRate = (float)(module.resourceRate * Math.Pow(factor.absolute.linear, 3.4099964449619539041298010520704));
        }
    }

    class TweakScaleFNAntimatterReactorUpdater : TweakScaleUpdater
    {
        public TweakScaleFNAntimatterReactorUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.FNAntimatterReactor module
        {
            get
            {
                return (FNPlugin.FNAntimatterReactor)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.radius = (float)factor.absolute.linear;
            module.ThermalPower = (float)(module.ThermalPower * factor.absolute.cubic);
            module.resourceRate = (float)(module.resourceRate * factor.absolute.cubic);
            module.upgradedThermalPower = (float)(module.upgradedThermalPower * factor.absolute.cubic);
            module.upgradedResourceRate = (float)(module.upgradedResourceRate * factor.absolute.cubic);
            // And then some magic for ReactorTemp and upgradedReactorTemp... factor^1.22? It's about right, but without math to back it up, I can't simply accept that.
        }
    }
    */

    class TweakScaleFNFusionReactorUpdater : TweakScaleUpdater
    {
        public TweakScaleFNFusionReactorUpdater(PartModule pm)
            : base(pm)
        {
        }

        FNPlugin.FNFusionReactor module
        {
            get
            {
                return (FNPlugin.FNFusionReactor)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.radius = (float)factor.absolute.linear;
            module.ThermalPower = (float)(module.ThermalPower * factor.absolute.cubic);
            module.resourceRate = (float)(module.resourceRate * factor.absolute.cubic);
            module.upgradedThermalPower = (float)(module.upgradedThermalPower * factor.absolute.cubic);
            module.upgradedResourceRate = (float)(module.upgradedResourceRate * factor.absolute.cubic);
            module.powerRequirements = (float)(module.powerRequirements * (module.isTokomak ? factor.absolute.quadratic : factor.absolute.cubic));
        }
    }
}
