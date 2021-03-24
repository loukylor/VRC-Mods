using System.Linq;
using System.Reflection;
using Harmony;
using MelonLoader;
using UnhollowerBaseLib.Attributes;

[assembly: MelonInfo(typeof(ClickFix.ClickFixMod), "ClickFix", "1.0.1", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ClickFix
{
    public class ClickFixMod : MelonMod
    {
        public static MethodInfo upGetter;
        public override void VRChat_OnUiManagerInit()
        {
            MethodInfo[] possibleGetters = typeof(VRCInput).GetProperties()
                .Where(pi => pi.Name.StartsWith("prop_Boolean_") && pi.SetMethod == null)
                .Select(pi => pi.GetMethod)
                .OrderBy(mb => mb.GetCustomAttribute<CallerCountAttribute>().Count)
                .ToArray();

            upGetter = possibleGetters[1]; // the up bool (the one that works at below 20fps) is the 2nd lowest caller count

            Harmony.Patch(possibleGetters[0], new HarmonyMethod(typeof(ClickFixMod).GetMethod(nameof(OnGetUp)))); // the up bool that doesnt is the lowest caller count
        }
        public static bool OnGetUp(VRCInput __instance, ref bool __result)
        {
            __result = __instance.prop_Boolean_1;
            return false;
        }
    }
}
