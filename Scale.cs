using KSPAPIExtensions;
/* GoodspeedTweakScale plugin (fundCfg) Copyright 2014 Gaius Goodspeed

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
using System.Reflection;
using UnityEngine;

namespace TweakScale
{
    /// <summary>
    /// Converts from Gaius' GoodspeedTweakScale to updated TweakScale.
    /// </summary>
    public class GoodspeedTweakScale : TweakScale
    {
        private bool updated = false;

        protected override void Setup()
        {
            base.Setup();
            if (!updated)
            {
                tweakName = (int)tweakScale;
                tweakScale = scaleFactors[tweakName];
            }
        }
    }

    public class TweakScale : PartModule, IPartCostModifier
    {
        /// <summary>
        /// The selected scale. Different from currentScale only for destination single update, where currentScale is set to match this.
        /// </summary>
        [KSPField(isPersistant = true, guiActiveEditor = true, guiName = "Scale", guiFormat = "S4", guiUnits = "m")]
        [UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0.625f, maxValue = 5, incrementLarge = 1.25f, incrementSmall = 0.125f, incrementSlide = 0.001f)]
        public float tweakScale = 1;

        /// <summary>
        /// Index into scale values array.
        /// </summary>
        [KSPField(isPersistant = false, guiActiveEditor = true, guiName = "Scale")]
        [UI_ChooseOption(scene = UI_Scene.Editor)]
        public int tweakName = 0;

        /// <summary>
        /// The scale to which the part currently is scaled.
        /// </summary>
        [KSPField(isPersistant = true)]
        public float currentScale = -1;

        /// <summary>
        /// The default scale, i.e. the number by which to divide tweakScale and currentScale to get the relative size difference from when the part is used without TweakScale.
        /// </summary>
        [KSPField(isPersistant = true)]
        public float defaultScale = -1;

        /// <summary>
        /// Whether the part should be freely scalable or limited to destination list of allowed values.
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool isFreeScale = false;

        /// <summary>
        /// The version of TweakScale last used to change this part. Intended for use in the case of non-backward-compatible changes.
        /// </summary>
        [KSPField(isPersistant = true)]
        public string version;

        /// <summary>
        /// The scale exponentValue array. If isFreeScale is false, the part may only be one of these scales.
        /// </summary>
        protected float[] scaleFactors = { 0.625f, 1.25f, 2.5f, 3.75f, 5f };

        /// <summary>
        /// The unmodified prefab part. From this, default values are found.
        /// </summary>
        private Part prefabPart;

        /// <summary>
        /// Like currentScale above, this is the current scale vector. If TweakScale supports non-uniform scaling in the future (e.g. changing only the length of destination booster), savedScale may represent such destination scaling, while currentScale won't.
        /// </summary>
        private Vector3 savedScale;

        /// <summary>
        /// The exponentValue by which the part is scaled by default. When destination part uses MODEL { scale = ... }, this will be different from (1,1,1).
        /// </summary>
        [KSPField(isPersistant = true)]
        public Vector3 defaultTransformScale = new Vector3(0f, 0f, 0f);

        /// <summary>
        /// Updaters for different PartModules.
        /// </summary>
        private IRescalable[] updaters;

        private enum Tristate
        {
            True,
            False,
            Unset
        }

        /// <summary>
        /// Whether this instance of TweakScale is the first. If not, log an error and make sure the TweakScale modules don't harmfully interact.
        /// </summary>
        private Tristate duplicate = Tristate.Unset;

        /// <summary>
        /// The Config for this part.
        /// </summary>
        public ScaleConfig config;

        /// <summary>
        /// Cost of unscaled, empty part.
        /// </summary>
        [KSPField(isPersistant = true)]
        public float dryCost;

        /// <summary>
        /// The ConfigNode that belongs to the part this modules affects.
        /// </summary>
        private ConfigNode PartNode
        {
            get
            {
                return GameDatabase.Instance.GetConfigs("PART").Single(c => c.name.Replace('_', '.') == part.partInfo.name)
                    .config;
            }
        }

        /// <summary>
        /// The ConfigNode that belongs to this modules.
        /// </summary>
        public ConfigNode moduleNode
        {
            get
            {
                return PartNode.GetNodes("MODULE").FirstOrDefault(n => n.GetValue("name") == moduleName);
            }
        }

        /// <summary>
        /// The current scaling factor.
        /// </summary>
        private ScalingFactor scalingFactor
        {
            get
            {
                return new ScalingFactor(tweakScale / defaultScale, tweakScale / currentScale, isFreeScale ? -1 : tweakName);
            }
        }

        /// <summary>
        /// The smallest scale the part can be.
        /// </summary>
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

        /// <summary>
        /// The largest scale the part can be.
        /// </summary>
        internal float maxSize
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

        /// <summary>
        /// Loads settings from <paramref name="config"/>.
        /// </summary>
        /// <param name="config">The settings to use.</param>
        private void SetupFromConfig(ScaleConfig config)
        {
            isFreeScale = config.isFreeScale;
            defaultScale = config.defaultScale;
            this.Fields["tweakScale"].guiActiveEditor = false;
            this.Fields["tweakName"].guiActiveEditor = false;
            if (isFreeScale)
            {
                this.Fields["tweakScale"].guiActiveEditor = true;
                var range = (UI_FloatEdit)this.Fields["tweakScale"].uiControlEditor;
                range.minValue = config.minValue;
                range.maxValue = config.maxValue;
                range.incrementLarge = (float)Math.Round((range.maxValue - range.minValue) / 10, 2);
                range.incrementSmall = (float)Math.Round(range.incrementLarge / 10, 2);
                this.Fields["tweakScale"].guiUnits = config.suffix;
            }
            else
            {
                this.Fields["tweakName"].guiActiveEditor = config.scaleFactors.Length > 1;
                var options = (UI_ChooseOption)this.Fields["tweakName"].uiControlEditor;

                if (scaleFactors.Length > 0)
                {
                    scaleFactors = config.scaleFactors;
                    options.options = config.scaleNames;
                }
            }
        }

        /// <summary>
        /// Sets up values from config, creates updaters, and sets up initial values.
        /// </summary>
        protected virtual void Setup()
        {
            if (part.partInfo == null)
            {
                return;
            }

            prefabPart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;

            updaters = TweakScaleUpdater.createUpdaters(part).ToArray();

            SetupFromConfig(config = new ScaleConfig(moduleNode));

            if (currentScale < 0f)
            {
                tweakScale = currentScale = defaultScale;
                if (!isFreeScale)
                {
                    tweakName = Tools.ClosestIndex(defaultScale, scaleFactors);
                    tweakScale = scaleFactors[tweakName];
                }
                dryCost = (float)(part.partInfo.cost - prefabPart.Resources.Cast<PartResource>().Aggregate(0.0, (a, b) => a + b.amount * b.info.unitCost));
            }
            else
            {
                if (!isFreeScale)
                {
                    tweakName = Tools.ClosestIndex(tweakScale, scaleFactors);
                    tweakScale = scaleFactors[tweakName];
                }
                updateByWidth(false);
                foreach (var updater in updaters)
                {
                    updater.OnRescale(scalingFactor);
                }
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
            version = this.GetType().Assembly.GetName().Version.ToString();
            base.OnSave(node);
        }

        /// <summary>
        /// Moves <paramref name="node"/> to reflect the new scale. If <paramref name="movePart"/> is true, also moves attached parts.
        /// </summary>
        /// <param name="node">The node to move.</param>
        /// <param name="baseNode">The same node, as found on the prefab part.</param>
        /// <param name="movePart">Whether or not to move attached parts.</param>
        private void moveNode(AttachNode node, AttachNode baseNode, bool movePart)
        {
            Vector3 oldPosition = node.position;
            node.position = baseNode.position * scalingFactor.absolute.linear;
            if (movePart && node.attachedPart != null)
            {
                if (node.attachedPart == part.parent)
                    part.transform.Translate(oldPosition - node.position);
                else
                    node.attachedPart.transform.Translate(node.position - oldPosition, part.transform);
            }
            rescaleNode(node, baseNode);
        }

        /// <summary>
        /// Change the size of <paramref name="node"/> to reflect the new size of the part it's attached to.
        /// </summary>
        /// <param name="node">The node to resize.</param>
        /// <param name="baseNode">The same node, as found on the prefab part.</param>
        private void rescaleNode(AttachNode node, AttachNode baseNode)
        {
            if (isFreeScale)
            {
                node.size = (int)(baseNode.size + (tweakScale - defaultScale) / (maxSize - minSize) * 5);
            }
            else
            {
                var options = (UI_ChooseOption)this.Fields["tweakName"].uiControlEditor;
                node.size = (int)(baseNode.size + (tweakName - Tools.ClosestIndex(defaultScale, config.allScaleFactors)) / (float)config.allScaleFactors.Length * 5);
            }
            if (node.size < 0)
            {
                node.size = 0;
            }
        }

        /// <summary>
        /// Change the size of all attachment nodes to reflect the new size of the part they're attached to.
        /// </summary>
        private void rescaleAllNodes()
        {
            foreach (AttachNode node in part.attachNodes)
            {
                var nodesWithSameId = part.attachNodes
                    .Where(a => a.id == node.id)
                    .ToArray();
                var idIdx = Array.FindIndex(nodesWithSameId, a => a == node);
                var baseNodesWithSameId = prefabPart.attachNodes
                    .Where(a => a.id == node.id)
                    .ToArray();
                if (idIdx < baseNodesWithSameId.Length)
                {
                    var baseNode = baseNodesWithSameId[idIdx];

                    rescaleNode(node, baseNode);
                }
                else
                {
                    Tools.LogWf("Error scaling part. Node {0} does not have counterpart in base part.", node.id);
                }
            }
            rescaleNode(part.srfAttachNode, prefabPart.srfAttachNode);
        }

        /// <summary>
        /// Updates properties that change linearly with scale.
        /// </summary>
        /// <param name="moveParts">Whether or not to move attached parts.</param>
        private void updateByWidth(bool moveParts)
        {
            if (defaultTransformScale.x == 0.0f)
            {
                defaultTransformScale = part.transform.GetChild(0).localScale;
            }

            savedScale = part.transform.GetChild(0).localScale = scalingFactor.absolute.linear * defaultTransformScale;
            part.transform.GetChild(0).hasChanged = true;
            part.transform.hasChanged = true;

            foreach (AttachNode node in part.attachNodes)
            {
                var nodesWithSameId = part.attachNodes
                    .Where(a => a.id == node.id)
                    .ToArray();
                var idIdx = Array.FindIndex(nodesWithSameId, a => a == node);
                var baseNodesWithSameId = prefabPart.attachNodes
                    .Where(a => a.id == node.id)
                    .ToArray();
                if (idIdx < baseNodesWithSameId.Length)
                {
                    var baseNode = baseNodesWithSameId[idIdx];

                    moveNode(node, baseNode, moveParts);
                }
                else
                {
                    Tools.LogWf("Error scaling part. Node {0} does not have counterpart in base part.", node.id);
                }
            }

            if (part.srfAttachNode != null)
            {
                moveNode(part.srfAttachNode, prefabPart.srfAttachNode, moveParts);
            }
            if (moveParts)
            {
                foreach (Part child in part.children)
                {
                    if (child.srfAttachNode != null && child.srfAttachNode.attachedPart == part) // part is attached to us, but not on destination node
                    {
                        Vector3 attachedPosition = child.transform.localPosition + child.transform.localRotation * child.srfAttachNode.position;
                        Vector3 targetPosition = attachedPosition * scalingFactor.relative.linear;
                        child.transform.Translate(targetPosition - attachedPosition, part.transform);
                    }
                }
            };
        }

        /// <summary>
        /// Whether the part holds any resources (fuel, electricity, etc).
        /// </summary>
        private bool hasResources
        {
            get
            {
                return part.Resources.Count > 0;
            }
        }

        /// <summary>
        /// Marks the right-click window as dirty (i.e. tells it to update).
        /// </summary>
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
            if (duplicate != Tristate.False)
            {
                if (duplicate == Tristate.True)
                {
                    return;
                }
                if (this != part.Modules.OfType<TweakScale>().First())
                {
                    Tools.LogWf("Duplicate TweakScale module on part [{0}] {1}", part.partInfo.name, part.partInfo.title);
                    Fields["tweakScale"].guiActiveEditor = false;
                    Fields["tweakName"].guiActiveEditor = false;
                    duplicate = Tristate.True;
                    return;
                }
                duplicate = Tristate.False;
            }

            if (HighLogic.LoadedSceneIsEditor && currentScale >= 0f)
            {
                bool changed = isFreeScale ? tweakScale != currentScale : currentScale != scaleFactors[tweakName];

                if (changed) // user has changed the scale tweakable
                {
                    if (!isFreeScale)
                    {
                        tweakScale = scaleFactors[tweakName];
                    }

                    updateByWidth(true);
                    updateWindow();

                    foreach (var updater in updaters)
                    {
                        updater.OnRescale(scalingFactor);
                    }
                    currentScale = tweakScale;
                }
                else if (part.transform.GetChild(0).localScale != savedScale) // editor frequently nukes our OnStart resize some time later
                {
                    updateByWidth(false);
                }
            }
        }

        public float GetModuleCost()
        {
            if (currentScale == -1)
            {
                Setup();
            }
            return (float)(dryCost - part.partInfo.cost + part.Resources.Cast<PartResource>().Aggregate(0.0, (a, b) => a + b.amount * b.info.unitCost));
        }
    }
}