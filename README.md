# About

I made this mod since I've been enjoying the maps, but I'm not a fan of the ? traps.

Anyways...

This mod does the following

- disables the ? traps from spawning
    - disables score upload (that way we don't pollute the leadorboard with invalid scores)
    - disables score upload **ON ALL TRACKS** if active.

- disables analytics

You can enable/disable any of the above by modifying a config file.

# Installation

This assumes you are running Windows 10, 64-bit

1. Navigate to the Rift of the Necrodancer Steam Folder:
![alt text](image.png)

2. Back up this folder somewhere safe
![alt text](image-2.png)

3. Install BepInEx Mono

    a. download and unzip [BepInEx 6.0.0-be.733](https://builds.bepinex.dev/projects/bepinex_be/733/BepInEx-Unity.Mono-win-x64-6.0.0-be.733%2B995f049.zip)

    ![alt text](image-3.png)

    b. Copy the contents into your Rift of the NecroDancer steam directory
    ![alt text](image-4.png)

4. Install this mod by copying `RiftAllseer.dll` to the plugins directory
![alt text](image-5.png)
![alt text](image-1.png)

5. Launch rift of the necrodancer

6. You should have a console window with info, meaning the mod has been launched successfully:
![alt text](image-6.png)

# Configuration

After the mod is launched for the first time, there is a file in `BepInEx1 > config > RiftAllseer.cfg`

In there you can change these configs:

- enable/disable game analytics (disabled by default)
- enable/disable logging game analytics to console (disabled by default)
- enable/disable ? traps (disabled by default)

> YOU MUST RESTART THE GAME FOR CONFIGURATION CHANGES TO TAKE EFFECT! 


# More details on installing BepInEx:

- docs: https://docs.bepinex.dev/master/articles/user_guide/installation/unity_mono.html

- builds: https://builds.bepinex.dev/projects/bepinex_be

# TODO
- [ ] figure out a non-console way to alert player of config settings
- [ ] unpload scores if the beatmap doesn't have any ? blocks
- [ ] alert player that score won't count after game (maybe text in menu screen or something?)
- [ ] allow player to change config and reload while in game instead of requiring full restart