using MelonLoader;
using PlayerList.Config;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;

namespace PlayerList.Entries
{
    public class EntryBase
    {
        public virtual string Name { get; }

        public MelonPreferences_Entry<bool> prefEntry;

        public GameObject gameObject;
        public Text textComponent;

        private string _originalText;
        public string OriginalText
        {
            get { return _originalText; }
        }

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
        public override int GetHashCode()
        {
            return _identifier;
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

        public virtual void OnAvatarInstantiated(VRCAvatarManager vrcPlayer, GameObject avatar)
        {
        }

        public virtual void OnAvatarDownloadProgressed(AvatarLoadingBar loadingBar, float downloadPercentage, long fileSize)
        {
        }

        public virtual void Remove()
        {
            Object.DestroyImmediate(gameObject);
            EntryManager.entries.Remove(this);
        }

        public static T CreateInstance<T>(GameObject gameObject, object[] parameters = null, bool includeConfig = false) where T : EntryBase, new()
        {
            EntryBase entry = new T
            {
                _identifier = gameObject.GetInstanceID(),
                gameObject = gameObject,
                textComponent = gameObject.GetComponent<Text>()
            };
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
