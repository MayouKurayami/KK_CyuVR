**DISCLAIMER**: This project is an effort to decompile and improve an existing plugin that was not written by me.  
The original plugin can be found here: https://vr-erogamer.com/archives/454

# KK_CyuVR
> Adds kissing functionality in Koikatsu official VR  


## Prerequisites  
- Afterschool DLC  
- BepInEx 4 and above  
- Unknown compatibility with Koikatsu Party (Steam release)  


## Installation
Place *KK_CyuVR.dll* in BepInEx root folder.  
This is a BepInEx 4 plugin, to run it with BepInEx 5 you'd need [BepInEx 4 Patcher](https://github.com/BepInEx/BepInEx.BepInEx4Upgrader)

Included within the release is another plugin (*CrossFader.dll*) that smoothes transitions between animations.  
Installation of this plugin is strongly recommended to ensure smooth animation in and out of kissing.   
(Install by placing in BepInEx root folder alongside KK_CyuVR.dll)
  
## Configurations
Configurations are located in *config.ini* in the BepInEx root folder, under section **[Cyu]**:
- **KissDistance** - Distance within which to start kissing outside of caress mode. **(Default: 0.18)**  
- **KissDistanceAibu** - Distance within which to start kissing in caress mode. **(Default: 0.35)**  
- **MouthOffset** - *Negative* vertical offset to player's mouth (increase this value to make your own mouth lower). Affects calculation of the above distances and attachment point of saliva string. **(Default: 0.12)**  
- **KissNeckAngle** - Head elevation of the female character during kissing. **(Default: 0.2)**   
- **KissMotionSpeed** - Speed of kissing motion in caress mode. **(Default: 0.1)**  
- **EyesMovement** - Enables eye and eyelids movement during kissing. **(Default: true)**  
- **TongueOverride** - By default the plugin adds mouth and tongue movement when excitement gauge is above 70 ***or*** the character is at "Lewd" state. This config overrides these conditions and enables mouth and tongue movement all the time. **(Default: false)**  

## Notes and Limitations
- In **caress mode**, the girl will lean forward to intialize kissing. Approach the girl slowly to avoid ~~bumping~~ clipping into her head!  
- This plugin will only enable kissing in situations where it's possible to kiss as in the non-VR version of the game. Therefore, it would not work in 3P, Darkness mode...etc..  

## Credits
All credit of the plugin up to version 0.0.4 goes to the unknown developer who made this plugin.  
