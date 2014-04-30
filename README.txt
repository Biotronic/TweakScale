GoodspeedTweakScale
===================

Forked from Gaius Goodspeed's Goodspeed Aerospace Part & TweakScale plugin:
http://forum.kerbalspaceprogram.com/threads/72567-0-23-5-Goodspeed-Aerospace-Parts-TweakScale-plugin-v2014-4-1B

New Features:

Integration with Modular Fuel Tanks!
    Will automatically update MFT's volume and the volume of existing tanks.

Minimum and maximum scales!
    Want your part to be available only in 1.25m and 2.5m scales? Make it so!

Example MODULE declaration:

MODULE
{
	name = GoodspeedTweakScale
	defaultScale = 2 // Can be elided, default of 1
    minScale = 1 // Can be elided, default of 0
    maxScale = 2 // Can be elided, default of 4
}

===================

This software is made available by the author under the terms of the
Creative Commons Attribution-NonCommercial-ShareAlike license.  See
the following web page for details:

http://creativecommons.org/licenses/by-nc-sa/4.0/

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.