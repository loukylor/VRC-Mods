using System;
using System.Collections.Generic;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using VRChatUtilityKit.Utilities;

namespace AvatarHider.DataTypes
{
    public class AvatarHiderPlayer
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            AvatarHiderPlayer objAsPlayerProp = (AvatarHiderPlayer)obj;
            if (objAsPlayerProp == null)
                return false;
            else
                return Equals(objAsPlayerProp);
        }
        public bool Equals(AvatarHiderPlayer playerProp)
        {
            return playerProp.userId == userId;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool active;

        public int photonId;
        public string userId;
        public Vector3 Position => player.transform.position;
        public Player player;
        public GameObject avatar;
        public List<AudioSource> audioSources = new List<AudioSource>();
        public bool hasLetAudioPlay;
        public bool hasLimitedAudio;

        public bool isFriend;
        public bool isShown;
        public bool isHidden;

        public void SetActive()
        {
            setActiveDelegate(this);
            if (!active)
                OnEnable?.Invoke(this);
            active = true;
        }
        public void SetInActive()
        {
            setInactiveDelegate(this);
            if (active)
                OnDisable?.Invoke(this);
            active = false;
        }

        public void StopAudio()
        {
            for (int i = 0; i < audioSources.Count; i++)
            { 
                if (audioSources[i] == null)
                {
                    audioSources.RemoveAt(i);
                    i -= 1;
                    continue;
                }
                audioSources[i].Stop();
            }
        }

        public void SetAvatar(GameObject avatar)
        {
            if (avatar != null)
            {
                this.avatar = avatar;
                avatar.SetActive(false);
                foreach (AudioSource audioSource in avatar.GetComponentsInChildren<AudioSource>(true))
                {
                    audioSources.Add(audioSource);
                    if (Config.LimitAudioDistance.Value)
                        LimitAudioSource(audioSource);
                }
                avatar.SetActive(true);
                active = false; // Do this so avatar sounds run on the first time
                hasLetAudioPlay = false;
                hasLimitedAudio = true;
            }
        }

        public void LimitAudioDistance()
        {
            foreach (AudioSource audioSource in audioSources)
                LimitAudioSource(audioSource);
        }

        private void LimitAudioSource(AudioSource audioSource)
        {
            audioSource.spatialBlend = 1;

            if (audioSource.minDistance > Config.MaxAudioDistance.Value)
                audioSource.minDistance = Config.MaxAudioDistance.Value;

            if (audioSource.maxDistance > Config.MaxAudioDistance.Value)
                audioSource.maxDistance = Config.MaxAudioDistance.Value;

            VRC_SpatialAudioSource spatialSource = audioSource.GetComponent<VRC_SpatialAudioSource>();
            if (spatialSource == null)
                return;

            ONSPAudioSource onspSource = spatialSource.GetComponent<ONSPAudioSource>();
            if (onspSource != null)
            {
                onspSource.enableSpatialization = true;
                if (onspSource.far > Config.MaxAudioDistance.Value)
                    onspSource.far = Config.MaxAudioDistance.Value;

                if (onspSource.volumetricRadius > Config.MaxAudioDistance.Value)
                    onspSource.volumetricRadius = Config.MaxAudioDistance.Value;

                if (onspSource.near > Config.MaxAudioDistance.Value)
                    onspSource.near = Config.MaxAudioDistance.Value;
            }

            spatialSource.EnableSpatialization = true;
            if (spatialSource.Far > Config.MaxAudioDistance.Value)
                spatialSource.Far = Config.MaxAudioDistance.Value;

            if (spatialSource.VolumetricRadius > Config.MaxAudioDistance.Value)
                spatialSource.VolumetricRadius = Config.MaxAudioDistance.Value;

            if (spatialSource.Near > Config.MaxAudioDistance.Value)
                spatialSource.Near = Config.MaxAudioDistance.Value;
        }

        public class RendererObject
        {
            public Renderer renderer;
            public bool wasActive;

            public RendererObject(Renderer renderer, bool isActive)
            {
                this.renderer = renderer;
                wasActive = isActive;
            }
        }

        private static Action<AvatarHiderPlayer> setInactiveDelegate;
        private static readonly Action<AvatarHiderPlayer> setInactiveCompletelyDelegate = new Action<AvatarHiderPlayer>((playerProp) =>
        {
            if (playerProp.avatar != null)
                playerProp.avatar.SetActive(false);
        });

        private static Action<AvatarHiderPlayer> setActiveDelegate;
        private static readonly Action<AvatarHiderPlayer> setActiveCompletelyDelegate = new Action<AvatarHiderPlayer>((playerProp) =>
        {
            if (playerProp.avatar != null)
                playerProp.avatar.SetActive(true);
        });

        public static event Action<AvatarHiderPlayer> OnEnable;
        public static event Action<AvatarHiderPlayer> OnDisable;

        private static bool wasLimitOnBefore = false;

        public static void Init()
        {
            Config.DisableSpawnSound.OnValueChangedUntyped += OnStaticConfigChange;
            Config.LimitAudioDistance.OnValueChangedUntyped += OnStaticConfigChange;
            Config.MaxAudioDistance.OnValueChangedUntyped += OnStaticConfigChange;
            OnStaticConfigChange();
        }

        public static void OnStaticConfigChange()
        {
            OnEnable = null;
            OnDisable = null;

            foreach (AvatarHiderPlayer playerProp in PlayerManager.filteredPlayers.Values)
            {  
                playerProp.SetActive();
                playerProp.active = false; // Do this so avatar sounds run on the first time
            }

            setActiveDelegate = setActiveCompletelyDelegate;
            setInactiveDelegate = setInactiveCompletelyDelegate;

            if (Config.DisableSpawnSound.Value)
            {
                foreach (AvatarHiderPlayer ahPlayer in PlayerManager.filteredPlayers.Values)
                    ahPlayer.hasLetAudioPlay = false;

                OnEnable += new Action<AvatarHiderPlayer>((ahPlayer) =>
                {
                    if (ahPlayer.hasLetAudioPlay)
                        ahPlayer.StopAudio();
                });
                OnDisable += new Action<AvatarHiderPlayer>((ahPlayer) =>
                {
                    if (!ahPlayer.hasLetAudioPlay)
                        ahPlayer.hasLetAudioPlay = true;
                });
            }
            if (Config.LimitAudioDistance.Value)
            {
                wasLimitOnBefore = true;
                foreach (AvatarHiderPlayer ahPlayer in PlayerManager.players.Values)
                {
                    if (!ahPlayer.active)
                        continue;

                    ahPlayer.LimitAudioDistance();

                    ahPlayer.hasLimitedAudio = true;
                }
                OnEnable += new Action<AvatarHiderPlayer>((ahPlayer) =>
                {
                    if (!ahPlayer.hasLimitedAudio)
                    {
                        ahPlayer.LimitAudioDistance();

                        ahPlayer.hasLimitedAudio = true;
                    }
                });
            }
            else if (wasLimitOnBefore)
            {
                VRCUtils.ReloadAllAvatars();       
            }
            RefreshManager.Refresh();
        }
    }
}
