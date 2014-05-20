using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TweakScale
{
    public interface IRescalable
    {
        void OnRescale(ScalingFactor factor);
    }

    public abstract class TweakScaleUpdater : IRescalable
    {
        // Every kind of updater is registered here, and the correct kind of updater is created for each PartModule.
        static Dictionary<string, Func<PartModule, IRescalable>> ctors = new Dictionary<string, Func<PartModule, IRescalable>>();
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

        protected Part Part
        {
            get
            {
                return _module.part;
            }
        }

        protected Part BasePart
        {
            get
            {
                return PartLoader.getPartInfoByName(Part.partInfo.name).partPrefab;
            }
        }

        protected T GetBaseModule<T>()
        {
            return BasePart.Modules.OfType<T>().First();
        }

        private TweakScaleUpdater()
        { }

        public TweakScaleUpdater(PartModule module)
        {
            _module = module;
        }

        // Creates an updater for each module attached to a part.
        public static IRescalable[] createUpdaters(Part part)
        {
            return part.Modules.Cast<PartModule>().Select(createUpdater).Where(a => (object)a != null).ToArray();
        }

        private static IRescalable createUpdater(PartModule module)
        {
            if (module is IRescalable)
            {
                return module as IRescalable;
            }
            var name = module.GetType().FullName;
            if (ctors.ContainsKey(name))
            {
                return ctors[name](module);
            }
            return null;
        }

        public abstract void OnRescale(ScalingFactor factor);
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<ModuleDeployableSolarPanel>();
            module.chargeRate = baseModule.chargeRate * factor.absolute.quadratic;
            module.flowRate = baseModule.flowRate * factor.absolute.quadratic;
            module.panelMass = baseModule.panelMass * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<ModuleReactionWheel>();
            module.PitchTorque = baseModule.PitchTorque * factor.absolute.cubic;
            module.YawTorque = baseModule.YawTorque * factor.absolute.cubic;
            module.RollTorque = baseModule.RollTorque * factor.absolute.cubic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<ModuleEngines>();
            module.minThrust = baseModule.minThrust * factor.absolute.quadratic;
            module.maxThrust = baseModule.maxThrust * factor.absolute.quadratic;
            module.heatProduction = baseModule.heatProduction * factor.absolute.squareRoot;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<ModuleEnginesFX>();
            module.minThrust = baseModule.minThrust * factor.absolute.quadratic;
            module.maxThrust = baseModule.maxThrust * factor.absolute.quadratic;
            module.heatProduction = baseModule.heatProduction * factor.absolute.squareRoot;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<ModuleRCS>();
            module.thrusterPower = baseModule.thrusterPower * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<ModuleControlSurface>();
            module.ctrlSurfaceArea = baseModule.ctrlSurfaceArea * factor.absolute.quadratic;
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

        public override void OnRescale(ScalingFactor factor)
        {
            var baseModule = GetBaseModule<ModuleResourceIntake>();
            module.area = baseModule.area * factor.absolute.quadratic;
        }
    }

    class TSGenericUpdater : IRescalable
    {
        Part _part;
        Part _basePart;

        public TSGenericUpdater(Part part)
        {
            _part = part;
            _basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;
        }

        private ConfigNode GetConfig(string name)
        {
            return GameDatabase.Instance.GetConfigs("TWEAKSCALEEXPONENTS").Where(a => a.name == name).Select(a=>a.config).FirstOrDefault();
        }

        private IEnumerable<Tuple<PartModule, PartModule>> ModSets
        {
            get
            {
                return _part.Modules.OfType<PartModule>().Zip(_basePart.Modules.OfType<PartModule>());
            }
        }


        public void OnRescale(ScalingFactor factor)
        {
            foreach (var modSet in ModSets)
            {
                var mod = modSet.Item1;
                var baseMod = modSet.Item2;
                var modType = mod.GetType();

                var cfg = GetConfig(modType.FullName);

                if (cfg != null)
                {
                    MonoBehaviour.print(string.Format("Found config for PartModule {0}", modType.FullName));
                    foreach (var value in cfg.values.OfType<ConfigNode.Value>())
                    {
                        if (value.name == "name")
                            continue;
                        MonoBehaviour.print(String.Format("Checking for field {0}", value.name));
                        double exp;
                        if (!double.TryParse(value.value, out exp))
                        {
                            MonoBehaviour.print(String.Format("Invalid value for exponent {0}: \"{1}\"", value.name, value.value));
                            continue;
                        }

                        var field = modType.GetField(value.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        var prop = modType.GetProperty(value.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (field != null)
                        {
                            if (field.FieldType == typeof(float))
                            {
                                float newValue = (float)field.GetValue(baseMod);
                                MonoBehaviour.print(String.Format("Old value: {0}", newValue));
                                newValue = newValue * (float)Math.Pow(factor.absolute.linear, exp);
                                MonoBehaviour.print(String.Format("New value: {0}", newValue));
                                field.SetValue(mod, newValue);
                            }
                            else if (field.FieldType == typeof(double))
                            {
                                double newValue = (double)field.GetValue(baseMod);
                                MonoBehaviour.print(String.Format("Old value: {0}", newValue));
                                newValue = newValue * Math.Pow(factor.absolute.linear, exp);
                                MonoBehaviour.print(String.Format("New value: {0}", newValue));
                                field.SetValue(mod, newValue);
                            }
                            else
                            {
                                MonoBehaviour.print(String.Format("Field {0} for PartModule type {1} is of type {2}. Required: float or bool", value.name, modType.FullName, field.FieldType.FullName));
                                continue;
                            }
                        }
                        else if (prop != null)
                        {
                            if (prop.PropertyType == typeof(float))
                            {
                                float newValue = (float)prop.GetValue(baseMod, null);
                                MonoBehaviour.print(String.Format("Old value: {0}", newValue));
                                newValue = newValue * (float)Math.Pow(factor.absolute.linear, exp);
                                MonoBehaviour.print(String.Format("New value: {0}", newValue));
                                prop.SetValue(mod, newValue, null);
                            }
                            else if (prop.PropertyType == typeof(double))
                            {
                                double newValue = (double)prop.GetValue(baseMod, null);
                                MonoBehaviour.print(String.Format("Old value: {0}", newValue));
                                newValue = newValue * Math.Pow(factor.absolute.linear, exp);
                                MonoBehaviour.print(String.Format("New value: {0}", newValue));
                                prop.SetValue(mod, newValue, null);
                            }
                            else
                            {
                                MonoBehaviour.print(String.Format("Property {0} for PartModule type {1} is of type {2}. Required: float or bool", value.name, modType.FullName, prop.PropertyType.FullName));
                                continue;
                            }
                        }
                        else
                        {
                            MonoBehaviour.print(String.Format("Non-existent field {0} for PartModule type {1}", value.name, modType.FullName));
                            continue;
                        }
                    }
                }
                else
                {
                    MonoBehaviour.print(string.Format("Didn't find config for PartModule {0}", modType.FullName));
                }
            }
        }
    }
}