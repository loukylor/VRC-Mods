using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRC.Core;

namespace InstanceHistory
{
    class InstanceManager
    {
        public static List<WorldInstance> instances = new List<WorldInstance>();
        private static readonly string instanceDatabasePath = Path.Combine(MelonUtils.UserDataDirectory, "InstanceHistory.json");

        public static void Init()
        {
            MelonLogger.Msg("Loading Instances...");

            if (!File.Exists(instanceDatabasePath))
            {
                File.WriteAllText(instanceDatabasePath, "[]");
                MelonLogger.Msg("InstanceHistory.json not found. Creating new one...");
            }
            else
            {
                string text = File.ReadAllText(instanceDatabasePath);
                if (string.IsNullOrWhiteSpace(text))
                { 
                    File.WriteAllText(instanceDatabasePath, "[]");
                    text = "[]";
                    MelonLogger.Msg("InstanceHistory.json is empty. Creating new one...");
                }

                try
                {
                    JArray array = JArray.Parse(text);
                    foreach (JToken token in array)
                        instances.Add(JObject.Parse(token.ToString()).ToObject<WorldInstance>());
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Something went wrong while parsing the json file.\nIt is likely that your InstanceHistory.json file is corrupted and will need to be manually deleted. Find it in the VRChat/UserData folder.\nFor debug purposes in case this is not the case, here is the error:\n" + ex.ToString());
                    return;
                }
            }
            
            MelonLogger.Msg("Instances Loaded!");
            
            WorldManager.OnEnterWorldEvent += new Action<ApiWorld, ApiWorldInstance>((world, instance) => Add(world, instance));
        }

        public static void Add(ApiWorld world, ApiWorldInstance instance)
        {
            instances.Insert(0, new WorldInstance(world.name, world.id, instance.instanceId));

            File.WriteAllText(instanceDatabasePath, JsonConvert.SerializeObject(instances, Formatting.Indented));
        }

        public struct WorldInstance
        {
            public string worldName;
            public string worldId;
            public string instanceId;

            public WorldInstance(string worldName, string worldId, string instanceId)
            {
                this.worldName = worldName;
                this.worldId = worldId;
                this.instanceId = instanceId;
            }
        }
    }
}
