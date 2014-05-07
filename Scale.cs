/* GoodspeedTweakScale plugin (c) Copyright 2014 Gaius Goodspeed

This software is made available by the author under the terms of the
Creative Commons Attribution-NonCommercial-ShareAlike license.  See
the following web page for details:

http://creativecommons.org/licenses/by-nc-sa/4.0/

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweakScale
{
    public class GoodspeedTweakScale : TweakScale
    {
    }
    public class TweakScale : PartModule
    {
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Scale")]
        [UI_FloatRange(minValue = 0f, maxValue = 4f, stepIncrement = 1f)]
        public float tweakScale = 1;

        [KSPField(isPersistant = true)]
        public float currentScale = -1;

        [KSPField(isPersistant = true)]
        public float defaultScale = -1;

        [KSPField(isPersistant = true)]
        public bool isFreeScale = false;

        private double[] scaleFactors = { 0.625, 1.25, 2.5, 3.75, 5.0 };

        private double[] massFactors = {0, 0, 1};

        private Part basePart;

        private Vector3 savedScale;

        private TweakScaleUpdater[] updaters;

        /// <summary>
        /// The ConfigNode that belongs to this module.
        /// </summary>
        private ConfigNode moduleNode
        {
            get
            {
                return GameDatabase.Instance.GetConfigs("PART").Single(c => c.name.Replace('_', '.') == part.partInfo.name)
                    .config.GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
            }
        }

        /// <summary>
        /// Reads a value from the ConfigNode and magically converts it to the type you ask. Tested for float, boolean and double[]. Anything else is at your own risk.
        /// </summary>
        /// <typeparam name="T">The type to convert to. Usually inferred from <paramref name="defaultValue"/>.</typeparam>
        /// <param name="name">Name of the ConfigNode's field</param>
        /// <param name="defaultValue">The value to use when the ConfigNode doesn't contain what we want.</param>
        /// <returns>The value in the ConfigNode, or <paramref name="defaultValue"/> if no decent value is found there.</returns>
        private T configValue<T>(string name, T defaultValue)
        {
            if (!moduleNode.HasValue(name))
            {
                return defaultValue;
            }
            string cfgValue = moduleNode.GetValue(name);
            try
            {
                return (T)Convert.ChangeType(cfgValue, typeof(T));
            }
            catch (InvalidCastException)
            {
                print("Failed to convert string value \"" + cfgValue + "\" to type " + typeof(T).Name);
                return defaultValue;
            }
        }

        private T[] configValue<T>(string name, T[] defaultValue)
        {
            if (!moduleNode.HasValue(name))
            {
                return defaultValue;
            }
            string cfgValue = moduleNode.GetValue(name);
            try
            {
                return cfgValue.Split(',').Select(a => (T)Convert.ChangeType(a, typeof(T))).ToArray();
            }
            catch (InvalidCastException)
            {
                print("Failed to convert string value \"" + cfgValue + "\" to type " + typeof(T[]).Name);
                return defaultValue;
            }
        }

        private double getScaleFactor(double index)
        {
            if (isFreeScale)
            {
                return index;
            }
            else
            {
                return scaleFactors[(int)index];
            }
        }

        private ScalingFactor scalingFactor
        {
            get
            {
                return new ScalingFactor(getScaleFactor(tweakScale) / getScaleFactor(defaultScale), getScaleFactor(tweakScale) / getScaleFactor(currentScale));
            }
        }

        private double clamp(double x, double min, double max)
        {
            return x < min ? min : x > max ? max : x;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;

            updaters = TweakScaleUpdater.createUpdaters(part);

            // Read all non-persistent config values. This needs to be done in case the part file has changed since last we checked (i.e. the game has been restarted and we loaded a ship), or if we copy a part.
            var range = (UI_FloatRange)this.Fields["tweakScale"].uiControlEditor;
            isFreeScale = configValue("freeScale", defaultValue: false);
            scaleFactors = configValue("scaleFactors", defaultValue: new[] { 0.625, 1.25, 2.5, 3.75, 5.0 }).OrderBy(a=>a).ToArray();
            massFactors = configValue("massFactors", defaultValue: new[] { 0.0, 0.0, 1.0 });
            range.minValue = configValue("minScale", defaultValue: isFreeScale ? 0.5f : 0.0f);
            range.maxValue = configValue("maxScale", defaultValue: isFreeScale ? 2.0f : scaleFactors.Length - 1.0f);
            range.stepIncrement = configValue("stepIncrement", defaultValue: isFreeScale ? 0.01f : 1.0f);

            if (currentScale < 0f)
            {
                print("GTS defaultScale == " + defaultScale.ToString());
                var tmpScale = configValue("defaultScale", defaultValue: 1.0);
                tweakScale = currentScale = defaultScale = (float)clamp(tmpScale, range.minValue, range.maxValue);
            }
            else
            {
                updateByWidth(scalingFactor, false);
                part.mass = (float)(basePart.mass * scalingFactor.absolute.cubic);
            }

            foreach (var updater in updaters)
            {
                updater.onStart(scalingFactor);
            }
        }

        private void moveNode(AttachNode node, AttachNode baseNode, Vector3 rescaleVector, bool movePart)
        {
            Vector3 oldPosition = node.position;
            node.position = Vector3.Scale(baseNode.position, rescaleVector);
            if (movePart && node.attachedPart != null)
            {
                if (node.attachedPart == part.parent)
                    part.transform.Translate(oldPosition - node.position);
                else
                    node.attachedPart.transform.Translate(node.position - oldPosition, part.transform);
            }
            node.size = (int)(baseNode.size + tweakScale - defaultScale);
            if (node.size < 0) node.size = 0;
            node.breakingForce = part.breakingForce;
            node.breakingTorque = part.breakingTorque;
        }

        private void updateByWidth(ScalingFactor factor, bool moveParts)
        {
            Vector3 rescaleVector = new Vector3((float)factor.absolute.linear, (float)factor.absolute.linear, (float)factor.absolute.linear);

            savedScale = part.transform.GetChild(0).localScale = Vector3.Scale(basePart.transform.GetChild(0).localScale, rescaleVector);
            part.transform.GetChild(0).hasChanged = true;
            part.transform.hasChanged = true;

            foreach (AttachNode node in part.attachNodes)
                moveNode(node, basePart.findAttachNode(node.id), rescaleVector, moveParts);
            if (part.srfAttachNode != null)
                moveNode(part.srfAttachNode, basePart.srfAttachNode, rescaleVector, moveParts);
            if (moveParts)
            {
                Vector3 relativeVector = new Vector3((float)factor.relative.linear, (float)factor.relative.linear, (float)factor.relative.linear);
                foreach (Part child in part.children)
                {
                    if (child.srfAttachNode != null && child.srfAttachNode.attachedPart == part) // part is attached to us, but not on a node
                    {
                        Vector3 attachedPosition = child.transform.localPosition + child.transform.localRotation * child.srfAttachNode.position;
                        Vector3 targetPosition = Vector3.Scale(attachedPosition, relativeVector);
                        child.transform.Translate(targetPosition - attachedPosition, part.transform);
                    }
                }
            };
        }

        private void updateBySurfaceArea(ScalingFactor factor) // values that change relative to the surface area (i.e. scale squared)
        {
            if (basePart.breakingForce == 22f) // not defined in the config, set to a reasonable default
                part.breakingForce = (float)(32.0 * factor.relative.quadratic); // scale 1 = 50, scale 2 = 200, etc.
            else // is defined, scale it relative to new surface area
                part.breakingForce = (float)(basePart.breakingForce * factor.absolute.quadratic);
            if (part.breakingForce < 22f)
                part.breakingForce = 22f;

            if (basePart.breakingTorque == 22f)
                part.breakingTorque = (float)(32.0 * factor.relative.quadratic);
            else
                part.breakingTorque = (float)(basePart.breakingTorque * factor.absolute.quadratic);
            if (part.breakingTorque < 22f)
                part.breakingTorque = 22f;
        }

        private void updateByRelativeVolume(ScalingFactor factor) // values that change relative to the volume (i.e. scale cubed)
        {

            part.mass = (float)(part.mass * massFactors.Select((a, i) => Math.Pow(a * factor.relative.linear, i + 1)).Sum());

            var newResourceValues = part.Resources.OfType<PartResource>().Select(a => new[] { a.amount * factor.relative.cubic, a.maxAmount * factor.relative.cubic }).ToArray();

            int idx = 0;
            foreach (PartResource resource in part.Resources)
            {
                var newValues = newResourceValues[idx];
                resource.amount = newValues[0];
                resource.maxAmount = newValues[1];
                idx++;
            }
        }

        private bool hasResources
        {
            get
            {
                return part.Resources.Count > 0;
            }
        }

        private void updateWindow() // redraw the right-click window with the updated stats
        {
            if (!isFreeScale && hasResources)
            {
                foreach (UIPartActionWindow win in FindObjectsOfType(typeof(UIPartActionWindow)))
                {
                    if (win.part == part)
                    {
                        // This causes the slider to be non-responsive - i.e. after you click once, you must click again, not drag the slider.
                        win.displayDirty = true;
                    }
                }
            }
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsEditor && currentScale >= 0f)
            {
                if (tweakScale != currentScale) // user has changed the scale tweakable
                {
                    foreach (var updater in updaters)
                    {
                        updater.preUpdate(scalingFactor);
                    }
                    updateBySurfaceArea(scalingFactor); // call this first, results are used by updateByWidth
                    updateByWidth(scalingFactor, true);
                    updateByRelativeVolume(scalingFactor);
                    updateWindow(); // call this last

                    currentScale = tweakScale;
                    foreach (var updater in updaters)
                    {
                        updater.postUpdate(scalingFactor);
                    }
                }
                else if (part.transform.GetChild(0).localScale != savedScale) // editor frequently nukes our OnStart resize some time later
                {
                    updateByWidth(scalingFactor, false);
                }
            }
        }
    }
}