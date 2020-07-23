**DISCLAIMER**: This project is an effort to decompile and improve an existing plugin that was not written by me.  
The original plugin can be found here: https://vr-erogamer.com/archives/332

# KK_CyuVR
> Adds kissing functionality in Koikatsu official VR  


## Prerequisites  
- Afterschool DLC  
- [Koikatsu Official VR Patch 0531](http://www.illusion.jp/preview/koikatu/download/vr.php)  
- BepInEx 4 and above (to run it with BepInEx 5 you'd need [BepInEx 4 Patcher](https://github.com/BepInEx/BepInEx.BepInEx4Upgrader), included within HF Patch)  
- (optional, strongly recommended): [CrossFader](https://github.com/MayouKurayami/KK_CrossFader/releases) plugin
- (optional, recommended): [KoikatuVRAssistPlugin](https://mega.nz/#!YQZyWRwQ!C2FX0Iwp-X7F5z55ytTlQGkjfqH6kQP-wcDPfNBvT0s) plugin



## Installation
[Download **KK_CyuVR zip** from the latest release](https://github.com/MayouKurayami/KK_CyuVR/releases) then extract it into your game directory (where the game exe and BepInEx folder are located).  
Replace old files if asked.  
*KK_CyuVR.dll* should end up in the BepInEx root directory.  

Installation of the [**CrossFader.dll**](https://github.com/MayouKurayami/KK_CrossFader/releases) plugin is strongly recommended to ensure smooth animation in and out of kissing.   

Also recommended is the [**KoikatuVRAssistPlugin**](https://mega.nz/#!YQZyWRwQ!C2FX0Iwp-X7F5z55ytTlQGkjfqH6kQP-wcDPfNBvT0s) plugin found on https://vr-erogamer.com/archives/322, for easier movement and better access to actions in VR. With it installed, one can rotate and move the camera at all times by holding the trigger button. While the UI menu is visible, hold the grip button for 1 second to freeze it in place, and drag and move it by holding the grip button. Double click the grip button to return the menu to your controller.

## Configurations
Configurations are located in *config.ini* in the BepInEx root folder, under section **[bero.cyu.cyuvr]**.  

**It is recommended to adjust the configs via the in-game plugin settings page instead of directly editing the config file.  
Press *F1* when not in VR to access the plugin settings at the upper right of the screen.**  
![](https://github.com/MayouKurayami/KK_CyuVR/blob/master/images/CyuVR_settings.png)  


- **Eyes Animation Openness (EyesMovement)** - Maximum openness of eyes and eyelids during kissing. Set to 0 to keep eyes closed during kiss **(0-100, Default: 50)**  

- **Force Allow Kiss (ForceKiss)** - Allow kissing even if the girl is set to refuse kiss **(Default: False)**

- **Girl Neck Elevation (KissNeckAngle)** - Head elevation of the female character during kissing. **(Default: 0.2)**   

- **Increase Kiss Intensity by Groping (GropeOverride)** - If enabled, kissing motion speed in caress mode will depend on the intensity of touching and dragging the girl, ignoring the maximum value set by *KissMotionSpeed*. **(Default: true)**  

- **Kiss Activation Distance (KissDistance)** - Distance within which to start kissing when not in caress mode. Unit in meters approximately **(Default: 0.18)**  

- **Kiss Activation Distance in Caress Mode (KissDistanceAibu)** - Distance within which to start kissing in caress mode. Unit in meters approximately **(Default: 0.28)**  

- **Kiss Intensity in Caress Mode (KissMotionSpeed)** - Default speed of kissing motion in caress mode.

- **Mode of Tongue and Mouth Movement (MouthMovement)** - Set when to enable/disable tongue and mouth movement (french kiss) in kissing. **Auto** will enable tongue and mouth movement when character is in "lewd" state OR when female character's excitement gauge is above 70 (climax threshold). **ForceOn** will enable tongue and mouth movement at all times, and **ForceOff** will disable it completely. **(Default: Auto)**  

- **Player Mouth Offset (MouthOffset)** - *Negative* vertical offset to player's mouth (increase this value to make your own mouth lower). Affects calculation of the kiss activation distances and attachment point of saliva string. **(Default: 0.12)**  

### Keyboard Shortcuts  

 - **Enable/Disable CyuVR (PluginToggleKey)** - Press this key to enable/disable the plugin. Plugin will always re-enable after changing position. **(Default: None)**


## Notes and Limitations
- Unknown compatibility with Koikatsu Party (Steam release)  

- Unknown compatibility with kPlug.

- Currently does not work in the unofficial VR mod for main game.  

- In **caress mode**, the girl will lean forward to initialize kissing. Approach the girl slowly to avoid ~~bumping~~ clipping into her head.  

- This plugin will only properly work in situations where it's possible to kiss as in the non-VR version of the game. Therefore, it would not work well in 3P, Darkness mode...etc.  

## Credits
All credit of the plugin up to version 0.0.4 goes to the unknown developer who made this plugin.  
