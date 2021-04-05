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
                using (StreamWriter stream = File.CreateText(instanceDatabasePath))
                    stream.WriteLine("[]");
            }
            else
            {
                JArray array = JArray.Parse(File.ReadAllText(instanceDatabasePath));
                foreach (JToken token in array)
                    instances.Add(JObject.Parse(token.ToString()).ToObject<WorldInstance>());
            }

            WorldManager.OnEnterWorldEvent += new Action<ApiWorld, ApiWorldInstance>((world, instance) => Add(world, instance));
        }

        public static void Add(ApiWorld world, ApiWorldInstance instance)
        {
            instances.Add(new WorldInstance(world.name, world.id, instance.idWithTags));

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
