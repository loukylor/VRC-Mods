using UnityEngine;
using UnityEngine.UI;

namespace PlayerList.UI
{
    public class TileWithText : TileBase
    {
        public Text textComponent;
        
        private bool _resize;
        public bool Resize
        {
            get { return _resize; }
            set { _resize = value; textComponent.resizeTextForBestFit = value; }
        }

        public TileWithText(GameObject parent, GameObject template, Vector3 position, string text, string name, bool resize = false, Color? color = null) : base(parent, template, position, name)
        {
            textComponent = gameObject.transform.GetComponentInChildren<Text>();

            if (textComponent != null)
            {
                textComponent.text = text;
                if (color != null)
                    textComponent.color = (Color)color;
                Resize = resize;
            }
        }
    }
}
