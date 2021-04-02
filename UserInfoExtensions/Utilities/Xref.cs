using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace UserInfoExtentions.Utilities
{
    class Xref
    {
        // This method is practically stolen from https://github.com/BenjaminZehowlt/DynamicBonesSafety/blob/master/DynamicBonesSafetyMod.cs
        public static bool CheckMethod(MethodBase methodBase, string match)
        {
            try
            {
                return XrefScanner.XrefScan(methodBase)
                    .Where(instance => instance.Type == XrefType.Global && instance.ReadAsObject().ToString() == match).Any();
            }
            catch { }
            return false;
        }
        public static bool CheckUsed(MethodBase methodBase, string methodName)
        {
            try
            {
                return XrefScanner.UsedBy(methodBase)
                    .Where(instance => instance.TryResolve() != null && instance.TryResolve().Name.Contains(methodName)).Any();
            }
            catch { }
            return false;
        }
    }
}
