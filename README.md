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
1. Simply follow the instructions on the [MelonLoader wiki](https://melonwiki.xyz/#/) on installing MelonLoader **0.3.0** (MelonLoader is the mod loader which will allow my mods to run). 
2. Make sure you've installed version 0.3.0, as 0.2.7.4 will not function with VRChat.
3. Then download the mod(s) you would like to install from the [releases](https://github.com/loukylor/VRC-Mods/releases) section of this repository.
4. Allow the game to run once (this will set up a bunch of things MelonLoader uses)
5. And finally, drag and drop the downloaded mod(s) into the newly created `Mods` folder in the `VRChat` folder and restart the game.
More detailed instructions and more mods can be found in the [VRChat Modding Group Discord](https://discord.gg/rCqKSvR).

# Mod List
- [AskToPortal](#asktoportal)
- [AvatarDownloadPriority](#avatardownloadpriority)
- [AvatarHider](#avatarhider)
- [CloningBeGone](#cloningbegone)
- [InstanceHistory](#instancehistory)
- [PlayerList](#playerlist)
- [PreviewScroller](#previewscroller)
- [ReloadAvatars](#reloadavatars)
- [SelectYourself](#selectyourself)
- [UserInfoExtensions](#userinfoextensions)

# AskToPortal
A mod that makees sure you want to enter a portal, every time you enter a portal

## Features
This mod also contains many checks for portal droppers, or people who use a mod that drops portals maliciously.
If the mod detects a portal dropper, it will give you the option of blacklisting the user until you restart your game.

You can also toggle the mod on and off and auto accept portals from friends, yourself, and one's placed in the world itself (by the creator).

## Picture of user prompt:
![Image of user prompt](https://i.imgur.com/uvUeUmL.png)

# AvatarDownloadPriority
A mod that allows you to prioritize certain downloads and limit the number of avatars downloading at once.

## Features
There are 3 settings that allow you to customize how avatar download priority is handled.
The first prioritizing friends' avatars downloads over other avatars.
The other setting would prioritize favorited friends' avatars over both friends and others' avatars. It will also still prioritize friends over regular users.
The last setting will prioritize your avatar over everyones.

You can also limit the max amount of avatars that can download/load at once.

## Important
If you have any issues, reloading the world you are in should fix them.

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

Adding onto performance increases, is now an option to only disable renderers. This means it will lag less when you walk into a crowd of people and lots of avatar enable at once.
The downside to this though, is that things like dynamic bones, final IK and other things will still calculate and take frametime. Just the avatar will be invisible.

If you prefer to disable the entire avatar (like how AvatarHider worked before), things have mostly stayed the same. 
The only difference is that the "Disable Spawn Sounds" setting will allow an avatar's spawn sounds to run once.

The last addition is there is now an option to always include hidden avatars, even if they're a friend and you set AvatarHider to ignore friends.

## Credits
This is a revived version of the mod [AvatarHider](https://github.com/ImTiara/AvatarHider), originally made by [ImTiara](https://github.com/ImTiara) so huge credits to them!
In the previous repository, both [dave-kun](https://github.com/dave-kun) and [Brycey92](https://github.com/Brycey92) were contributor so credits to them too.

# CloningBeGone
Turns off cloning when you join an instance.

## Features
You can configure whether you want cloning to be on or off based off instance type.
So for example, you can have cloning on in Invite+ worlds and off in all the other instance types.

# InstanceHistory
A basic instance history mod

## Features
It has an optinal dependency for UIX when opening the instance history menu. This means you can run with or without it.
It is highly recommnded to use UIX because it's just easier, although there are preferences to change the position of the regular button if you don't like UIX.

## Credits
- [DubyaDude](https://github.com/DubyaDude) as I used [RubyButtonAPI](https://github.com/DubyaDude/RubyButtonAPI) as reference for my button API.

# PlayerList
Adds a player list to the ShortcutMenu

## Features
Each entry to the player list is a button that will open the user in the QuickMenu on click.
The player's name will be colored to the rank they are (OGTrustRanks compatible!), each entry also has the player's ping, fps, platform, avatar performance and distance from you.
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

And Each of these can be individually toggled on or off.

Now for more customizable things, you can change fontsize, the list's position (the QuickMenu hitbox will scale automatically), and the PlayerList button position.
You can also change the color of the name, so instead of showing trust and friends, you could show friends only, or trust only, or just none.
The list can also be numbered, or ticked and can be condensed so more stuff fits on one line.

The list may also be turned off on startup, and can always be toggled on using `left ctrl + f1`

I also plan to add more entries, please ping or DM with ideas. I don't bite!
Oh yea, also report any errors to me. You can make an issue or ping/DM me.

## Credits
- [KortyBoi](https://github.com/KortyBoi) as he let me use the layout from his player list, and helped me with getting some of the information.
- [knah](https://github.com/knah) as I use [Join Notifier's](https://github.com/knah/VRCMods) join/leave system also I used [Advanced Safety's](https://github.com/knah/VRCMods) native patch.
- [DubyaDude](https://github.com/DubyaDude) as I used [RubyButtonAPI](https://github.com/DubyaDude/RubyButtonAPI) as reference for my button API.
- [Psychloor](https://github.com/Psychloor) as I used his code for the risky functions check.
- Frostbyte for being a big meanie and telling me how to optimize things

## Picture of List
![Picture of List](https://i.imgur.com/jvfytTc.png)

# PreviewScroller
A mod that let's you sort of scroll the avatar preview so you can control where it's facing

# ReloadAvatars
Adds buttons to reload a single user's avatar or all users' avatar.

## Features
The buttons can each be toggled on and off using UIX

## Requirements
 - [UIExpansionKit](https://github.com/knah/VRCMods/)

# SelectYourself
Adds a button that allows you to select yourself

## Features
The button can be toggled on and off using UIX

## Requirements
 - [UIExpansionKit](https://github.com/knah/VRCMods/)

# UserInfoExtensions
A mod that adds buttons to the to make VRChat more convenient

## Features
It adds individually toggleable buttons that allow you to:
 - Select a user in the Quick Menu from the Social Menu page.
 - Find the avatar's author from the Social Menu and Avatar Menu pages.
 - Selects the world instance of the selected user in the World Menu (if you can join their world).
 - Show the bio of the selected user (bio is different from status).
 - Open the links the selected user has in their bio.
 - Display the languages the selected user has in their bio.
 - Open the selected user in the system's default browser.
 - Open the selected user's avatar in the system's default browser.

The buttons can always be accessed in a popup attached to the User Details Page.

Within the popup, you can see the user's:
 - username (what the person logs in with)
 - platform (Quest or PC)
 - last login (literal login, not starting the game) 

## Requirements
 - [UIExpansionKit](https://github.com/knah/VRCMods/)

## Credits
 - [Ben](https://github.com/BenjaminZehowlt) as I referenced his code a lot to implement Xref Scanning
 - [knah](https://github.com/knah) as he helped me a ton with Async stuff
