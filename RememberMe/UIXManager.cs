using UIExpansionKit.API;

namespace RememberMe
{
    class UIXManager
    {
        public static void AddMethodToUIInit()
        {
            ExpansionKitApi.OnUiManagerInit += RememberMe.Instance.OnUiManagerInit;
        }
    }
}
