using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// A set of utilites for Xref scanning.
    /// </summary>
    public static class XrefUtils
    {

        /// <summary>
        /// Returns if a string is contained within the given method's body.
        /// </summary>
        /// <param name="method">The method to check</param>
        /// <param name="match">The string to check</param>
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

        /// <summary>
        /// Returns if the given method is called by the other given method.
        /// </summary>
        /// <param name="method">The method to check</param>
        /// <param name="methodName">The name of the method that uses the given method</param>
        /// <param name="type">The type of the method that uses the given method</param>
        public static bool CheckUsedBy(MethodInfo method, string methodName, Type type = null)
        {
            foreach (XrefInstance instance in XrefScanner.UsedBy(method))
            {
                if (instance.Type == XrefType.Method)
                {
                    try
                    {
                        if ((type == null || instance.TryResolve().DeclaringType == type) && instance.TryResolve().Name.Contains(methodName))
                            return true;
                    }
                    catch
                    {

                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns whether the given method is using another the other given method.
        /// </summary>
        /// <param name="method">The method to check</param>
        /// <param name="methodName">The name of the method that is used by the given method</param>
        /// <param name="type">The type of the method that is used by the given method</param>
        public static bool CheckUsing(MethodInfo method, string methodName, Type type = null)
        {
            foreach (XrefInstance instance in XrefScanner.XrefScan(method))
            {
                if (instance.Type == XrefType.Method)
                {
                    try
                    {
                        if ((type == null || instance.TryResolve().DeclaringType == type) && instance.TryResolve().Name.Contains(methodName))
                            return true;
                    }
                    catch
                    {

                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Dumps the Xref information on a method.
        /// This is for DEBUG PURPOSES ONLY.
        /// </summary>
        /// <param name="method">The method to dump information on</param>
        public static void DumpXrefInfo(MethodBase method)
        {
            MelonLogger.Msg(ConsoleColor.Yellow, $"Scanning {method.Name}");
            
            MelonLogger.Msg(ConsoleColor.Yellow, $"Checking UsedBy");
            DumpScan(XrefScanner.UsedBy(method));
            
            MelonLogger.Msg(ConsoleColor.Green, "Checking Using");
            DumpScan(XrefScanner.XrefScan(method));
        }

        private static void DumpScan(IEnumerable<XrefInstance> scan)
        {
            foreach (XrefInstance instance in scan)
            {
                if (instance.Type == XrefType.Global)
                    MelonLogger.Msg(instance.ReadAsObject().ToString());

                MelonLogger.Msg(instance.Type);

                MethodBase resolvedMethod = instance.TryResolve();
                if (instance.Type == XrefType.Method)
                {
                    if (resolvedMethod == null)
                    {
                        MelonLogger.Msg("null");
                        MelonLogger.Msg("null");
                    }
                    else
                    {
                        MelonLogger.Msg(resolvedMethod.Name);
                        MelonLogger.Msg(resolvedMethod.DeclaringType.Name);
                    }

                    MelonLogger.Msg("");
                }
            }
        }
    }
}
