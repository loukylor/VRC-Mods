using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;

[assembly: MelonInfo(typeof(VRChatUtilityKit.VRChatUtilityKitMod), "VRChatUtilityKit", "1.1.5", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonPriority(-100)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit
{
    public class VRChatUtilityKitMod : MelonMod
    {
        internal static VRChatUtilityKitMod Instance { get; private set; }

        private static object[] melonLoaderEventSubscribers;

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Initializing...");
            Instance = this;
            melonLoaderEventSubscribers = MelonHandler.Mods
                .Select(mod => mod.Assembly.GetTypes())
                .SelectMany(types => types)
                .Where(type => { try { return type.IsSubclassOf(typeof(MelonLoaderEvents)) || Attribute.GetCustomAttribute(type, typeof(MelonLoaderEventsAttribute)) != null; } catch { return false; } })
                .OrderBy((type) =>
                {
                    MelonLoaderEventsPriorityAttribute priority = (MelonLoaderEventsPriorityAttribute)Attribute.GetCustomAttribute(type, typeof(MelonLoaderEventsPriorityAttribute));
                    return priority == null ? 0 : priority.priority;
                })
                .Select(type => type.IsSubclassOf(typeof(MelonLoaderEvents)) ? Activator.CreateInstance(type) : type)
                .ToArray();

            // Keep some calls normal because they need to run before everything else
            XrefUtils.Init();
            try
            {
                UiManager.Init();
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Error while initializing UiManager:\n" + ex.ToString());
            }

            foreach (object subscriber in melonLoaderEventSubscribers)
            {
                try
                {
                    if (subscriber is Type subscriberAsType)
                    {
                        try
                        {
                            subscriberAsType.GetMethod("OnApplicationStart", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)?.Invoke(null, null);
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException($"Type {subscriberAsType.Name} which is subscribed to MelonLoaderEvents via an attribute has an OnApplicationStart that is not static.");
                        }
                    }
                    else
                    {
                        (subscriber as MelonLoaderEvents).OnApplicationStart();
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Exception during OnApplicationStart:\n{ex}");
                }
            }

            VRCUtils.OnUiManagerInit += OnUiManagerInit;
        }

        private void OnUiManagerInit()
        {
            MelonLogger.Msg("Initializing UI...");
            try
            {
                UiManager.UiInit();
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Error while initializing UiManager:\n" + ex.ToString());
            }

            foreach (object subscriber in melonLoaderEventSubscribers)
            {
                try
                {
                    if (subscriber is Type subscriberAsType)
                    {
                        try
                        {
                            subscriberAsType.GetMethod("OnUiManagerInit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)?.Invoke(null, null);
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException($"Type {subscriberAsType.Name} which is subscribed to MelonLoaderEvents via an attribute has an OnUiManagerInit that is not static.");
                        }
                    }
                    else
                    {
                        (subscriber as MelonLoaderEvents).OnUiManagerInit();
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Exception during OnUiManagerInit:\n{ex}");
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach (object subscriber in melonLoaderEventSubscribers)
            {
                try
                {
                    if (subscriber is Type subscriberAsType)
                    {
                        try
                        {
                            subscriberAsType.GetMethod("OnSceneWasLoaded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)?.Invoke(null, new object[2] { buildIndex, sceneName });
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException($"Type {subscriberAsType.Name} which is subscribed to MelonLoaderEvents via an attribute has an OnSceneWasLoaded that is not static.");
                        }
                    }
                    else
                    {
                        (subscriber as MelonLoaderEvents).OnSceneWasLoaded(buildIndex, sceneName);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Exception during OnSceneWasLoaded:\n{ex}");
                }
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (object subscriber in melonLoaderEventSubscribers)
            {
                try
                {
                    if (subscriber is Type subscriberAsType)
                    {
                        try
                        {
                            subscriberAsType.GetMethod("OnSceneWasInitialized", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)?.Invoke(null, new object[2] { buildIndex, sceneName });
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException($"Type {subscriberAsType.Name} which is subscribed to MelonLoaderEvents via an attribute has an OnSceneWasInitialized that is not static.");
                        }
                    }
                    else
                    {
                        (subscriber as MelonLoaderEvents).OnSceneWasInitialized(buildIndex, sceneName);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Exception during OnSceneWasInitialized:\n{ex}");
                }
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            foreach (object subscriber in melonLoaderEventSubscribers)
            {
                try
                {
                    if (subscriber is Type subscriberAsType)
                    {
                        try
                        {
                            subscriberAsType.GetMethod("OnSceneWasUnloaded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)?.Invoke(null, new object[2] { buildIndex, sceneName });
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException($"Type {subscriberAsType.Name} which is subscribed to MelonLoaderEvents via an attribute has an OnSceneWasUnloaded that is not static.");
                        }
                    }
                    else
                    {
                        (subscriber as MelonLoaderEvents).OnSceneWasUnloaded(buildIndex, sceneName);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Exception during OnSceneWasUnloaded:\n{ex}");
                }
            }
        }

        public override void OnApplicationQuit()
        {
            foreach (object subscriber in melonLoaderEventSubscribers)
            {
                try
                {
                    if (subscriber is Type subscriberAsType)
                    {
                        try
                        {
                            subscriberAsType.GetMethod("OnApplicationQuit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)?.Invoke(null, null);
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException($"Type {subscriberAsType.Name} which is subscribed to MelonLoaderEvents via an attribute has an OnApplicationQuit that is not static.");
                        }
                    }
                    else
                    {
                        (subscriber as MelonLoaderEvents).OnApplicationQuit();
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Exception during OnApplicationQuit:\n{ex}");
                }
            }
        }

        public override void OnUpdate()
        {
            if (AsyncUtils._toMainThreadQueue.TryDequeue(out Action result))
                result.Invoke();
            KeybindAttribute.OnUpdate();
        }
    }
}
