using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;

namespace TriggerESP.Components
{
    [RegisterTypeInIl2Cpp]
    class TriggerESPRenderManager : MonoBehaviour
    {
        public TriggerESPRenderManager(IntPtr obj0) : base(obj0) { }

        internal CommandBuffer commandBuffer;

        // Again most of this code is from VRChat mono
        internal void Awake()
        {
            commandBuffer = new CommandBuffer() { name = "TriggerESP" };
            GetComponent<Camera>().AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
        }

        internal void OnPreRender()
        {
            commandBuffer?.Clear();
            if (!TriggerESPMod.isOn)
                return;

            commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            for (int i = 0; i < TriggerESPComponent.currentESPs.Count; i++)
                if (TriggerESPComponent.currentESPs[i].gameObject.active)
                    commandBuffer.DrawRenderer(TriggerESPComponent.currentESPs[i].renderer, TriggerESPComponent.currentESPs[i].renderer.material);
        }
    }
}
