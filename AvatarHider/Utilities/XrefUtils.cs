using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace AvatarHider.Utilities
{
    class XrefUtils
    {
        public static bool CheckMethod(MethodInfo method, string match)
        {
            try
            {
                return XrefScanner.XrefScan(method)
                    .Where(instance => instance.Type == XrefType.Global && instance.ReadAsObject().ToString().Contains(match)).Any();
            }
            catch { }
            return false;
        }
    }
}
