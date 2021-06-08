using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MelonLoader;

[assembly: MelonInfo(typeof(VRChatUtilities.VRCUtilitiesMod), "VRCUtilities", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRChatUtilities
{
    class VRCUtilitiesMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            if (typeof(MelonMod).GetMethod("VRChat_OnUiManagerInit") == null)
                MelonCoroutines.Start(GetAssembly());
        }

        private static IEnumerator GetAssembly()
        {
            Assembly assemblyCSharp = null;
            while (true)
            {
                assemblyCSharp = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == "Assembly-CSharp");
                if (assemblyCSharp == null)
                    yield return null;
                else
                    break;
            }

            MelonCoroutines.Start(WaitForUiManagerInit(assemblyCSharp));
        }
        private static IEnumerator WaitForUiManagerInit(Assembly assemblyCSharp)
        {

            Type vrcUiManager = assemblyCSharp.GetType("VRCUiManager");
            PropertyInfo uiManagerSingleton = vrcUiManager.GetProperties().First(pi => pi.PropertyType == vrcUiManager);

            Type quickMenu = assemblyCSharp.GetType("QuickMenu");
            PropertyInfo quickMenuSingleton = quickMenu.GetProperties().First(pi => pi.PropertyType == quickMenu);

            while (uiManagerSingleton.GetValue(null) == null)
                yield return null;

            while (quickMenuSingleton.GetValue(null) == null)
                yield return null;

            foreach (MelonMod mod in MelonHandler.Mods)
                mod.GetType().GetMethod("VRChat_OnUiManagerInit")?.Invoke(mod, null);
        }
    }
}
