# Disclaimer!!!
VRChat does **not** condone the use of mods, and, if found to be modifying the client, then you **will** be punished.

**That being said,** there is no anticheat of any sort (file integrity check, position check, etc.) placed on the client.
The only anticheat (if it could be called that) in VRChat is an API limiter, and Photon checks, and the mods listed in this repository, will not trigger any of those.
The real only way to get punished on this game is to piss of the aforementioned anticheats, or be reported by a user with video evidence of you using a mod (whether this be you flying or verbally admitting you mod the game).

So just, stay safe, and don't be stupid and run around saying you use mods, and you'll be a-ok.

Also, for my sake, **I am not responsible for any bans using my mods may cause**. 
I do my abselute best to make sure none of my mods, verified or unverified, will increase your chance of getting banned by any amount.
And in fact, only one of my mods could trigger any anticheat in any way (UserInfoExtensions), but there are limits in place that completely prevent this.
**But**, no matter how small, there is always a chance. 

# Installation 
1. Simply follow the instructions on the [MelonLoader wiki](https://melonwiki.xyz/#/) on installing MelonLoader **0.4.0** (MelonLoader is the mod loader which will allow my mods to run). 
2. Make sure you've installed version 0.4.0, as 0.2.7.4 will not function with VRChat.
3. Then download the mod(s) you would like to install from the [releases](https://github.com/loukylor/VRC-Mods/releases) section of this repository.
4. Allow the game to run once (this will set up a bunch of things MelonLoader uses)
5. And finally, drag and drop the downloaded mod(s) into the newly created `Mods` folder in the `VRChat` folder and restart the game.
More detailed instructions and more mods can be found in the [VRChat Modding Group Discord](https://discord.gg/rCqKSvR).

# Mod List
- [AskToPortal](#asktoportal)
- [AvatarHider](#avatarhider)
- [ChairExitController](#chairexistcontroller)
- [CloningBeGone](#cloningbegone)
- [InstanceHistory](#instancehistory)
- [PlayerList](#playerlist)
- [PreviewScroller](#previewscroller)
- [PrivateInstanceIcon](#privateinstanceicon)
- [ReloadAvatars](#reloadavatars)
- [RememberMe](#rememberme)
- [SelectYourself](#selectyourself)
- [TriggerESP](#triggeresp)
- [UserHistory](#userhistory)
- [UserInfoExtensions](#userinfoextensions)
- [VRChatUtilityKit](#vrchatutilitykit)

# AskToPortal
A mod that makees sure you want to enter a portal, every time you enter a portal

## Features
This mod also contains many checks for portal droppers, or people who use a mod that drops portals maliciously.
If the mod detects a portal dropper, it will give you the option of blacklisting the user until you restart your game.

You can also toggle the mod on and off and auto accept portals from friends, yourself, and one's placed in the world itself (by the creator).

## Picture of user prompt:
![Basic User Prompt](https://i.imgur.com/IiOnkCM.png)
![Detailed User Prompt](https://i.imgur.com/N4QHlbb.png)
![Basic User Prompt with Errors](https://i.imgur.com/fja7qNY.png)
![Detailed User Prompt with Errors](https://i.imgur.com/SJPALdl.png)

## Requirements
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# AvatarHider
The following is the original README:

This mod will automatically hide avatars based on the distance away from you.
There's no real reason to render avatars that you don't even pay attention to, right?

For the best experience, it is recommended to run this mod with UIExpansionKit.
"Hide Distance" is customizable and can be changed in meters (default is 7 meters).
Friend's avatars are ignored by default, but can be hidden by distance too if needed.
"Exclude Shown Avatars" will ignore to hide a persons avatar if you are showing the avatar.
"Disable Spawn Sounds" will only prevent a spawn sound from replaying when the avatar becomes visable again.

Tip:
If a friend is using an unoptimized avatar and you would like AvatarHider to hide it, disable "Ignore Friends" and enable "Exclude Shown Avatars".
Then show your friends avatars that you would like to be ignored by AvatarHider.
And set the friend with the unoptimized avatar to the "Use Safety Settings" in the QuickMenu.

## Additions in my Version
I spent lots of time to make sure the mod runs well, and also can be more snappy.
There is no longer any delay to hide an avatar and also there should be less performance hit in general.

The other differences are all audio related.
The "Disable Spawn Sounds" setting will now allow an avatar's spawn sounds to run once. This will only run for those who aren't always shwon.
There are new settings "Max Audio Distance" and "Limit Audio Distance" that allow you to limit the max distance of an avatar's audio.
This runs for all players.

**Warning:** Limiting audio distance may break VRChat's spatialized audio sources.

The last addition is there is now an option to always include hidden avatars, even if they're a friend and you set AvatarHider to ignore friends.

## Credits
This is a revived version of the mod [AvatarHider](https://github.com/ImTiara/AvatarHider), originally made by [ImTiara](https://github.com/ImTiara) so huge credits to them!
In the previous repository, both [dave-kun](https://github.com/dave-kun) and [Brycey92](https://github.com/Brycey92) were contributor so credits to them too.

## Requirements
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# ChairExitController
Prevents you from falling out of chairs accidentally.
Press both triggers in VR, or q and e in desktop to leave chairs.

# CloningBeGone
Turns off cloning when you join an instance.

## Features
You can configure whether you want cloning to be on or off based off instance type.
So for example, you can have cloning on in Invite+ worlds and off in all the other instance types.

You can also disable/enable cloning for a specific avatar. The buttons to control these can be toggled on and off.
Keep in mind however, that this requires the use of UIX and will overwrite the instance type cloning.

## Requirements
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# InstanceHistory
A basic instance history mod

## Features
It has an optinal dependency for UIX when opening the instance history menu. This means you can run with or without it.
It is highly recommnded to use UIX because it's just easier, although there are preferences to change the position of the regular button if you don't like UIX.

## Requirements
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# PlayerList
Adds a player list to the ShortcutMenu

## Features
Each entry to the player list is a button that will open the user in the QuickMenu on click.
The player's name will be colored to the rank they are (OGTrustRanks compatible!), each entry also has the player's ping, fps, platform, avatar performance, distance from you, and number of owned objects.
Note that the number of owned objects will be inaccurate in instances where you're alone and also when you first join an instance.
You may also toggle each of these on and off.
So, if you don't like how the avatar performance takes up space, you can turn it off.

Note: distance from you will be disabled in worlds that do not allow risky functions.

There is also a list of info about the game and world you are in.
It lists:
- Time since joining the instance (Room Time)
- System time in 12hr format and 24hr format 
- Game build number (Game Version)
- Position in world (Coordinate Position)
- World Name
- World Author Name
- Instance Master (The person who get the host glitch)
- Instance Creator (The person who has moderation powers in the instance, only applicable to non-public instances)
- Whether risky functions are allowed or not.

And Each of these can be individually toggled on or off.

Now for more customizable things, you can change fontsize, the list's position (the QuickMenu hitbox will scale automatically), and the PlayerList button position.
You can also change the color of the name, so instead of showing trust and friends, you could show friends only, or trust only, or just none.
The list can also be numbered, or ticked and can be condensed so more stuff fits on one line.

The list may also be turned off on startup, and can always be toggled on using `left ctrl + f1`

I also plan to add more entries, please ping or DM with ideas. I don't bite!
Oh yea, also report any errors to me. You can make an issue or ping/DM me.

## Credits
- [KortyBoi](https://github.com/KortyBoi) as he let me use the layout from his player list, and helped me with getting some of the information.
- Frostbyte for being a big meanie and telling me how to optimize things

## Picture of List
![Picture of the List](https://i.imgur.com/jvfytTc.png)

## Requirements
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# PreviewScroller
A mod that let's you sort of scroll the avatar preview so you can control where it's facing

## GIF of Scrolling
![GIF of the Scrolling in Action](https://i.imgur.com/D2JVwnD.mp4)

## Requirements
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# PrivateInstanceIcon
Adds an icon to the social menu that shows if you can join a person or not.

## Features
Let's you configure whether you want the icon on people that are on join me, but in privates.<br>
You can also just straight up hide users from the list that you cant join. Note that when the list refreshes, the hidden users might pop up for a frame.<br>
There is also a setting to include/exclude users from your favorites lists.

## Picture of the Icon
![Picture of me with the icon on it](https://i.imgur.com/bLgOC5R.png)

# ReloadAvatars
Adds buttons to reload a single user's avatar or all users' avatar.

## Features
The buttons can each be toggled on and off using UIX

## Requirements
 - [UIExpansionKit](https://github.com/knah/VRCMods/)
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# RememberMe
Mod for VRChat using MelonLoader

Adds a "Remember Me" check-box to the Login screen.
When "Remember Me" is checked off it will auto-fill the last saved VRChat Credentials.

## Disclaimer
I have not made any changes to this mod. All code is not my own, and I have received permission for this.

## Credits
- The original author of the mod, [HerpDerpinstine](https://github.com/HerpDerpinstine)
- A contributor on the original repository, [dave-kun](https://github.com/dave-kun)

# SelectYourself
Adds a button that allows you to select yourself

## Features
The button can be toggled on and off using UIX

## Requirements
 - [UIExpansionKit](https://github.com/knah/VRCMods/)

# TriggerESP
A trigger ESP that will highlight all VRChat interactables as well as any Unity UI buttons.

## Features
The color the ESP is customizable, and you can also set the color as random.
The strength of the ESP is customizable as well.

Note that it disables itself in worlds that don't allow risky functions.

## Picture of the ESP
![Picture of Outline](https://i.imgur.com/QnawlKb.jpg)
![Picture of Wireframe](https://i.imgur.com/nnTN4na.jpg)

## Requirements
 - [UIExpansionKit](https://github.com/knah/VRCMods/)
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

## Credites
 - The shader used was a heavily modified version of the "Distance Fade Outline Shader" found in https://github.com/netri/Neitri-Unity-Shaders

# UserHistory
A mod that shows you when a user joined, and when clicking on them, opens when in the user page.
It's basically a copy + paste of [InstanceHistory](#instancehistory)

## Features
It has an optinal dependency for UIX when opening the user history menu. This means you can run with or without it.
It is highly recommnded to use UIX because it's just easier, although there are preferences to change the position of the regular button if you don't like UIX.

## Requirements
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# UserInfoExtensions
A mod that adds buttons to the to make VRChat more convenient

## Features
Adding individually toggleable buttons that allow you to:
 - Select a user in the Quick Menu from the Social Menu page.
 - Find the avatar's author from the Social Menu and Avatar Menu pages.
 - Open the links the selected user has in their bio.
 - Display the languages the selected user has in their bio.

The buttons can always be accessed in a popup attached to the User Details Page.

Additionally, in the popup you can see the user's:
 - username (what the person logs in with)
 - platform (Quest or PC)
 - last login (literal login, not starting the game)
 - date joined (date original unmerged account created)
 - friend number (the number friend they are. like 1st friend, 2nd friend, etc.)

For avatars, you can see their:
 - author's name
 - name
 - supported platforms
 - release type
 - time which they were last updated
 - version

## Requirements
 - [UIExpansionKit](https://github.com/knah/VRCMods/)
 - [VRChatUtilityKit](https://github.com/loukylor/VRC-Mods/releases)

# VRChatUtilityKit
Various sets of utilites for developers to use in their mods.

## For Developers
If you wish to use this mod, please respect the license, LGPL v3. You can read more below.
The source is documented, and the XML file is included in the release.
To utilise the XML file, just put it in the same directory as the copy of the utility kit you are referencing.

## Licensing
This library is licensed under LGPL v3.
This means that you are allowed to reference the library in your code as long as you disclose source and have a license and copyright notice you will be fine.
In the case that you would like to modify or include the library in your mod, you must use the same license as well as state any changes.

If you are caught not properly following the license, I will not hesitate to take you repo or Discord account down.

Also note that I have used code licensed under GPL v3, however, I have been granted express permission to license this code under LGPL v3.

# Credits
- [knah](https://github.com/knah) as I use [Join Notifier's](https://github.com/knah/VRCMods) join/leave and asynchronous utilities.
- [DubyaDude](https://github.com/DubyaDude) as I used [RubyButtonAPI](https://github.com/DubyaDude/RubyButtonAPI) as reference for my button API.
- [Psychloor](https://github.com/Psychloor) as I used his code as a template for my risky functions check.