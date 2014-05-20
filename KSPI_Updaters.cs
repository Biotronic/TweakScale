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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.ModuleSolarSail>();
            module.surfaceArea = module.surfaceArea * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.MicrowavePowerReceiver>();
            module.collectorArea = baseModule.collectorArea * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.ISRUScoop>();
            module.scoopair = baseModule.scoopair * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.AtmosphericIntake>();
            module.area = baseModule.area * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.FNRadiator>();
            module.radiatorArea = baseModule.radiatorArea * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.AlcubierreDrive>();
            module.effectSize1 = baseModule.effectSize1 * factor.absolute.linear;
            module.effectSize2 = baseModule.effectSize2 * factor.absolute.linear;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.FNNozzleController>();
            module.radius = baseModule.radius * factor.absolute.linear;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.AntimatterStorageTank>();
            module.chargeNeeded = baseModule.chargeNeeded * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.FNGenerator>();
            module.radius = baseModule.radius * factor.absolute.linear;
            module.maxThermalPower = baseModule.maxThermalPower * factor.absolute.cubic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.ElectricEngineController>();
            module.maxPower = baseModule.maxPower * (float)Math.Pow(factor.absolute.linear, Math.Log(6) / Math.Log(2));
        }
    }

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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<FNPlugin.FNFusionReactor>();
            module.radius = baseModule.radius * factor.absolute.linear;
            module.ThermalPower = baseModule.ThermalPower * factor.absolute.cubic;
            module.resourceRate = baseModule.resourceRate * factor.absolute.cubic;
            module.upgradedThermalPower = baseModule.upgradedThermalPower * factor.absolute.cubic;
            module.upgradedResourceRate = baseModule.upgradedResourceRate * factor.absolute.cubic;
            module.powerRequirements = baseModule.powerRequirements * (module.isTokomak ? factor.absolute.quadratic : factor.absolute.cubic);
        }
    }
}
