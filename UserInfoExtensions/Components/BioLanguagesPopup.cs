using System;
using UnityEngine;
using UnityEngine.UI;
using UserInfoExtensions.Modules;

namespace UserInfoExtensions.Components
{
    public class BioLanguagesPopup : VRCUiPopup
    {
        public Button closeButton;
        public Text[] languageTexts;
        public GameObject[] languageStates;

        public new void OnEnable()
        {
            base.OnEnable();
            for (int i = 0; i < languageStates.Length; i++)
            {
                if (i < BioButtons.userLanguages.Count)
                {
                    languageTexts[i].text = BioButtons.userLanguages[i];
                    languageStates[i].SetActive(true);
                }
                else
                {
                    languageStates[i].SetActive(false);
                }
            }
        }

        public unsafe BioLanguagesPopup(IntPtr obj0) : base(obj0)
        {
        }
    }
}
