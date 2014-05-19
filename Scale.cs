using KSPAPIExtensions;
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
        [KSPField(isPersistant = true, guiActiveEditor = true, guiName = "Scale", guiFormat = "S4", guiUnits = "m")]
        [UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0.625f, maxValue = 5, incrementLarge = 1.25f, incrementSmall = 0.125f, incrementSlide = 0.001f)]
        public float tweakScale = 1;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiName = "Scale")]
        [UI_ChooseOption(scene = UI_Scene.Editor)]
        public int tweakName = 0;

        [KSPField(isPersistant = true)]
        public float currentScale = -1;

        [KSPField(isPersistant = true)]
        public float defaultScale = -1;

        [KSPField(isPersistant = true)]
        public bool isFreeScale = false;

        [KSPField(isPersistant = true)]
        public int version = 0;

        private static int currentVersion = 1;

        private float[] scaleFactors = { 0.625f, 1.25f, 2.5f, 3.75f, 5f };

        private float[] massFactors = { 0.0f, 0.0f, 1.0f };

        private Part basePart;

        private Vector3 savedScale;

        private ITweakScaleUpdatable[] updaters;

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

        private ScalingFactor scalingFactor
        {
            get
            {
                return new ScalingFactor(tweakScale / defaultScale, tweakScale / currentScale);
            }
        }


        private float minSize
        {
            get
            {
                if (isFreeScale)
                {
                    var range = (UI_FloatEdit)this.Fields["tweakScale"].uiControlEditor;
                    return range.minValue;
                }
                else
                {
                    return scaleFactors.Min();
                }
            }
        }

        private float maxSize
        {
            get
            {
                if (isFreeScale)
                {
                    var range = (UI_FloatEdit)this.Fields["tweakScale"].uiControlEditor;
                    return range.maxValue;
                }
                else
                {
                    return scaleFactors.Max();
                }
            }
        }


        private void SetupFromConfig(ScaleConfig config)
        {

            isFreeScale = config.isFreeScale;
            massFactors = config.massFactors;
            defaultScale = config.defaultScale;
            if (isFreeScale)
            {
                var range = (UI_FloatEdit)this.Fields["tweakScale"].uiControlEditor;
                range.minValue = config.minValue;
                range.maxValue = config.maxValue;
                range.incrementLarge = (float)Math.Round((range.maxValue - range.minValue) / 10, 2);
                range.incrementSmall = (float)Math.Round(range.incrementLarge / 10, 2);
                this.Fields["tweakScale"].guiUnits = config.suffix;
            }
            else
            {
                var options = (UI_ChooseOption)this.Fields["tweakName"].uiControlEditor;
                scaleFactors = config.scaleFactors;
                options.options = config.scaleNames;
            }
            this.Fields["tweakScale"].guiActiveEditor = isFreeScale;
            this.Fields["tweakName"].guiActiveEditor = !isFreeScale;
        }

        private void Setup()
        {
            if (part.partInfo == null)
            {
                return;
            }
            basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;

            updaters = TweakScaleUpdater.createUpdaters(part);

            SetupFromConfig(new ScaleConfig(moduleNode));

            if (currentScale < 0f)
            {
                tweakScale = currentScale = defaultScale;
                if (!isFreeScale)
                {
                    tweakName = Tools.ClosestIndex(defaultScale, scaleFactors);
                }
            }
            else
            {
                updateByWidth(scalingFactor, false);
                part.mass = basePart.mass * scalingFactor.absolute.cubic;
            }

            foreach (var updater in updaters)
            {
                updater.OnStartScaling(scalingFactor);
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Setup();
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            Setup();
        }

        public override void OnSave(ConfigNode node)
        {
            version = currentVersion;
            base.OnSave(node);
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



            node.size = (int)(baseNode.size + (tweakScale - defaultScale) / (maxSize - minSize) * 5);
            if (node.size < 0) node.size = 0;
            node.breakingForce = part.breakingForce;
            node.breakingTorque = part.breakingTorque;
        }

        private void updateByWidth(ScalingFactor factor, bool moveParts)
        {
            Vector3 rescaleVector = Vector3.one * factor.absolute.linear;

            savedScale = part.transform.GetChild(0).localScale = Vector3.Scale(basePart.transform.GetChild(0).localScale, rescaleVector);
            part.transform.GetChild(0).hasChanged = true;
            part.transform.hasChanged = true;

            foreach (AttachNode node in part.attachNodes)
                moveNode(node, basePart.findAttachNode(node.id), rescaleVector, moveParts);
            if (part.srfAttachNode != null)
                moveNode(part.srfAttachNode, basePart.srfAttachNode, rescaleVector, moveParts);
            if (moveParts)
            {
                Vector3 relativeVector = Vector3.one * factor.relative.linear;
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
                part.breakingForce = 32.0f * factor.relative.quadratic; // scale 1 = 50, scale 2 = 200, etc.
            else // is defined, scale it relative to new surface area
                part.breakingForce = basePart.breakingForce * factor.absolute.quadratic;
            if (part.breakingForce < 22f)
                part.breakingForce = 22f;

            if (basePart.breakingTorque == 22f)
                part.breakingTorque = 32.0f * factor.relative.quadratic;
            else
                part.breakingTorque = basePart.breakingTorque * factor.absolute.quadratic;
            if (part.breakingTorque < 22f)
                part.breakingTorque = 22f;
        }

        private void updateByRelativeVolume(ScalingFactor factor) // values that change relative to the volume (i.e. scale cubed)
        {

            part.mass = part.mass * massFactors.Select((a, i) => (float)Math.Pow(a * factor.relative.linear, i + 1)).Sum();

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
            if (!isFreeScale)
            {
                tweakScale = scaleFactors[tweakName];
            }
            if (HighLogic.LoadedSceneIsEditor && currentScale >= 0f)
            {
                if (tweakScale != currentScale) // user has changed the scale tweakable
                {
                    foreach (var updater in updaters)
                    {
                        updater.OnPreUpdateScaling(scalingFactor);
                    }
                    updateBySurfaceArea(scalingFactor); // call this first, results are used by updateByWidth
                    updateByWidth(scalingFactor, true);
                    updateByRelativeVolume(scalingFactor);
                    updateWindow(); // call this last

                    currentScale = tweakScale;
                    foreach (var updater in updaters)
                    {
                        updater.OnPostUpdateScaling(scalingFactor);
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