
# ⚠ Using this prefab has implications ⚠

**Anything that relies on world space will *not* function correctly.**
Examples:

* Static batching
* Occlusion culling
* VRCObjectSync
* User-placed portals

If you need to sync the position of an object you need to do so in relation to the `Anchor` object. It's the first child of `WorldParent`.

___

# Dependencies

* [Cyan Player Object Pool](https://cyanlaser.github.io/CyanPlayerObjectPool/)
* [VRRefAssist](https://livedimensions.github.io/VRRefAssist/)

___

# Installation

1. Make a backup of your project. You can easily do this with the Creator Companion.
2. Install the dependencies listed above.
3. [Add Floating Origin with the Creator Companion](https://kitkat4191.github.io/JetSim-VCC-Listing/)
    Alternatively you can manually install the unitypackage found in the [latest release](https://github.com/KitKat4191/JetSim-FloatingOrigin/releases/latest)
    If you instead wish to use the Unity Package Manager:
    * On the top bar in Unity `Window > Package Manager`
    * Click the `[+]` in the top left
    * Select `Add package from git URL`
    * Paste this link: `https://github.com/KitKat4191/JetSim-FloatingOrigin.git`
4. On the top bar in Unity `KitKat > JetSim > Floating Origin > Install`
5. You should have a `WorldParent` object in your scene. You will need to parent all the objects that players will be able to see, walk on, interact with, etc. to this object. For performance reasons It's important that you only parent objects that actually need to move with the world. I recommend that you also put objects that are close together in the world under a common parent. This is to reduce the amount of child objects on the `WorldParent`.

___
