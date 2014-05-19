using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TweakScale
{
    public interface ITweakScaleUpdatable
    {
        // Called by OnLoad.
        void OnLoadScaling(ScalingFactor factor);
        // Called by OnStart.
        void OnStartScaling(ScalingFactor factor);
        // Called before updating resources.
        void OnPreUpdateScaling(ScalingFactor factor);
        // Called after updating resources.
        void OnPostUpdateScaling(ScalingFactor factor);
    }

    public abstract class TweakScaleUpdater : ITweakScaleUpdatable
    {
        // Every kind of updater is registered here, and the correct kind of updater is created for each PartModule.
        static Dictionary<string, Func<PartModule, ITweakScaleUpdatable>> ctors = new Dictionary<string, Func<PartModule, ITweakScaleUpdatable>>();
        static TweakScaleUpdater()
        {
            // Initialize above array.
            // Stock modules:
            ctors["ModuleDeployableSolarPanel"] = a => new TweakScaleSolarPanelUpdater(a);
            ctors["ModuleEngines"] = a => new TweakScaleEngineUpdater(a);
            ctors["ModuleEnginesFX"] = a => new TweakScaleEngineFXUpdater(a);
            ctors["ModuleReactionWheel"] = a => new TweakScaleReactionWheelUpdater(a);
            ctors["ModuleRCS"] = a => new TweakScaleRCSUpdater(a);
            ctors["ModuleResourceIntake"] = a => new TweakScaleIntakeUpdater(a);
            ctors["ModuleControlSurface"] = a => new TweakScaleControlSurfaceUpdater(a);

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
        public static ITweakScaleUpdatable[] createUpdaters(Part part)
        {
            return part.Modules.Cast<PartModule>().Select(createUpdater).Where(a => (object)a != null).ToArray();
        }

        private static ITweakScaleUpdatable createUpdater(PartModule module)
        {
            if (module is ITweakScaleUpdatable)
            {
                return module as ITweakScaleUpdatable;
            }
            var name = module.GetType().FullName;
            if (ctors.ContainsKey(name))
            {
                return ctors[name](module);
            }
            return null;
        }

        public virtual void OnLoadScaling(ScalingFactor factor) { }
        public virtual void OnStartScaling(ScalingFactor factor) { }
        public virtual void OnPreUpdateScaling(ScalingFactor factor) { }
        public virtual void OnPostUpdateScaling(ScalingFactor factor) { }
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

        public override void OnStartScaling(ScalingFactor factor)
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

        public override void OnStartScaling(ScalingFactor factor)
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

        public override void OnStartScaling(ScalingFactor factor)
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

        public override void OnStartScaling(ScalingFactor factor)
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

        public override void OnStartScaling(ScalingFactor factor)
        {
            module.thrusterPower = module.thrusterPower * factor.absolute.quadratic;
        }
    }

    class TweakScaleControlSurfaceUpdater : TweakScaleUpdater
    {
        public TweakScaleControlSurfaceUpdater(PartModule pm)
            : base(pm)
        {
        }

        ModuleControlSurface module
        {
            get
            {
                return (ModuleControlSurface)_module;
            }
        }

        public override void OnStartScaling(ScalingFactor factor)
        {
            module.ctrlSurfaceArea = module.ctrlSurfaceArea * factor.absolute.quadratic;
        }
    }

    class TweakScaleIntakeUpdater : TweakScaleUpdater
    {
        public TweakScaleIntakeUpdater(PartModule pm)
            : base(pm)
        {
        }

        ModuleResourceIntake module
        {
            get
            {
                return (ModuleResourceIntake)_module;
            }
        }

        public override void OnStartScaling(ScalingFactor factor)
        {
            module.area = module.area * factor.absolute.quadratic;
        }
    }
}