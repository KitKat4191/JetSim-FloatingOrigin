
# JetSim - Floating Origin

___

![JetSim](https://raw.githubusercontent.com/KitKat4191/JetSim-VCC-Listing/main/Website/banner.png)

___

## ⚠ Using this prefab has implications ⚠

**Anything that relies on world space may not function at all, or just produce undesireable results.**

Examples:

* Static batching
* Occlusion culling
* VRCObjectSync
* Specific shaders

When syncing the position of an object you need to do so relative to the `Anchor` object.
`Anchor` is the first child of `WorldParent`.

One such system that can do this is [SmartObjectSync](https://github.com/MMMaellon/SmartObjectSync).

___

## Dependencies

* Cyan Player Object Pool [VCC](https://cyanlaser.github.io/CyanPlayerObjectPool/), [GitHub](https://github.com/CyanLaser/CyanPlayerObjectPool)
* VRRefAssist [VCC](https://livedimensions.github.io/VRRefAssist/), [GitHub](https://github.com/LiveDimensions/VRRefAssist)

___

## Installation instructions

Before doing anything else, please make a backup of your project.
You can easily do this with the Creator Companion.

1. Add `Cyan Player Object Pool` and `VRRefAssist` to your project through your method of choice.
2. Add the `Floating Origin` package to your project via one of the options below.
3. In the `VRC Scene Descriptor` that can be found on the `VRCWorld` object you need to set the `Respawn Height Y` to some really large negative number such as `-100000`. This is to prevent the player and pickups from constantly respawning when you fly or fall quickly.
4. On the top bar in Unity `KitKat > JetSim > Floating Origin > Install`
5. You now have a `WorldParent` object in your scene. You will need to parent all the objects that players will be able to see, walk on, interact with, etc. to this object.
6. If you are using Unity 2022 you can mark the `WorldParent` object as the [default parent](https://vrclibrary.com/wiki/books/whats-new-in-unity-2022/page/set-any-gameobject-as-default-parent) to make your life easier.

### Option 1: VCC

* Add `Floating Origin` to the creator companion.
    `Floating Origin` is part of the [JetSim VCC listing](https://kitkat4191.github.io/JetSim-VCC-Listing/).

### Option 2: Unity Package Manager

* On the top bar in Unity `Window > Package Manager`.
* Click the `[+]` in the top left.
* Select `Add package from git URL`.
* Paste this link: `https://github.com/KitKat4191/JetSim-FloatingOrigin.git`.

### Option 3: Unitypackage

* Download the unitypackage from the [latest release](https://github.com/KitKat4191/JetSim-FloatingOrigin/releases/latest).
* Drag the unitypackage from your downloads folder to the `Project` tab in your open Unity project.

___

### General Info

For performance reasons It's important that you only parent objects that actually need to move with the world. I recommend that you also put objects that are close together in the world under a common parent. This is to reduce the amount of direct child objects on the `WorldParent`.

___
