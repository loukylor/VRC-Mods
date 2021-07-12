using System;
using MelonLoader;
using PlayerList.Config;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class EntryBase : MonoBehaviour
    {
        public EntryBase(IntPtr obj0) : base(obj0) { }

        public EntryBase() { }

        public virtual string Name { get; }

        public MelonPreferences_Entry<bool> prefEntry;

        public Text textComponent;

        private string _originalText;
        public string OriginalText
        {
            get { return _originalText; }
        }

        private static int globalIdentifier = 0;

        private int _identifier;
        public int Identifier
        {
            get { return _identifier; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            EntryBase objAsEntry = obj as EntryBase;
            if (objAsEntry == null)
                return false;
            else
                return Equals(objAsEntry);
        }
        public bool Equals(EntryBase entry)
        {
            if (entry == null)
                return false;
            return entry._identifier == _identifier;
        }

        public virtual void Init(object[] parameters = null)
        {
        }

        public void Refresh(object[] parameters = null)
        {
            if (!gameObject.active)
                return;

            ProcessText(parameters);
        }

        protected virtual void ProcessText(object[] parameters = null)
        {
        }

        public virtual void OnSceneWasLoaded()
        {
        }

        public virtual void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
        }

        public virtual void OnConfigChanged()
        {
        }

        public virtual void OnAvatarInstantiated(VRCAvatarManager vrcPlayer, ApiAvatar avatar, GameObject gameObject)
        {
        }

        public virtual void OnAvatarDownloadProgressed(AvatarLoadingBar loadingBar, float downloadPercentage, long fileSize)
        {
        }

        public virtual void Remove()
        {
            EntryManager.entries.Remove(this);
            DestroyImmediate(gameObject);
        }

        public static T CreateInstance<T>(GameObject gameObject, object[] parameters = null, bool includeConfig = false) where T : EntryBase
        {
            EntryBase entry = gameObject.AddComponent<T>();
            entry._identifier = globalIdentifier++;
            entry.textComponent = gameObject.GetComponent<Text>();
            if (entry.textComponent != null)
                entry._originalText = entry.textComponent.text;
            
            if (includeConfig)
            { 
                entry.prefEntry = PlayerListConfig.category.CreateEntry(entry.Name.Replace(" ", ""), true, is_hidden: true);
                entry.gameObject.SetActive(entry.prefEntry.Value);
            }

            entry.Init(parameters);

            return (T)entry;
        }
    }
}
