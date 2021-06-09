using UIExpansionKit.API;

namespace AvatarHider
{
    class UIXManager
    {
        public static void AddMethodToUIInit()
        {
            ExpansionKitApi.OnUiManagerInit += AvatarHiderMod.Instance.OnUiManagerInit;
        }
    }
}
