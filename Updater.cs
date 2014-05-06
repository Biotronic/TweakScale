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
            ctors["ModularFuelTanks.ModuleFuelTanks"] = a => new TweakScaleModularFuelTanks4_3Updater(a);
            ctors["RealFuels.ModuleFuelTanks"] = a => new TweakScaleRealFuelUpdater(a);
            ctors["ModuleDeployableSolarPanel"] = a => new TweakScaleSolarPanelUpdater(a);
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
        abstract public void onStart(ScalingFactor factor);

        // Called before updating resources.
        abstract public void preUpdate(ScalingFactor factor);

        // Called after updating resources.
        abstract public void postUpdate(ScalingFactor factor);
    }

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
            module.basemass = (float)(module.basemass * factor.relative.cubic);
            module.basemassPV = (float)(module.basemassPV * factor.relative.cubic);
            module.volume *= factor.relative.cubic;
            module.UpdateMass();
        }

        override public void postUpdate(ScalingFactor factor)
        {
            module.UpdateMass();
            module.UpdateTweakableMenu();
        }

        public override void onStart(ScalingFactor factor)
        {
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
            module.basemass = (float)(module.basemass * factor.relative.cubic);
            module.basemassPV = (float)(module.basemassPV * factor.relative.cubic);
            module.volume *= factor.relative.cubic;
            module.UpdateMass();
        }

        override public void postUpdate(ScalingFactor factor)
        {
            module.UpdateMass();
        }

        public override void onStart(ScalingFactor factor)
        {
        }
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

        public override void preUpdate(ScalingFactor factor)
        {
        }

        public override void postUpdate(ScalingFactor factor)
        {
        }

        public override void onStart(ScalingFactor factor)
        {
            module.chargeRate = (float)(module.chargeRate * factor.absolute.quadratic);
            module.flowRate = (float)(module.flowRate * factor.absolute.quadratic);
            module.panelMass = (float)(module.panelMass * factor.absolute.quadratic);
        }
    }
}