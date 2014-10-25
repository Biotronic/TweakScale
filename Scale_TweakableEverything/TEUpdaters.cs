using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweakableEVA;
using TweakableEverything;

namespace TweakScale
{
    public static class TweakScaleTools
    {
        static public void RescaleFloatRange(PartModule pm, string name, float factor)
        {
            var field = pm.GetType().GetField(name);
            var oldValue = (float)field.GetValue(pm);
            var fr = (UI_FloatRange)pm.Fields[name].uiControlEditor;
            fr.maxValue *= factor;
            fr.minValue *= factor;
            fr.stepIncrement *= factor;
            field.SetValue(pm, oldValue * factor);
        }
    }

    public class ModuleTweakableDecoupleUpdater : IRescalable<ModuleTweakableDecouple>
    {
        private ModuleTweakableDecouple _module;

        public ModuleTweakableDecoupleUpdater(ModuleTweakableDecouple pm)
        {
            _module = pm;
        }

        public void OnRescale(ScalingFactor factor)
        {
            TweakScaleTools.RescaleFloatRange(_module, "ejectionForce", factor.relative.quadratic);
        }
    }

    public class ModuleTweakableDockingNodeUpdater : IRescalable<ModuleTweakableDockingNode>
    {
        private ModuleTweakableDockingNode _module;

        public ModuleTweakableDockingNodeUpdater(ModuleTweakableDockingNode pm)
        {
            _module = pm;
        }

        public void OnRescale(ScalingFactor factor)
        {
            TweakScaleTools.RescaleFloatRange(_module, "acquireRange", factor.relative.linear);
            TweakScaleTools.RescaleFloatRange(_module, "acquireForce", factor.relative.quadratic);
            TweakScaleTools.RescaleFloatRange(_module, "acquireTorque", factor.relative.quadratic);
            TweakScaleTools.RescaleFloatRange(_module, "undockEjectionForce", factor.relative.quadratic);
            TweakScaleTools.RescaleFloatRange(_module, "minDistanceToReEngage", factor.relative.linear);
        }
    }

    public class ModuleTweakableEVAUpdater : IRescalable<ModuleTweakableEVA>
    {
        private ModuleTweakableEVA _module;

        public ModuleTweakableEVAUpdater(ModuleTweakableEVA pm)
        {
            _module = pm;
        }

        public void OnRescale(ScalingFactor factor)
        {
            TweakScaleTools.RescaleFloatRange(_module, "thrusterPowerThrottle", factor.relative.linear);
        }
    }

    public class ModuleTweakableReactionWheelUpdater : IRescalable<ModuleTweakableReactionWheel>
    {
        private ModuleTweakableReactionWheel _module;

        public ModuleTweakableReactionWheelUpdater(ModuleTweakableReactionWheel pm)
        {
            _module = pm;
        }

        public void OnRescale(ScalingFactor factor)
        {
            TweakScaleTools.RescaleFloatRange(_module, "RollTorque", factor.relative.cubic);
            TweakScaleTools.RescaleFloatRange(_module, "PitchTorque", factor.relative.cubic);
            TweakScaleTools.RescaleFloatRange(_module, "YawTorque", factor.relative.cubic);
        }
    }
}
