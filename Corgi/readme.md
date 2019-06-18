# Bug Fixes

The following were made to fix some issues with the Ladder / Grip classes that performed Character transform sets without checking platform collisions.

## CorgiControllerOverride
* Downward raycasts made too close (or in) to a platform mask with fail to detect a collide, and would prevent additional raycasts from triggering.
* Added SetTransform method that checks for platform collisions and moves Character to the closest point possible.

## CharterGripOverride
* Used safe transform set from overridden CorgiControllerOverride class.

## CharacterLadderOverride
* If discrete ladder GO were close together, race condition occurred, such that the last ladder piece was the one to set the variables (e.g. TouchingLadder, etc.). Used layer check instead.
* Replaced four instances where Character transform was being set without collision checks.