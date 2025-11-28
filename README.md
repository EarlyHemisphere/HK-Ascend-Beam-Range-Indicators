# HK-Ascend-Beam-Range-Indicator

Adds indicators during the Ascension phase of the Absolute Radiance fight that show the range of the beam she is firing. There are three sets of them:

1. (cyan) Real-time indicator
2. (red) Indicators that only update the moment she aims her beam to show the range that the beam that fired last could have fired within
3. (green) A line denoting a 270 degree rotation relative to the origin of the Radiance's beams. On this line, the knight's hitbox is least likely to intersect with the next laser fired

The boundary lines are placed such that the outer edge of the lines matches exactly with the outer edge of the beam hitbox when the beam is shot at the extreme of its range.

This mod is made for Hollow Knight 1.5 and requires Mod Common cuz I'm too lazy to convert to SFCore and only testing with Any Radiance anyways.

The code is kinda bad but I don't wanna make it pretty at the moment.
