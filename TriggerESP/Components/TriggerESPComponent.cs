using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

namespace TriggerESP.Components 
{
    [RegisterTypeInIl2Cpp]
    public class TriggerESPComponent : MonoBehaviour
    {
        public TriggerESPComponent(IntPtr obj0) : base(obj0) { }

		internal static List<TriggerESPComponent> currentESPs = new List<TriggerESPComponent>();

        internal Component trigger;
        internal MeshRenderer renderer;
        internal MeshFilter filter;

		internal void Init(Component trigger)
        {
			currentESPs.Add(this);

			renderer = gameObject.AddComponent<MeshRenderer>();
			filter = gameObject.AddComponent<MeshFilter>();

			OnShouldUseOutlineChanged(!TriggerESPMod.shouldUseOutline.Value, TriggerESPMod.shouldUseOutline.Value);

			this.trigger = trigger;
			Clone();

			renderer.enabled = false;
		}

		internal static void OnShouldUseOutlineChanged(bool oldValue, bool newValue)
        {
			if (oldValue == newValue)
				return;

			if (newValue)
				foreach (TriggerESPComponent espComponent in currentESPs)
					espComponent.renderer.material = new Material(TriggerESPMod.outlineShader);
			else
				foreach (TriggerESPComponent espComponent in currentESPs)
					espComponent.renderer.material = new Material(TriggerESPMod.wireframeShader);

			OnThicknessPrefChanged();
			OnColorPrefChanged();
		}

		internal static void OnColorPrefChanged()
        {
			TriggerESPMod.espColorR.Value = Mathf.Clamp(TriggerESPMod.espColorR.Value, 0f, 255f);
			TriggerESPMod.espColorG.Value = Mathf.Clamp(TriggerESPMod.espColorG.Value, 0f, 255f);
			TriggerESPMod.espColorB.Value = Mathf.Clamp(TriggerESPMod.espColorB.Value, 0f, 255f);


			foreach (TriggerESPComponent espComponent in currentESPs)
            {
				if (TriggerESPMod.randomESPColor.Value)
					espComponent.renderer.material.color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
				else
					espComponent.renderer.material.color = new Color(TriggerESPMod.espColorR.Value, TriggerESPMod.espColorG.Value, TriggerESPMod.espColorB.Value);
				espComponent.renderer.enabled = false;
			}
		}

		internal static void OnThicknessPrefChanged() 
		{
			TriggerESPMod.wireframeWidth.Value = Mathf.Clamp(TriggerESPMod.wireframeWidth.Value, 0f, 1f);

			if (TriggerESPMod.shouldUseOutline.Value)
            {
				foreach (TriggerESPComponent espComponent in currentESPs)
				{
					espComponent.renderer.material.SetFloat("_Width", TriggerESPMod.outlineStrength.Value);
					espComponent.renderer.enabled = false;
				}
			}
            else
            {
				foreach (TriggerESPComponent espComponent in currentESPs)
				{
					espComponent.renderer.material.SetFloat("_Width", TriggerESPMod.wireframeWidth.Value);
					espComponent.renderer.enabled = false;
				}
			}

		}

        internal void OnDestroy()
        {
            for (int i = 0; i < currentESPs.Count; i++)
            {
                if (currentESPs[i].GetInstanceID() == GetInstanceID())
                {
                    currentESPs.RemoveAt(i);
                    return;
                }
            }
        }

		// This is code taken from VRChat mono
		private void Clone()
		{
			MeshCollider meshCollider = trigger.TryCast<MeshCollider>();
			if (meshCollider != null)
			{
				filter.sharedMesh = meshCollider.sharedMesh;
				transform.position = trigger.transform.position;
				transform.rotation = trigger.transform.rotation;
				transform.localScale *= 1.01f;
				return;
			}

			BoxCollider boxCollider = trigger.TryCast<BoxCollider>();
			if (boxCollider != null)
			{
				filter.sharedMesh = TriggerESPMod.cube;
				
				transform.position = boxCollider.transform.TransformPoint(boxCollider.center);
				transform.rotation = boxCollider.transform.rotation;
				transform.localScale = Vector3.Scale(boxCollider.size, transform.localScale) * 1.01f;
				
				return;
			}

			SphereCollider sphereCollider = trigger.TryCast<SphereCollider>();
			if (sphereCollider != null)
			{
				filter.sharedMesh = TriggerESPMod.sphere;
				
				transform.position = sphereCollider.transform.TransformPoint(sphereCollider.center);
				transform.localScale = sphereCollider.radius * transform.localScale * 1.01f;
				
				return;
			}

			CapsuleCollider capsuleCollider = trigger.TryCast<CapsuleCollider>();
			if (capsuleCollider != null)
			{
				filter.sharedMesh = TriggerESPMod.capsule;
				
				transform.position = capsuleCollider.transform.TransformPoint(capsuleCollider.center);

				Quaternion rhs = Quaternion.identity;
				float num = capsuleCollider.height;
				float num2 = capsuleCollider.radius;
				if (capsuleCollider.direction == 0)
				{
					rhs = Quaternion.Euler(0f, 0f, -90f);
					num2 = Mathf.Abs(Mathf.Max(capsuleCollider.transform.lossyScale.y, capsuleCollider.transform.lossyScale.z) * capsuleCollider.radius);
					num = Mathf.Max(capsuleCollider.transform.lossyScale.x * capsuleCollider.height, num2);
				}
				else if (capsuleCollider.direction == 1)
				{
					num2 = Mathf.Abs(Mathf.Max(capsuleCollider.transform.lossyScale.x, capsuleCollider.transform.lossyScale.z) * capsuleCollider.radius);
					num = Mathf.Max(capsuleCollider.transform.lossyScale.y * capsuleCollider.height, num2);
				}
				else if (capsuleCollider.direction == 2)
				{
					rhs = Quaternion.Euler(90f, 0f, 0f);
					num2 = Mathf.Abs(Mathf.Max(capsuleCollider.transform.lossyScale.x, capsuleCollider.transform.lossyScale.y) * capsuleCollider.radius);
					num = Mathf.Max(capsuleCollider.transform.lossyScale.z * capsuleCollider.height, num2);
				}
				transform.rotation = capsuleCollider.transform.rotation * rhs;
				transform.localScale = Vector3.Scale(new Vector3(num2, num / 4f, num2), transform.localScale) * 1.01f;
				
				return;
			}

			Selectable selectable = trigger.TryCast<Selectable>();
			if (selectable != null)
            {
				filter.sharedMesh = TriggerESPMod.cube;

				RectTransform selectableRect = selectable.transform.Cast<RectTransform>();
				if (selectable.image != null)
				{
					// Determine which is bigger and choose the bigger one
					RectTransform imageRect = selectable.image.transform.Cast<RectTransform>();
					if ((imageRect.sizeDelta.x * imageRect.sizeDelta.y) > (selectableRect.sizeDelta.x * selectableRect.sizeDelta.y))
						selectableRect = imageRect;
				}

				transform.position = selectableRect.position;
				transform.rotation = trigger.transform.rotation;
				transform.localScale = new Vector3(selectableRect.sizeDelta.x, selectableRect.sizeDelta.y, 1) * 1.01f;
			}
		}
	}
}
