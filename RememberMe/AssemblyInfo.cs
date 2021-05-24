using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;

[assembly: AssemblyTitle(RememberMe.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(RememberMe.BuildInfo.Company)]
[assembly: AssemblyProduct(RememberMe.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + RememberMe.BuildInfo.Author)]
[assembly: AssemblyTrademark(RememberMe.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(RememberMe.BuildInfo.Version)]
[assembly: AssemblyFileVersion(RememberMe.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(RememberMe.RememberMe), RememberMe.BuildInfo.Name, RememberMe.BuildInfo.Version, RememberMe.BuildInfo.Author, RememberMe.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("VRChat", "VRChat")]