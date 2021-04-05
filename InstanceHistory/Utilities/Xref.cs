using System.Linq;
using System.Reflection;

namespace InstanceHistory.Utilities
{
    class Xref
    {
        // This method is practically stolen from https://github.com/BenjaminZehowlt/DynamicBonesSafety/blob/master/DynamicBonesSafetyMod.cs
        public static bool CheckMethod(MethodBase methodBase, string match)
        {
            try
            {
                return UnhollowerRuntimeLib.XrefScans.XrefScanner.XrefScan(methodBase)
                    .Where(instance => instance.Type == UnhollowerRuntimeLib.XrefScans.XrefType.Global && instance.ReadAsObject().ToString().Contains(match)).Any();
            }
            catch { }
            return false;
        }
        public static bool CheckUsed(MethodBase methodBase, string methodName)
        {
            try
            {
                return UnhollowerRuntimeLib.XrefScans.XrefScanner.UsedBy(methodBase)
                    .Where(instance => instance.TryResolve() != null && instance.TryResolve().Name.Contains(methodName)).Any();
            }
            catch { }
            return false;
        }
    }
}
