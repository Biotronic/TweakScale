GoodspeedTweakScale
===================

Forked from Gaius Goodspeed's Goodspeed Aerospace Part & TweakScale plugin:
http://forum.kerbalspaceprogram.com/threads/72567-0-23-5-Goodspeed-Aerospace-Parts-TweakScale-plugin-v2014-4-1B

New Features:

Integration with Modular Fuel Tanks!
    Will automatically update MFT's volume and the volume of existing tanks.

Minimum and maximum scales!
    Want your part to be available only in 1.25m and 2.5m scales? Make it so!

Custom scale factors!
    Do you need 0.3125m scale in addition to the more common ones? Just add it! The maxScale and minScale automatically adapts to the number of scale factors.

Free scaling!
    Mostly for trusses, I think. Lets you create parts of any size.


Example MODULE declarations:

MODULE
{
    name = GoodspeedTweakScale
    defaultScale = 2 // Default 1
    minScale = 1     // Default 0
    maxScale = 2     // Default 4
}

MODULE
{
    name = GoodspeedTweakScale
    freeScale = true    // Default false
    stepIncrement = 0.1 // Default 0.01 when freeScale is true
    minScale = 0.5      // Default 0.0 when freeScale is true
    maxScale = 2.0      // Default 4.0 when freeScale is true
}

MODULE
{
    name = GoodspeedTweakScale
    scaleFactors = 0.625, 1.25, 2.5, 3.75, 5.0 // Default range of scale factors
}


===================

This software is made available by the author under the terms of the
Creative Commons Attribution-NonCommercial-ShareAlike license.  See
the following web page for details:

http://creativecommons.org/licenses/by-nc-sa/4.0/

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.