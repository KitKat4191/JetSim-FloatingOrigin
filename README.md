
# JetSim - Floating Origin

___

## ⚠ Using this prefab has implications ⚠

**Anything that relies on world space may not function at all, or just produce undesireable results.**
Examples:

* Static batching
* Occlusion culling
* VRCObjectSync
* User-placed portals
* Specific shaders

If you need to sync the position of an object you need to do so relative to the `Anchor` object.
`Anchor` is the first child of `WorldParent`

One such system that can do this is [SmartObjectSync](https://github.com/MMMaellon/SmartObjectSync)

___

## Dependencies

* Cyan Player Object Pool [VCC](https://cyanlaser.github.io/CyanPlayerObjectPool/), [GitHub](https://github.com/CyanLaser/CyanPlayerObjectPool)
* VRRefAssist [VCC](https://livedimensions.github.io/VRRefAssist/), [GitHub](https://github.com/LiveDimensions/VRRefAssist)

___

## Installation instructions

Before doing anything else, please make a backup of your project.
You can easily do this with the Creator Companion.

### Option 1: VCC

* Add `Cyan Player Object Pool`, `VRRefAssist` and `Floating Origin` to the creator companion.
    `Floating Origin` is part of the [JetSim VCC listing](https://kitkat4191.github.io/JetSim-VCC-Listing/).

### Option 2: Unity Package Manager

* On the top bar in Unity `Window > Package Manager`
* Click the `[+]` in the top left
* Select `Add package from git URL`
* Paste this link: `https://github.com/KitKat4191/JetSim-FloatingOrigin.git`

### Option 3: Unitypackage

* Install the dependencies listed above
* Download the unitypackage in the [latest release](https://github.com/KitKat4191/JetSim-FloatingOrigin/releases/latest)
* Drag the unitypackage from your downloads folder to the `Project` tab in your open Unity project.

___

* In the `VRC Scene Descriptor` that can be found on the `VRCWorld` object you need to set the `Respawn Height Y` to some really large negative number such as `-100000`. This is to prevent the player and pickups constantly respawning when you fly or fall quickly.
* Also in the `Scene Descriptor`, I highly recommend that you enable `Forbid User Portals`. This is because user portals are synced in world space and will be desynced compared to the rest of the world.
* On the top bar in Unity `KitKat > JetSim > Floating Origin > Install`
* You now have a `WorldParent` object in your scene. You will need to parent all the objects that players will be able to see, walk on, interact with, etc. to this object.
* If you are using Unity 2022 you can mark the `WorldParent` object as the [default parent](https://vrclibrary.com/wiki/books/whats-new-in-unity-2022/page/set-any-gameobject-as-default-parent) to make your life easier.

___

### General Info

Please **DO NOT** unpack the `FloatingOrigin` prefab. If you for some reason have to unpack it, make sure that the `Anchor` object is the first child of `WorldParent`.

For performance reasons It's important that you only parent objects that actually need to move with the world. I recommend that you also put objects that are close together in the world under a common parent. This is to reduce the amount of root child objects on the `WorldParent`.

___
