
# JetSim - Floating Origin

___

![JetSim](https://raw.githubusercontent.com/KitKat4191/JetSim-VCC-Listing/main/Website/banner.png)

___

## ⚠ Using this prefab has implications ⚠

**Anything that relies on world space may not function at all, or just produce undesirable results.**

Examples:

* Static batching
* Occlusion culling
* VRCObjectSync
* Specific shaders

When syncing the position of an object you should do so relative to the `Anchor` object. `Anchor` is the first child of `WorldParent`.

One such system that can do this is [SmartObjectSync](https://github.com/MMMaellon/SmartObjectSync).

___

## Dependencies

* VRRefAssist [VCC](https://livedimensions.github.io/VRRefAssist/), [GitHub](https://github.com/LiveDimensions/VRRefAssist).

___

## Installation instructions

* Before doing anything else, please make a backup of your project. You can easily do this with the Creator Companion.
* `VCC > Projects > Manage Project > The \/ arrow to the right of Open Project > Make Backup`.
* The superior option is to just use version control software such as [Git](https://git-scm.com/book/en/v2/Getting-Started-What-is-Git%3F). This [VRC Library page](https://vrclibrary.com/wiki/books/lightbulbs-tutorials-tips-tricks/page/putting-unity-projects-in-github-for-ez-sharing-backups) explains how to get started.

___

1. Add `VRRefAssist` to your project through your method of choice.
2. Add the `Floating Origin` package to your project via one of the options below.
3. On the top bar in Unity click `KitKat > JetSim > Floating Origin > Install`.

### Option 1: VCC

* Add `Floating Origin` to the creator companion. `Floating Origin` is part of the [JetSim VCC listing](https://kitkat4191.github.io/JetSim-VCC-Listing/).
* `VCC > Projects > Manage Project > JetSim - Floating Origin > Add package (+)`.

### Option 2: Unity Package Manager

* On the top bar in Unity click `Window > Package Manager`.
* Click the `[+]` in the top left of the `Package Manager` window.
* Select `Add package from git URL...` in the dropdown menu.
* Paste this link: `https://github.com/KitKat4191/JetSim-FloatingOrigin.git`
* Click `Add` on the right side of the link input field.

### Option 3: `.unitypackage`

* Download the `.unitypackage` from the [latest release](https://github.com/KitKat4191/JetSim-FloatingOrigin/releases/latest).
* Drag the `.unitypackage` from your downloads folder to the `Project` tab in your open Unity project.

___

### General Info

* The `VRC Scene Descriptor` that can be found on the `VRCWorld` object now has its `Respawn Height Y` set to `-1000000`. This is to prevent the player and pickups from constantly respawning when you travel downward quickly.
* You now have a `WorldParent` object in your scene. You will need to parent all the objects that players will be able to see, walk on, interact with, etc. to this object.
* You can mark the `WorldParent` object as the [default parent](https://vrclibrary.com/wiki/books/whats-new-in-unity-2022/page/set-any-gameobject-as-default-parent) to make your life easier.
* For performance reasons It's important that you only parent objects to the `WorldParent` that actually need to move with the world.
* I recommend that you put objects that are close together in the world under a common parent. This is to reduce the amount of direct child objects on the `WorldParent`. Which in turn reduces how many objects I need to iterate over in U#.

___

<script type='text/javascript' src='https://storage.ko-fi.com/cdn/widget/Widget_2.js'></script><script type='text/javascript'>kofiwidget2.init('Support me on Ko-fi', '#72a4f2', 'W7W1N6X22');kofiwidget2.draw();</script>