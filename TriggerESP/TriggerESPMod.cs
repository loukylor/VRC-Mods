using System.Collections;
using System.IO;
using System.Reflection;
using MelonLoader;
using TriggerESP.Components;
using UnhollowerRuntimeLib;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRChatUtilityKit.Utilities;

[assembly: MelonInfo(typeof(TriggerESP.TriggerESPMod), "TriggerESP", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace TriggerESP
{
	// you dont understand how much of a huge fucking pain this was oml this was such a project
    public class TriggerESPMod : MelonMod
    {
		internal static bool isOn;

		internal static TriggerESPRenderManager controller;

		internal static Shader wireframeShader;

		internal static Mesh sphere;
		internal static Mesh cube;
		internal static Mesh capsule;

		internal static MelonPreferences_Category category;
		internal static MelonPreferences_Entry<bool> randomESPColor;
		internal static MelonPreferences_Entry<float> espColorR;
		internal static MelonPreferences_Entry<float> espColorG;
		internal static MelonPreferences_Entry<float> espColorB;
		internal static MelonPreferences_Entry<float> espStrength;

		private static string sceneName;

		public override void OnApplicationStart()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Toggle Trigger ESP", OnESPToggle);

			category = MelonPreferences.CreateCategory("TriggerESP", "TriggerESP Config");
			randomESPColor = category.CreateEntry(nameof(randomESPColor), false, "Enable/Disable random color of ESP");
			espColorR = category.CreateEntry(nameof(espColorR), 0f, "The red color of the ESP. Will be ignored if random ESP color is on");
			espColorG = category.CreateEntry(nameof(espColorG), 255f, "The green of the ESP. Will be ignored if random ESP color is on");
			espColorB = category.CreateEntry(nameof(espColorB), 0f, "The blue color of the ESP. Will be ignored if random ESP color is on");

			foreach (MelonPreferences_Entry entry in category.Entries)
				entry.OnValueChangedUntyped += TriggerESPComponent.OnColorPrefChanged;

			espStrength = category.CreateEntry(nameof(espStrength), 0.75f, "The strength of the ESP itself");
			espStrength.OnValueChangedUntyped += TriggerESPComponent.OnThicknessPrefChanged;

			sphere = Resources.Load("PrimitiveMeshes/sphere").Cast<Mesh>();
			cube = Resources.Load("PrimitiveMeshes/cube").Cast<Mesh>();
			capsule = Resources.Load("PrimitiveMeshes/capsule").Cast<Mesh>();

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TriggerESP.triggerespshader.assetbundle"))
			{
				using (var memoryStream = new MemoryStream((int)stream.Length))
				{
					stream.CopyTo(memoryStream);
					AssetBundle assetBundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
					wireframeShader = assetBundle.LoadAsset_Internal("Assets/Shaders/Outline.shader", Il2CppType.Of<Shader>()).Cast<Shader>();
					wireframeShader.hideFlags |= HideFlags.DontUnloadUnusedAsset;
				}
			}

			VRCUtils.OnEmmWorldCheckCompleted += OnEmmWorldCheckCompleted;
		}

		private void OnEmmWorldCheckCompleted(bool areRiskyFuncsAllowed)
        {
			if (areRiskyFuncsAllowed)
				MelonCoroutines.Start(WaitForSceneInit());
        }

		private IEnumerator WaitForSceneInit()
        {
			while (sceneName == null)
				yield return null;

			foreach (Collider collider in Resources.FindObjectsOfTypeAll<Collider>())
			{
				if (collider.gameObject.scene.name == sceneName && collider.GetComponent<VRC_Interactable>() != null)
				{
					if (collider.GetComponent<UdonBehaviour>() != null)
					{
						if (collider.GetComponent<UdonBehaviour>().serializedProgramAsset != null)
							AddESPComponent(collider.Cast<Component>());
					}
					else
					{
						AddESPComponent(collider.Cast<Component>());
					}
				}
			}

			foreach (Selectable selectable in Resources.FindObjectsOfTypeAll<Selectable>())
				if (selectable.gameObject.scene.name == sceneName && selectable.transform.TryCast<RectTransform>() != null)
					AddESPComponent(selectable);
		}

		public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
			TriggerESPMod.sceneName = null;

			if (buildIndex != -1)
				return;

			if (controller == null)
				controller = HighlightsFX.prop_HighlightsFX_0.gameObject.AddComponent<TriggerESPRenderManager>();


			TriggerESPMod.sceneName = sceneName;
		}

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
			if (buildIndex != -1)
				return;

			TriggerESPComponent.currentESPs.Clear();
			isOn = false;
		}

		private static void AddESPComponent(Component trigger)
        {
			GameObject gameObjectChild = new GameObject(trigger.name + " | ESPRenderer");
			gameObjectChild.transform.SetParent(trigger.transform, false);
			gameObjectChild.AddComponent<TriggerESPComponent>().Init(trigger);
		}


        private static void OnESPToggle() => isOn = !isOn;
	}
}
