# [Kingdom Hearts 2.5 HD Remix](remasters/15plus25/index.md) - KH2FM+ Debug Menu

The E3 2014 Kiosk Demo for Kingdom Hearts 2.5 HD Remix has the developer debug menu enabled for KH2FM+. The menu works but causes crashes when attempting to use most functions intended for use with the complete game data.
	By replacing the demo `index.dat` and `kingdom2.mself` with the files from a retail copy of the game it is possible to restore most missing functionality.

# Debug Menu Controls and Setup
To access the debug menu on a PS3 devkit or emulator right-click on the screen with a connected mouse. To select an option left-click it in the menu. It is recommended to only use the debug menu _after_ the title screen to avoid crashes.
	The following steps are recommended to improve useability: 
	
1. Click `SUBMENU`, `VSYNC FRAME LIMIT`, and choose `60`. This will set the frame rate to 60fps and improve mouse movement significantly when navigating the debug menu.
	
2. Click `CONFIG`, `EDIT`, and check to see if `SHOW VERSION` is enabled (it is enabled if the check box on the left is dark grey instead of light grey). If enabled, you will not access the normal pause menu when pressing start even if outside of battle. You can disable this to restore normal pausing. 
		
# Detailed Breakdown of Debug Menu (Top-level)
## HIDE DEBUG
Self-explanatory.
	
## STATUS
Displays various realtime diagnostics.
		
## CONFIG >>
Various useful setting under ``EDIT``.
	
## SUBMENU ON/OFF >>
Allows docking of submenus in the top-level menu.
		
## SETINFO
	
## MAPJUMP 
This can be used to warp to any map in the game. More documentation to come. 
For internal map names refer to [KH2 Worlds](../kh2/worlds.md)
By changing ``EVENT`` user can warp to various boss fights, scripted fights, and cutscenes.
		
## LOCALSET >>
	
## PROGRESS  >>
	
## SIGNAL
	
## ITEM
Inventory editor. More documentation to come.
		
## DEBUG DRAW >>
Disabled graphics debugging functions. These were used for developing the PS2 version but seem to have not been used for the PS3 port.
		
## MISSION >>
	
## OBJENTRY >>
Opens a menu allowing the user to change playable characters and party members and spawn objects/enemies/bosses etc.

## CHECK CACHEBUFFER >>
	
## CASH SAVE

# Submenus
## DEBUG

## SYSTEM

## FIELD

## BATTLE

## EVENT

## MAP
Free camera mode can be activated here. Free camera will cause a crash in battles if game is not paused. Right analog controls camera angle, left stick moves camera horizontally. Hold L1 to switch to vertical movement.

## WORLD

## GUMI BATTLE

## GUMI EDIT
Nothing

## MENU

## EFFECT
Allows user to center camera on active visual effects. Contains menus for effect editors. These crash when attempting to load an effect file.

## SOUND

## ETC

## YASUI
Mostly unknown options at this point. ``TEST`` will kill Sora.

## SUGI
Nothing.

## TOZAWA
For testing menus. Options include opening the ``SAVE``, ``LOAD``, and ``PARTY`` menus, changing game difficulty, giving oneself max synthesis materials, and max munny. 

## ITO
Allows user to play various minigames. 

## SAVE POINT

## TREASURE BOX
