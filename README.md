# Installation instructions

___

1. Install [Cyan Player Object Pool](https://cyanlaser.github.io/CyanPlayerObjectPool/).
2. Install [VRRefAssist](https://github.com/LiveDimensions/VRRefAssist).
3. Add the Floating Origin unitypackage to your project.
4. On the top bar `KitKat > JetSim > Floating Origin > Install`.
5. You now have a `WorldParent` object in your scene. You will need to parent all objects that are part of the world to this object. It's important that you only parent objects that actually need to move with the world. This is because the more objects you have, the more processing it will take to move the origin. For optimal performance I recommend that you put objects that are close together in the world under a common parent. This is to reduce the amount of child objects on the `WorldParent`.
6. Most standard sync scripts such as `VRCObjectSync` will not work correctly. You need to use a sync script that can sync the position relative to a `Transform`.
