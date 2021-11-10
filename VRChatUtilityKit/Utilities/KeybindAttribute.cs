using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

#pragma warning disable IDE0051 // Remove unused private members

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// An attribute that calls the method it is put on when the given key is pressed.
    /// Meant primarily for debugging purposes, but can be utilised for easy keybind creation.
    /// Can only be applied to static methods without parameters.
    /// </summary>
    [MelonLoaderEvents]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class KeybindAttribute : Attribute
    {
        /// <summary>
        /// The key to press to call the attached method.
        /// </summary>
        public KeyCode KeyCode { get; private set; }

        /// <summary>
        /// Creates a new keybind. 
        /// The method the attribute is put on will be called when the given key is pressed.
        /// Meant primarily for debugging purposes, but can be utilised for easy keybind creation.
        /// Can only be applied to static methods without parameters.
        /// </summary>
        /// <param name="keyCode">The key to press to call the attached method</param>
        public KeybindAttribute(KeyCode keyCode)
        {
            KeyCode = keyCode;
        }

        internal static ValueTuple<MethodInfo, KeybindAttribute>[] keybinds;

        private static void OnApplicationStart()
        {
            // this is performant i swear
            keybinds = MelonHandler.Mods
                .Select(mod => mod.Assembly.GetTypes())
                .SelectMany(types => types)
                // Include instance bindingflag so users get a granular error
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                .Where(method => { try { return method.GetCustomAttribute<KeybindAttribute>() != null; } catch { return false; } })
                .Select(method => new ValueTuple<MethodInfo, KeybindAttribute>(method, method.GetCustomAttribute<KeybindAttribute>()))
                .ToArray();
        }

        internal static void OnUpdate()
        {
            foreach (ValueTuple<MethodInfo, KeybindAttribute> keybind in keybinds)
            {
                if (Input.GetKeyDown(keybind.Item2.KeyCode))
                {
                    try
                    {
                        keybind.Item1.Invoke(null, null);
                    }
                    catch (TargetException)
                    {
                        throw new ArgumentException($"Method of keybind {keybind.Item1.Name} on type {keybind.Item1.DeclaringType.Name} is not static.");
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException($"Method of keybind {keybind.Item1.Name} on type {keybind.Item1.DeclaringType.Name} has parameters.");
                    }
                }
            }
        }
    }
}
