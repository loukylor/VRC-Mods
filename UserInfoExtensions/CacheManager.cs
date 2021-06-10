using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnhollowerRuntimeLib;
using VRC.Core;

namespace UserInfoExtensions
{
    class CacheManager
    {
        public static Dictionary<string, UserInfoExtensionsAPIUser> cachedUsers = new Dictionary<string, UserInfoExtensionsAPIUser>();

        public static void Init() 
        {
            UserInfoExtensionsMod.Instance.HarmonyInstance.Patch(typeof(API).GetMethod("SendRequestInternal"), new HarmonyMethod(typeof(CacheManager).GetMethod(nameof(OnContainerComplete), BindingFlags.Static | BindingFlags.NonPublic)));
        } 

        private static void OnContainerComplete(ApiContainer __2)
        {
            if (__2 == null) return;
            
            if (__2.OnSuccess != null)
                __2.OnSuccess = ((Il2CppSystem.Action<ApiContainer>)new Action<ApiContainer>(OnContainerSuccess)).CombineImpl(__2.OnSuccess).Cast<Il2CppSystem.Action<ApiContainer>>();
            else
                __2.OnSuccess = new Action<ApiContainer>(OnContainerSuccess);
        }
        private static void OnContainerSuccess(ApiContainer container)
        {
            if (container == null) return;

            ApiModelListContainer<APIUser> listContainer = container.TryCast<ApiModelListContainer<APIUser>>();
            if (listContainer != null)
            {
                try
                {
                    for (int i = 0; i < listContainer.ResponseList.Count; i++)
                        cachedUsers[listContainer.ResponseModels[i].id] = new UserInfoExtensionsAPIUser(listContainer.ResponseList[i]);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Failed while caching user from list:\n" + ex.ToString());
                }
            }
            else if (container.Model != null && container.Model.GetIl2CppType() == Il2CppType.Of<APIUser>())
            {
                try
                {
                    cachedUsers[container.Model.id] = new UserInfoExtensionsAPIUser(container.Data);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Failed while caching user:\n" + ex.ToString());
                }
            }
        }

        public class UserInfoExtensionsAPIUser
        {
            public string Id { get; set; }
            public string DateJoined { get; set; }

            public bool IsFullAPIUser 
            { 
                get { return !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(DateJoined); }
            }

            public UserInfoExtensionsAPIUser(string id, string dateJoined)
            {
                Id = id;
                DateJoined = dateJoined;
            }
            public UserInfoExtensionsAPIUser(Il2CppSystem.Object jsonObject)
            {
                if (jsonObject == null) return;

                Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Object> jsonDictionary = jsonObject.Cast<Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Object>>();
                if (jsonDictionary.ContainsKey("id"))
                    Id = jsonDictionary["id"].ToString();

                if (jsonDictionary.ContainsKey("date_joined"))
                    DateJoined = jsonDictionary["date_joined"].ToString();
            }
        }
    }
}
