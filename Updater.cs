using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TweakScale
{
    abstract class TweakScaleUpdater
    {
        // Every kind of updater is registered here, and the correct kind of updater is created for each PartModule.
        static Dictionary<string, Func<PartModule, TweakScaleUpdater>> ctors = new Dictionary<string, Func<PartModule, TweakScaleUpdater>>();
        static TweakScaleUpdater()
        {
            // Initialize above array.
            // Stock modules:
            ctors["ModuleDeployableSolarPanel"] = a => new TweakScaleSolarPanelUpdater(a);
            ctors["ModuleEngines"] = a => new TweakScaleEngineUpdater(a);
            ctors["ModuleEnginesFX"] = a => new TweakScaleEngineFXUpdater(a);
            ctors["ModuleReactionWheel"] = a => new TweakScaleReactionWheelUpdater(a);

            // Modular Fuel Tanks/Real Fuels:
            ctors["ModularFuelTanks.ModuleFuelTanks"] = a => new TweakScaleModularFuelTanks4_3Updater(a);
            ctors["RealFuels.ModuleFuelTanks"] = a => new TweakScaleRealFuelUpdater(a);

            // KSP Interstellar stuff:
            ctors["FNPlugin.AlcubierreDrive"] = a => new TweakScaleAlcubierreDriveUpdater(a);
            ctors["FNPlugin.AntimatterStorageTank"] = a => new TweakScaleAntimatterStorageTankUpdater(a);
            ctors["FNPlugin.AtmosphericIntake"] = a => new TweakScaleAtmosphericIntakeUpdater(a);
            ctors["FNPlugin.ElectricEngineController"] = a => new TweakScaleElectricEngineControllerUpdater(a);
            ctors["FNPlugin.FNGenerator"] = a => new TweakScaleFNGeneratorUpdater(a);
            ctors["FNPlugin.FNNozzleController"] = a => new TweakScaleFNNozzleControllerUpdater(a);
            ctors["FNPlugin.FNRadiator"] = a => new TweakScaleFNRadiatorUpdater(a);
            ctors["FNPlugin.FNRadiator"] = a => new TweakScaleFNRadiatorUpdater(a);
            ctors["FNPlugin.ISRUScoop"] = a => new TweakScaleISRUScoopUpdater(a);
            ctors["FNPlugin.MicrowavePowerReceiver"] = a => new TweakScaleMicrowavePowerReceiverUpdater(a);
            ctors["FNPlugin.ModuleSolarSail"] = a => new TweakScaleSolarSailUpdater(a);
        }

        protected PartModule _module;

        private TweakScaleUpdater()
        { }

        public TweakScaleUpdater(PartModule module)
        {
            _module = module;
        }

        // Creates an updater for each module attached to a part.
        public static TweakScaleUpdater[] createUpdaters(Part part)
        {
            return part.Modules.Cast<PartModule>().Select(createUpdater).Where(a => (object)a != null).ToArray();
        }

        private static TweakScaleUpdater createUpdater(PartModule module)
        {
            var name = module.GetType().FullName;
            if (ctors.ContainsKey(name))
            {
                MonoBehaviour.print("Creating updater for " + name);
                return ctors[name](module);
            }
            return null;
        }

        // Called on start. Use this for setting up non-persistent values.
        public virtual void onStart(ScalingFactor factor) { }

        // Called before updating resources.
        public virtual void preUpdate(ScalingFactor factor) { }

        // Called after updating resources.
        public virtual void postUpdate(ScalingFactor factor) { }
    }

    class TweakScaleSolarPanelUpdater : TweakScaleUpdater
    {
        public TweakScaleSolarPanelUpdater(PartModule pm)
            : base(pm)
        {
        }

        ModuleDeployableSolarPanel module
        {
            get
            {
                return (ModuleDeployableSolarPanel)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.chargeRate = module.chargeRate * factor.absolute.quadratic;
            module.flowRate = module.flowRate * factor.absolute.quadratic;
            module.panelMass = module.panelMass * factor.absolute.quadratic;
        }
    }

    class TweakScaleReactionWheelUpdater : TweakScaleUpdater
    {
        public TweakScaleReactionWheelUpdater(PartModule pm)
            : base(pm)
        {
        }

        ModuleReactionWheel module
        {
            get
            {
                return (ModuleReactionWheel)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.PitchTorque = module.PitchTorque * factor.absolute.cubic;
            module.YawTorque = module.YawTorque * factor.absolute.cubic;
            module.RollTorque = module.RollTorque * factor.absolute.cubic;
        }
    }

    class TweakScaleEngineUpdater : TweakScaleUpdater
    {
        public TweakScaleEngineUpdater(PartModule pm)
            : base(pm)
        {
        }

        ModuleEngines module
        {
            get
            {
                return (ModuleEngines)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.minThrust = module.minThrust * factor.absolute.quadratic;
            module.maxThrust = module.maxThrust * factor.absolute.quadratic;
            module.heatProduction = module.heatProduction * factor.absolute.squareRoot;
        }
    }

    class TweakScaleEngineFXUpdater : TweakScaleUpdater
    {
        public TweakScaleEngineFXUpdater(PartModule pm)
            : base(pm)
        {
        }

        ModuleEnginesFX module
        {
            get
            {
                return (ModuleEnginesFX)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.minThrust = module.minThrust * factor.absolute.quadratic;
            module.maxThrust = module.maxThrust * factor.absolute.quadratic;
            module.heatProduction = module.heatProduction * factor.absolute.squareRoot;
        }
    }

    class TweakScaleRCSUpdater : TweakScaleUpdater
    {
        public TweakScaleRCSUpdater(PartModule pm)
            : base(pm)
        {
        }

        ModuleRCS module
        {
            get
            {
                return (ModuleRCS)_module;
            }
        }

        public override void onStart(ScalingFactor factor)
        {
            module.thrusterPower = module.thrusterPower * factor.absolute.quadratic;
        }
    }
}