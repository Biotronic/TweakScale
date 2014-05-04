using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweakScale
{
    abstract class TweakScaleUpdater
    {
        static Dictionary<string, Func<PartModule, TweakScaleUpdater>> ctors = new Dictionary<string,Func<PartModule, TweakScaleUpdater>>();
        static TweakScaleUpdater()
        {
            ctors["ModularFuelTanks.ModuleFuelTanks"] = a => new TweakScaleModularFuelTanks4_3Updater(a);
            ctors["RealFuels.ModuleFuelTanks"] = a => new TweakScaleRealFuelUpdater(a);
        }

        protected PartModule _module;

        private TweakScaleUpdater()
        { }

        public TweakScaleUpdater(PartModule module)
        {
            _module = module;
        }

        abstract public void preUpdate(double rescaleFactor);

        abstract public void postUpdate();

        public static TweakScaleUpdater[] createUpdaters(Part part)
        {
            return part.Modules.Cast<PartModule>().Select(createUpdater).ToArray();
        }

        private static TweakScaleUpdater createUpdater(PartModule module)
        {
            var name = module.GetType().FullName;
            if (ctors.ContainsKey(name))
            {
                return ctors[name](module);
            }
            return TweakScaleRegularUpdater.instance;
        }
    }

    class TweakScaleRegularUpdater : TweakScaleUpdater
    {
        private static TweakScaleRegularUpdater _instance = new TweakScaleRegularUpdater(null);

        public TweakScaleRegularUpdater(PartModule pm)
            : base(pm)
        {
        }

        public static TweakScaleRegularUpdater instance
        {
            get
            {
                return _instance;
            }
        }

        override public void preUpdate(double rescaleFactor)
        {
        }

        override public void postUpdate()
        {
        }
    }

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

        override public void preUpdate(double rescaleFactor)
        {
            module.basemass = (float)(module.basemass * rescaleFactor);
            module.basemassPV = (float)(module.basemassPV * rescaleFactor);
            module.volume *= rescaleFactor;
            module.UpdateMass();
        }

        override public void postUpdate()
        {
            module.UpdateMass();
            module.UpdateTweakableMenu();
        }
    }

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

        override public void preUpdate(double rescaleFactor)
        {
            module.basemass = (float)(module.basemass * rescaleFactor);
            module.basemassPV = (float)(module.basemassPV * rescaleFactor);
            module.volume *= rescaleFactor;
            module.UpdateMass();
        }

        override public void postUpdate()
        {
            module.UpdateMass();
        }
    }
}
