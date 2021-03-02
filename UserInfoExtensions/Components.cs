using System;
using System.IO;
using System.Net;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UserInfoExtentions.Modules;

namespace UserInfoExtentions.Component
{
    //Learned from Knah's UIExpansionKit (https://github.com/knah/VRCMods/blob/master/UIExpansionKit/Components/EnableDisableListener.cs)
    public class BioLinksPopup : VRCUiPopup
    {
        public UnityEngine.UI.Button openLinkButton;
        public UnityEngine.UI.ToggleGroup toggleGroup;
        public UnityEngine.UI.Text[] linkTexts;
        public UnityEngine.UI.RawImage[] icons;
        public GameObject[] linkStates;
        public Uri currentLink;

        public new void OnEnable()
        {
            base.OnEnable();
            for (int index = 0; index < linkStates.Length; index++)
            {
                linkStates[index].SetActive(true);
                if (index < BioButtons.bioLinks.Count)
                {
                    DownloadTexture(index);
                }
                else
                {
                    linkStates[index].SetActive(false);
                }
            }
        }
        public new void OnDisable()
        {
            base.OnDisable();
            foreach (GameObject linkstate in linkStates) linkstate.GetComponent<UnityEngine.UI.Toggle>().isOn = false;
        }

        [method: HideFromIl2Cpp]
        public async void DownloadTexture(int index)
        {
            linkTexts[index].text = BioButtons.bioLinks[index].OriginalString.Length >= 43 ? BioButtons.bioLinks[index].OriginalString.Substring(0, 43) : BioButtons.bioLinks[index].OriginalString;
            WebRequest iconRequest = WebRequest.Create($"http://www.google.com/s2/favicons?domain_url={BioButtons.bioLinks[index].Host}&sz=64");

            WebResponse response = await iconRequest.GetResponseAsync().NoAwait();

            MemoryStream stream = new MemoryStream();
            response.GetResponseStream().CopyTo(stream);

            await Utilities.YieldToMainThread();
            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, stream.ToArray());
            icons[index].texture = tex;
        }

        public void OnOpenLink()
        {
            if (currentLink != null)
            {
                System.Diagnostics.Process.Start(currentLink.OriginalString);
                Close();
                Utilities.OpenPopupV2("Notice:", "Link has been opened in the default browser", "Close", new Action(() => Utilities.ClosePopup()));
                currentLink = null;
            }
        }

        public unsafe BioLinksPopup(IntPtr obj0) : base(obj0)
        {
        }
    }
    public class BioLanguagesPopup : VRCUiPopup
    {
        public UnityEngine.UI.Button closeButton;
        public UnityEngine.UI.Text[] languageTexts;
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

