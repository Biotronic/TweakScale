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
using ModularFuelTanks;
//using RealFuels;

public class GoodspeedTweakScale : PartModule
{
    [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Scale"), UI_FloatRange(minValue = 0f, maxValue = 4f, stepIncrement = 1f)]
    public float tweakScale = 1;
    
    [KSPField(isPersistant = true)]
    public float currentScale = -1;
    
    [KSPField(isPersistant = true)]
    public float defaultScale = -1;
    
    [KSPField(isPersistant = true)]
    public bool constantHeight = false;

    [KSPField(isPersistant = true)]
    public bool isFreeScale = false;
    
    private double[] scaleFactors = {0.625, 1.25, 2.5, 3.75, 5.0};
    
    private Part basePart;
    
    private Vector3 savedScale;

    private PartModule modularFuelTank = null;

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

        return (T)Convert.ChangeType(cfgValue, typeof(T));
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

    private double defaultScaleFactor
    {
        get
        {
            return getScaleFactor(defaultScale);
        }
    }

    private double currentScaleFactor
    {
        get
        {
            return getScaleFactor(currentScale);
        }
    }

    private double scaleFactor
    {
        get
        {
            return getScaleFactor(tweakScale);
        }
    }
    
    public override void OnStart(StartState state)
    {
        base.OnStart(state);
        basePart = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;
        modularFuelTank = part.Modules.Cast<PartModule>().SingleOrDefault(a => a.moduleName == "ModuleFuelTanks");

        // Read all non-persistent config values. This needs to be done in case the part file has changed since last we checked (i.e. the game has been restarted and we loaded a ship), or if we copy a part.
        var range = (UI_FloatRange)this.Fields["tweakScale"].uiControlEditor;
        isFreeScale = configValue("freeScale", defaultValue: false);
        scaleFactors = configValue("scaleFactors", defaultValue: new[] { 0.625, 1.25, 2.5, 3.75, 5.0 });
        range.minValue = configValue("minScale", defaultValue: isFreeScale ? 0.5f : 0f);
        range.maxValue = configValue("maxScale", defaultValue: scaleFactors.Length - 1f);
        range.stepIncrement = configValue("stepIncrement", defaultValue: isFreeScale ? 0.01f : 1f);

        if ( currentScale < 0f )
        {
            print("GTS defaultScale == " + defaultScale.ToString());
            tweakScale = currentScale = defaultScale = configValue("defaultScale", defaultValue: 1);
        }
        else
        {
            double rescaleAbsolute = scaleFactor / defaultScaleFactor;
            updateByWidth(rescaleAbsolute, false);
            part.mass = (float)(basePart.mass * (constantHeight ? rescaleAbsolute * rescaleAbsolute : rescaleAbsolute * rescaleAbsolute * rescaleAbsolute));
        }
    }
    
    private void moveNode(AttachNode node, AttachNode baseNode, Vector3 rescaleVector, bool movePart)
    {
        Vector3 oldPosition = node.position;
        node.position = Vector3.Scale(baseNode.position, rescaleVector);
        if ( movePart && node.attachedPart != null )
        {
            if ( node.attachedPart == part.parent )
                part.transform.Translate(oldPosition - node.position);
            else
                node.attachedPart.transform.Translate(node.position - oldPosition, part.transform);
        }
        node.size = (int)(baseNode.size + tweakScale - defaultScale);
        if ( node.size < 0 ) node.size = 0;
        node.breakingForce = part.breakingForce;
        node.breakingTorque = part.breakingTorque;
    }
    
    private void updateByWidth(double rescaleFactor, bool moveParts)
    {
        Vector3 rescaleVector = new Vector3((float)rescaleFactor, constantHeight ? 1f : (float)rescaleFactor, (float)rescaleFactor);
        
        savedScale = part.transform.GetChild(0).localScale = Vector3.Scale(basePart.transform.GetChild(0).localScale, rescaleVector);
        part.transform.GetChild(0).hasChanged = true;
        part.transform.hasChanged = true;
        
        foreach ( AttachNode node in part.attachNodes )
            moveNode(node, basePart.findAttachNode(node.id), rescaleVector, moveParts);
        if ( part.srfAttachNode != null )
            moveNode(part.srfAttachNode, basePart.srfAttachNode, rescaleVector, moveParts);
        if ( moveParts )
        {
            float relativeFactor = (float)(scaleFactor / currentScaleFactor);
            Vector3 relativeVector = new Vector3(relativeFactor, constantHeight ? 1f : relativeFactor, relativeFactor);
            foreach ( Part child in part.children )
            {
                if ( child.srfAttachNode != null && child.srfAttachNode.attachedPart == part ) // part is attached to us, but not on a node
                {
                    Vector3 attachedPosition = child.transform.localPosition + child.transform.localRotation * child.srfAttachNode.position;
                    Vector3 targetPosition = Vector3.Scale(attachedPosition, relativeVector);
                    child.transform.Translate(targetPosition - attachedPosition, part.transform);
                }
            }
        }
    }
    
    private void updateBySurfaceArea(double rescaleFactor) // values that change relative to the surface area (i.e. scale squared)
    {
        if ( basePart.breakingForce == 22f ) // not defined in the config, set to a reasonable default
            part.breakingForce = (float)(32.0 * scaleFactor * scaleFactor); // scale 1 = 50, scale 2 = 200, etc.
        else // is defined, scale it relative to new surface area
            part.breakingForce = (float)(basePart.breakingForce  * rescaleFactor);
        if ( part.breakingForce < 22f )
            part.breakingForce = 22f;
        
        if ( basePart.breakingTorque == 22f )
            part.breakingTorque = (float)(32.0 * scaleFactor * scaleFactor);
        else
            part.breakingTorque = (float)(basePart.breakingTorque * rescaleFactor);
        if ( part.breakingTorque < 22f )
            part.breakingTorque = 22f;
    }
    
    private void updateByRelativeVolume(double rescaleFactor) // values that change relative to the volume (i.e. scale cubed)
    {
        part.mass = (float)(part.mass * rescaleFactor);

        var newResourceValues = part.Resources.Cast<PartResource>().Select(a => new[] { a.amount * rescaleFactor, a.maxAmount * rescaleFactor }).ToArray();

        bool hasModularFuelTank = (object)modularFuelTank != null;
        if (hasModularFuelTank)
        {
            updateFuelTankVolume(modularFuelTank, rescaleFactor);
        }
        int i = 0;
        foreach (PartResource resource in part.Resources)
        {
            var newValues = newResourceValues[i];
            resource.amount = newValues[0];
            resource.maxAmount = newValues[1];
            i++;
        }
        if (hasModularFuelTank)
        {
            updateFuelTankMass(modularFuelTank);
        }
    }

    private void updateFuelTankMass(PartModule pm)
    {
        var fueltank = (ModuleFuelTanks)pm;
        fueltank.UpdateMass();
        //fueltank.UpdateTweakableMenu();
    }

    private void updateFuelTankVolume(PartModule pm, double rescaleFactor)
    {
        var fueltank = (ModuleFuelTanks)pm;

        fueltank.basemass = (float)(fueltank.basemass * rescaleFactor);
        fueltank.basemassPV = (float)(fueltank.basemassPV * rescaleFactor);
        fueltank.volume *= rescaleFactor;
        fueltank.UpdateMass();
    }
    
    private void updateWindow() // redraw the right-click window with the updated stats
    {
        foreach ( UIPartActionWindow win in FindObjectsOfType(typeof(UIPartActionWindow)) )
            if ( win.part == part )
                win.displayDirty = true;
    }
    
    public void Update()
    {
        if ( HighLogic.LoadedSceneIsEditor && currentScale >= 0f )
        {
            if ( tweakScale != currentScale ) // user has changed the scale tweakable
            {
                double rescaleAbsolute = scaleFactor / defaultScaleFactor;
                double rescaleRelative = scaleFactor / currentScaleFactor;
                
                updateBySurfaceArea(rescaleAbsolute * rescaleAbsolute); // call this first, results are used by updateByWidth
                updateByWidth(rescaleAbsolute, true);
                updateByRelativeVolume(constantHeight ? rescaleRelative * rescaleRelative : rescaleRelative * rescaleRelative * rescaleRelative);
                updateWindow(); // call this last

                currentScale = tweakScale;
            }
            else if ( part.transform.GetChild(0).localScale != savedScale ) // editor frequently nukes our OnStart resize some time later
            {
                updateByWidth(scaleFactor / defaultScaleFactor, false);
            }
        }
    }
}
