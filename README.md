# Disclaimer!!!
VRChat does **not** condone the use of mods, and, if found to be modifying the client, then you **will** be punished.

**That being said,** there is no anticheat of any sort (file integrity check, position check, etc.) placed on the client.
The only anticheat (if it could be called that) in VRChat is an API limiter, and Photon checks, and the mods listed in this repository, will not trigger any of those.
The real only way to get punished on this game is to piss of the aforementioned anticheats, or be reported by a user with video evidence of you using a mod (whether this be you flying or verbally admitting you mod the game).

So just, stay safe, and don't be stupid and run around saying you use mods, and you'll be a-ok.

# Installation 
1. Simply follow the instructions on the [MelonLoader wiki](https://melonwiki.xyz/#/) on installing MelonLoader (MelonLoader is the mod loader which will allow my mods to run). 
2. Then download the mod(s) you would like to install from the [releases](https://github.com/loukylor/VRC-Mods/releases) section of this repository.
3. And finally, drag and drop the downloaded mod(s) into the newly created `Mods` folder in the `VRChat` folder.
More detailed instructions and more mods can be found in the [VRChat Modding Group Discord](https://discord.gg/rCqKSvR).

# Mod List
- [AskToPortal](#asktoportal)
- [CloningBeGone](#cloningbegone)
- [PlayerList](#playerlist)
- [UserInfoExtensions](#userinfoextensions)

# AskToPortal
A mod that makees sure you want to enter a portal, every time you enter a portal

## Features
This mod also contains many checks for portal droppers, or people who use a mod that drops portals maliciously.
If the mod detects a portal dropper, it will give you the option of blacklisting the user until you restart your game.

You can also toggle the mod on and off and auto accept portals from friends, yourself, and one's placed in the world itself (by the creator).

## Disclaimer
AskToPortal is NOT Portal Confirmation by 404. AskToPortal is simply a replacement for Portal Confirmation as 404 was banned from the VRChat Modding Group and AskToPortal will NOT work in conjunction with Portal Confirmation.

## Picture of user prompt:
![Image of user prompt](https://i.imgur.com/uvUeUmL.png)

# CloningBeGone
Turns off cloning when you join an instance. It's really that simple lol.

# PlayerList
Adds a player list to the ShortcutMenu

## Features
Adds a player list to the menu that includes player count.
Each entry to the player list is a button that will open the user in the QuickMenu on click.
The player's name will be colored to the rank they are (OGTrustRanks compatible!), each entry also has the player's ping, fps and platform

There is also a list of info about the game and world you are in.
It lists:
- Time since joining the instance (Room Time)
- System time in 12hr format and 24hr format (this will be configurable in the future, I just got lazy)
- Game build number (Game Version)
- Position in world (Coordinate Position)
- World Name
- World Author Name
- Instance Master (The person who get the host glitch)
- Instance Creator (The person who has moderation powers in the instance, only applicable to non-public instances)

Keep in mind to fit all this stuff I had to expand the QuickMenu's hitbox horizontally by a ton, so keep that in mind.

I also plan to make these toggable in the future (and add more please ping or DM with ideas. I don't bite!)
Oh yea, also report any errors to me. You can make an issue or ping/DM me.

## Credits
- [KortyBoi](https://github.com/KortyBoi) as he let me use the layout from his player list, and helped me with getting some of the information
- [knah](https://github.com/knah) as I use [Join Notifier's](https://github.com/knah/VRCMods) join/leave system.

## Picture of List
![Picture of List](https://i.imgur.com/beIZBQX.png)

# UserInfoExtensions
A mod that adds buttons to the to make VRChat more convenient

## Features
Adding individually toggleable buttons that allow you to:
 - Select a user in the Quick Menu from the Social Menu page.
 - Find the avatar's author from the Social Menu and Avatar Menu pages.
 - Selects the world instance of the selected user in the World Menu (if you can join their world).
 - Show the bio of the selected user (bio is different from status).
 - Open the links the selected user has in their bio.
 - Display the languages the selected user has in their bio.
 - Open the selected user in the system's default browser.
 - Open the selected user's avatar in the system's default browser.

The buttons can always be accessed in a popup attached to the User Details Page.

## Requirements
 - [UIExpansionKit](https://github.com/knah/VRCMods/)

## Credits
 - [Ben](https://github.com/BenjaminZehowlt) as I referenced his code a lot to implement Xref Scanning
 - [knah](https://github.com/knah) as he helped me a ton with Async stuff
