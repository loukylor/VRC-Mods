using UnityEngine;
using UnityEngine.UI;

namespace PlayerList.Entries
{
    public class EntryBase
    {
        private string _originalText;
        public string OriginalText
        {
            get { return _originalText; }
        }

        public GameObject gameObject;
        public Text textComponent;

        private int _identifier;
        public int Identifier
        {
            get { return _identifier; }
        }

        public virtual void Init(object[] parameters = null)
        {

        }

        public void Refresh(object[] parameters = null)
        {
            textComponent.text = _originalText;
            ProcessText(parameters);
        }

        public virtual void ProcessText(object[] parameters = null)
        {
        }

        public static EntryBase CreateInstance<T>(GameObject gameObject, object[] parameters = null) where T : EntryBase, new()
        {
            EntryBase entry = new T();

            entry._identifier = gameObject.GetInstanceID();
            entry.gameObject = gameObject;
            entry.textComponent = gameObject.GetComponent<Text>();
            entry._originalText = entry.textComponent.text;

            entry.Init(parameters);

            return entry;
        }
        public void ChangeEntry(string identifier, string value) => textComponent.text = textComponent.text.Replace($"{{{identifier}}}", value);
        public void ChangeEntry(string identifier, object value) => textComponent.text = textComponent.text.Replace($"{{{identifier}}}", value.ToString());
    }
}
