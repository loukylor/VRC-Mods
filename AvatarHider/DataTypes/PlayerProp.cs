using System;
using System.Collections.Generic;
using UnityEngine;
using VRC;

namespace AvatarHider.DataTypes
{
    public class PlayerProp
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            PlayerProp objAsPlayerProp = (PlayerProp)obj;
            if (objAsPlayerProp == null)
                return false;
            else
                return Equals(objAsPlayerProp);
        }
        public bool Equals(PlayerProp playerProp)
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
        public Vector3 position;
        public Player player;
        public GameObject avatar;
        public List<RendererObject> avatarRenderers = new List<RendererObject>();
        public List<AudioSource> audioSources = new List<AudioSource>();
        public bool hasLetAudioPlay;

        public bool isFriend;
        public bool isShown;
        public bool isHidden;

        public void SetActive()
        {
            setActiveDelegate(this);
        }
        public void SetInActive()
        {
            setInactiveDelegate(this);
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
            avatarRenderers.Clear();
            if (avatar != null)
            {
                this.avatar = avatar;
                foreach (Renderer renderer in avatar.GetComponentsInChildren<Renderer>(true))
                    avatarRenderers.Add(new RendererObject(renderer, renderer.enabled));
                avatar.SetActive(true);
                active = false; // Do this so avatar sounds run on the first time
                foreach (AudioSource audioSource in avatar.GetComponentsInChildren<AudioSource>(true))
                    audioSources.Add(audioSource);
                hasLetAudioPlay = false;
            }
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

        private static Action<PlayerProp> setInactiveDelegate;
        private static readonly Action<PlayerProp> setInactivePartlyDelegate = new Action<PlayerProp>((playerProp) =>
        {
            for (int i = 0; i < playerProp.avatarRenderers.Count; i++)
            {
                if (playerProp.avatarRenderers[i].renderer == null)
                {
                    playerProp.avatarRenderers.RemoveAt(i);
                    i -= 1;
                    continue;
                }

                bool isEnabled = playerProp.avatarRenderers[i].renderer.enabled;
                if (isEnabled && !playerProp.avatarRenderers[i].wasActive)
                    playerProp.avatarRenderers[i].wasActive = true;
                else if (!isEnabled && !playerProp.avatarRenderers[i].wasActive)
                    playerProp.avatarRenderers[i].wasActive = false;

                if (isEnabled)
                    playerProp.avatarRenderers[i].renderer.enabled = false;
            }
        });
        private static readonly Action<PlayerProp> setInactiveCompletelyDelegate = new Action<PlayerProp>((playerProp) =>
        {
            for (int i = 0; i < playerProp.avatarRenderers.Count; i++)
            {
                if (playerProp.avatarRenderers[i].renderer == null)
                {
                    playerProp.avatarRenderers.RemoveAt(i);
                    i -= 1;
                    continue;
                }

                bool isEnabled = playerProp.avatarRenderers[i].renderer.enabled;
                if (isEnabled && !playerProp.avatarRenderers[i].wasActive)
                    playerProp.avatarRenderers[i].wasActive = true;
                else if (!isEnabled && !playerProp.avatarRenderers[i].wasActive)
                    playerProp.avatarRenderers[i].wasActive = false;
            }

            if (playerProp.avatar != null)
                playerProp.avatar.SetActive(false);
        });

        private static Action<PlayerProp> setActiveDelegate;
        private static readonly Action<PlayerProp> setActivePartlyDelegate = new Action<PlayerProp>((playerProp) =>
        {
            for (int i = 0; i < playerProp.avatarRenderers.Count; i++)
                playerProp.avatarRenderers[i].renderer.enabled = playerProp.avatarRenderers[i].wasActive;
        });
        private static readonly Action<PlayerProp> setActiveCompletelyDelegate = new Action<PlayerProp>((playerProp) =>
        {
            playerProp.avatar.SetActive(true);
        });

        public static event Action<PlayerProp> OnEnable;
        public static event Action<PlayerProp> OnDisable;

        public static void Init()
        {
            Config.HideAvatarsCompletely.OnValueChanged += OnRelevantConfigChanged;
            Config.DisableSpawnSound.OnValueChanged += OnRelevantConfigChanged;
            OnRelevantConfigChanged(true, false);
        }

        public static void OnRelevantConfigChanged(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;

            OnEnable = null;
            OnDisable = null;

            foreach (PlayerProp playerProp in PlayerManager.filteredPlayers.Values)
            {  
                playerProp.SetActive();
                playerProp.active = false; // Do this so avatar sounds run on the first time
            }
            if (Config.HideAvatarsCompletely.Value)
            {
                if (Config.DisableSpawnSound.Value)
                {
                    foreach (PlayerProp playerProp in PlayerManager.filteredPlayers.Values)
                        playerProp.hasLetAudioPlay = false;

                    setActiveDelegate += setActiveCompletelyDelegate;
                    setInactiveDelegate += setInactiveCompletelyDelegate;

                    OnEnable += new Action<PlayerProp>((playerProp) =>
                    {
                        if (playerProp.hasLetAudioPlay)
                            playerProp.StopAudio();
                    });
                    OnDisable += new Action<PlayerProp>((playerProp) =>
                    {
                        if (!playerProp.hasLetAudioPlay)
                            playerProp.hasLetAudioPlay = true;
                    });
                }
                else
                {
                    setActiveDelegate = setActiveCompletelyDelegate;
                    setInactiveDelegate = setInactiveCompletelyDelegate;
                }
            }
            else
            {
                setActiveDelegate = setActivePartlyDelegate;
                setInactiveDelegate = setInactivePartlyDelegate;
            }

            setActiveDelegate += new Action<PlayerProp>((playerProp) =>
            {
                if (!playerProp.active)
                    OnEnable?.Invoke(playerProp);
                playerProp.active = true;
            });
            setInactiveDelegate += new Action<PlayerProp>((playerProp) =>
            {
                if (playerProp.active)
                    OnDisable?.Invoke(playerProp);
                playerProp.active = false;
            });

            RefreshManager.Refresh();
        }
    }
}
