﻿using BeatSaberMarkupLanguage;
using BeatSaberPlus.Utils;
using BeatSaberPlusChatCore.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Plugins.ChatEmoteRain
{

    using PrefabPair = ValueTuple<Dictionary<string, Components.TimeoutScript>, UnityEngine.GameObject>;

    /// <summary>
    /// Chat Emote Rain instance
    /// </summary>
    internal partial class ChatEmoteRain : PluginBase
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name => "Chat Emote Rain";
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.ChatEmoteRain.Enabled; set => Config.ChatEmoteRain.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override EActivationType ActivationType => EActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        internal static ChatEmoteRain Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Emote rain view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Emote rain left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// Emote rain right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;
        /// <summary>
        /// Chat core instance
        /// </summary>
        private bool m_ChatCoreAcquired = false;
        /// <summary>
        /// Asset bundle
        /// </summary>
        private AssetBundle m_AssetBundle = null;
        /// <summary>
        /// Particles systems
        /// </summary>
        private Dictionary<Utils.Game.SceneType, PrefabPair> m_ParticleSystems = new Dictionary<Utils.Game.SceneType, PrefabPair>();
        /// <summary>
        /// Combo state Dictionary<EmoteID, Tuple<ComboCount, lastSeenTickCount>>
        /// </summary>
        private Dictionary<string, Tuple<int, int>> m_ComboState = new Dictionary<string, Tuple<int, int>>();
        /// <summary>
        /// Combo state Dictionary<EmoteID, Tuple<List<UserIDs>, lastSeenTickCount>>
        /// </summary>
        private Dictionary<string, Tuple<List<string>, int>> m_ComboState2 = new Dictionary<string, Tuple<List<string>, int>>();
        /// <summary>
        /// SubRain emotes
        /// </summary>
        private Dictionary<string, Texture2D> m_SubRainTextures = new Dictionary<string, Texture2D>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the plugin
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind singleton
            Instance = this;

            /// Bind events
            Utils.Game.OnSceneChange += Game_OnSceneChange;

            /// Create CustomMenuSongs directory if not existing
            if (!Directory.Exists("CustomSubRain"))
                Directory.CreateDirectory("CustomSubRain");

            /// Load assets
            LoadAssets();

            /// Load SubRain files
            LoadSubRainFiles();

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                Utils.ChatService.Acquire();

                /// Run all services
                Utils.ChatService.Multiplexer.OnTextMessageReceived += ChatCoreMutiplixer_OnTextMessageReceived;
            }
        }
        /// <summary>
        /// Disable the plugin
        /// </summary>
        protected override void OnDisable()
        {
            /// Un-init chat core
            if (m_ChatCoreAcquired)
            {
                /// Unbind services
                Utils.ChatService.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                Utils.ChatService.Release();
                m_ChatCoreAcquired = false;
            }

            /// Unload assets
            UnloadAssets();

            /// Unload SubRain emotes
            m_SubRainTextures.Clear();

            /// Unbind events
            Utils.Game.OnSceneChange -= Game_OnSceneChange;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show plugin UI
        /// </summary>
        protected override void ShowUIImplementation()
        {
            /// Create view if needed
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<UI.Settings>();
            /// Create view if needed
            if (m_SettingsLeftView == null)
                m_SettingsLeftView = BeatSaberUI.CreateViewController<UI.SettingsLeft>();
            /// Create view if needed
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            /// Change main view
            BeatSaberPlus.UI.ViewFlowCoordinator.Instance.ChangeMainViewController(m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On game scene change
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        private void Game_OnSceneChange(Game.SceneType p_Scene)
        {
            foreach (var l_KVP in m_ParticleSystems)
            {
                if (l_KVP.Key == p_Scene)
                    continue;

                l_KVP.Value.Item1.Clear();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load assets
        /// </summary>
        private void LoadAssets()
        {
            m_AssetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("BeatSaberPlus.Plugins.ChatEmoteRain.Resources.ChatEmoteRain.bundle"));

            m_ParticleSystems.Add(Utils.Game.SceneType.Menu, new PrefabPair(
                new Dictionary<string, Components.TimeoutScript>(),
                m_AssetBundle.LoadAsset<GameObject>("ERParticleSystemMenu Variant")
            ));
            Logger.Instance.Debug("Prefab at: " + (m_ParticleSystems[Utils.Game.SceneType.Menu].Item2 ? m_ParticleSystems[Utils.Game.SceneType.Menu].Item2.GetFullPath() : "null"));

            m_ParticleSystems.Add(Utils.Game.SceneType.Playing, new PrefabPair(
                new Dictionary<string, Components.TimeoutScript>(),
                m_AssetBundle.LoadAsset<GameObject>("ERParticleSystemPlaySpace Variant")
            ));
            Logger.Instance.Debug("Prefab at: " + (m_ParticleSystems[Utils.Game.SceneType.Playing].Item2 ? m_ParticleSystems[Utils.Game.SceneType.Playing].Item2.GetFullPath() : "null"));
        }
        /// <summary>
        /// Unload assets
        /// </summary>
        private void UnloadAssets()
        {
            if (m_AssetBundle == null)
                return;

            m_ParticleSystems.Clear();

            m_AssetBundle.Unload(true);
            m_AssetBundle = null;
        }
        /// <summary>
        /// Load SubRain emotes
        /// </summary>
        internal void LoadSubRainFiles()
        {
            m_SubRainTextures.Clear();

            foreach (string l_CurrentFile in Directory.GetFiles("CustomSubRain", "*.png"))
            {
                if (!l_CurrentFile.ToLower().EndsWith(".png"))
                    continue;

                Texture2D l_Texture = new Texture2D(2, 2);
                if (!ImageConversion.LoadImage(l_Texture, File.ReadAllBytes(l_CurrentFile)))
                {
                    Logger.Instance.Warn("Failed to load image " + l_CurrentFile);
                    continue;
                }
                l_Texture.wrapMode = TextureWrapMode.Mirror;

                string l_EmoteName = l_CurrentFile.Substring(l_CurrentFile.LastIndexOf('\\') + 1);
                m_SubRainTextures.Add(l_EmoteName.Remove(l_EmoteName.Length - 4), l_Texture);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start a SubRain
        /// </summary>
        internal void StartSubRain()
        {
            if (!Config.ChatEmoteRain.SubRain)
                return;

            if (   (Utils.Game.ActiveScene == Utils.Game.SceneType.Menu    && Config.ChatEmoteRain.MenuRain)
                || (Utils.Game.ActiveScene == Utils.Game.SceneType.Playing && Config.ChatEmoteRain.SongRain))
            {
                string[] l_EmoteIDs  = m_SubRainTextures.Keys.ToArray();
                byte     l_EmitCount = (byte)Config.ChatEmoteRain.SubRainEmoteCount;

                if(l_EmoteIDs.Length == 0)
                {
                    l_EmoteIDs = new string[] { "" };
                }

                foreach (string l_CurrentEmote in l_EmoteIDs)
                    SharedCoroutineStarter.instance.StartCoroutine(StartParticleSystem("SubRainSprite_" + l_CurrentEmote, false, l_EmitCount));
            }
        }
        /// <summary>
        /// Unregister a particle system
        /// </summary>
        /// <param name="p_EmoteID">Emote ID</param>
        /// <param name="p_Mode">Game mode</param>
        internal void UnregisterParticleSystem(string p_EmoteID, Utils.Game.SceneType p_Mode)
        {
            GameObject.Destroy(m_ParticleSystems[p_Mode].Item1[p_EmoteID].gameObject);
            m_ParticleSystems[p_Mode].Item1.Remove(p_EmoteID);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">ID of the message</param>
        private void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_Service, IChatMessage p_Message)
        {
            if (p_Message.IsSystemMessage && Config.ChatEmoteRain.SubRain
                && (p_Message.Message.StartsWith("⭐") || p_Message.Message.StartsWith("👑")))
            {
                //Logger.Instance.Debug($"Received System Message: {p_Message.Message}; Should be Sub.");
                StartSubRain();
            }

            IChatEmote[] l_Emotes = Config.ChatEmoteRain.ComboMode ? FilterEmotesForCombo(p_Message) : p_Message.Emotes;
            if (l_Emotes.Length > 0)
            {
                (from iChatEmote in l_Emotes group iChatEmote by iChatEmote.Id into emoteGrouping
                 select new { emote = emoteGrouping.First(), count = (byte)emoteGrouping.Count() }
                ).ToList().ForEach(x => HMMainThreadDispatcher.instance.Enqueue(EnqueueEmote(x.emote, x.count)));
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enqueue a emote for display
        /// </summary>
        /// <param name="p_Emote">Emote to enqueue</param>
        /// <param name="p_Count">Display count</param>
        /// <returns></returns>
        private IEnumerator EnqueueEmote(IChatEmote p_Emote, byte p_Count)
        {
            yield return null;

            if (   (Utils.Game.ActiveScene == Utils.Game.SceneType.Menu    && Config.ChatEmoteRain.MenuRain)
                || (Utils.Game.ActiveScene == Utils.Game.SceneType.Playing && Config.ChatEmoteRain.SongRain))
            {
                SharedCoroutineStarter.instance.StartCoroutine(StartParticleSystem(p_Emote.Id, p_Emote.IsAnimated, p_Count));
            }
        }
        /// <summary>
        /// Start particle system or update existing one for an Emote
        /// </summary>
        /// <param name="p_EmoteID">ID of the emote</param>
        /// <param name="p_EmoteIsAnimated">Is an animated emote ?</param>
        /// <param name="p_Count">Display count</param>
        /// <returns></returns>
        private IEnumerator<WaitUntil> StartParticleSystem(string p_EmoteID, bool p_EmoteIsAnimated, byte p_Count)
        {
            bool                l_IsManagedEmote    = !p_EmoteID.StartsWith("SubRainSprite_");
            EnhancedImageInfo   l_EnhancedImageInfo = default;

            if (l_IsManagedEmote)
                yield return new WaitUntil(() => ChatImageProvider.instance.CachedImageInfo.TryGetValue(p_EmoteID, out l_EnhancedImageInfo) && Utils.Game.ActiveScene != Game.SceneType.None);

            Components.TimeoutScript l_TimeoutScript;
            PrefabPair l_PrefabPair = m_ParticleSystems[Utils.Game.ActiveScene];

            if (!l_PrefabPair.Item1.ContainsKey(p_EmoteID) || !l_PrefabPair.Item1[p_EmoteID])
            {
                l_TimeoutScript         = GameObject.Instantiate(l_PrefabPair.Item2).GetComponent<Components.TimeoutScript>();
                l_TimeoutScript.key     = p_EmoteID;
                l_TimeoutScript.mode    = Utils.Game.ActiveScene;

                var l_ParticleSystem = l_TimeoutScript.PS.main;

                if (Utils.Game.ActiveScene == Utils.Game.SceneType.Menu)
                    l_ParticleSystem.startSize = Config.ChatEmoteRain.MenuRainSize;
                if (Utils.Game.ActiveScene == Utils.Game.SceneType.Playing)
                    l_ParticleSystem.startSize = Config.ChatEmoteRain.SongRainSize;

                l_ParticleSystem.startSpeed     = Config.ChatEmoteRain.EmoteFallSpeed;
                l_ParticleSystem.startLifetime  = (8 / (Config.ChatEmoteRain.EmoteFallSpeed - 1)) + 1;

                if (!l_PrefabPair.Item1.ContainsKey(p_EmoteID))
                    l_PrefabPair.Item1.Add(p_EmoteID, l_TimeoutScript);
                else
                    l_PrefabPair.Item1[p_EmoteID] = l_TimeoutScript;

                /// Sorta working animated emotes
                if (p_EmoteIsAnimated && l_IsManagedEmote)
                {
                    var l_TextureSheetAnimation = l_TimeoutScript.PS.textureSheetAnimation;
                    l_TextureSheetAnimation.enabled     = true;
                    l_TextureSheetAnimation.mode        = ParticleSystemAnimationMode.Sprites;
                    l_TextureSheetAnimation.timeMode    = ParticleSystemAnimationTimeMode.Lifetime;

                    int     l_SpriteCount   = l_EnhancedImageInfo.AnimControllerData.sprites.Length - 1;
                    float   l_TimeForEmote  = 0;
                    for (int l_I = 0; l_I < l_SpriteCount; ++l_I)
                    {
                        l_TextureSheetAnimation.AddSprite(l_EnhancedImageInfo.AnimControllerData.sprites[l_I]);
                        l_TimeForEmote += l_EnhancedImageInfo.AnimControllerData.delays[l_I];
                    }

                    AnimationCurve l_AnimationCurve = new AnimationCurve();
                    float l_SingleFramePercentage  = 1.0f / l_SpriteCount;
                    float l_CurrentTimePercentage  = 0;
                    float l_CurrentFramePercentage = 0;

                    for (int l_FrameCounter = 0; l_CurrentTimePercentage <= 1; ++l_FrameCounter)
                    {
                        if (l_FrameCounter > l_SpriteCount)
                        {
                            l_FrameCounter              = 0;
                            l_CurrentFramePercentage    = 0;
                        }

                        l_AnimationCurve.AddKey(l_CurrentTimePercentage, l_CurrentFramePercentage);

                        l_CurrentTimePercentage     += l_EnhancedImageInfo.AnimControllerData.delays[l_FrameCounter] / l_TimeForEmote;
                        l_CurrentFramePercentage    += l_SingleFramePercentage;
                    }

                    l_TextureSheetAnimation.frameOverTime   = new ParticleSystem.MinMaxCurve(1.0f, l_AnimationCurve);
                    l_TextureSheetAnimation.cycleCount      = (int)(l_TimeoutScript.PS.main.startLifetime.constant * 1000 / l_TimeForEmote);
                }

                if (l_IsManagedEmote)
                    l_TimeoutScript.PSR.material.mainTexture = l_EnhancedImageInfo.Sprite.texture;
                else if(p_EmoteID != "SubRainSprite_")
                    l_TimeoutScript.PSR.material.mainTexture = ChatEmoteRain.Instance.m_SubRainTextures[p_EmoteID.Substring("SubRainSprite_".Length)];
            }
            else
                l_TimeoutScript = l_PrefabPair.Item1[p_EmoteID];

            l_TimeoutScript.Emit(p_Count);
        }
        /// <summary>
        /// Filter emotes for combo
        /// </summary>
        /// <param name="p_Emotes">Emotes to filter</param>
        /// <returns></returns>
        private IChatEmote[] FilterEmotesForCombo(IChatMessage p_Message)
        {
            IChatEmote[] returner = null;
            if(Config.ChatEmoteRain.ComboModeType == 1) // Trigger type: 0 = Emote; 1 = User
            {
                IChatEmote[] l_Emotes = p_Message.Emotes;
                string l_Sender = p_Message.Sender.Id;

                List<IChatEmote> l_FirstFiltering = new List<IChatEmote>();
                foreach(IChatEmote l_CurrentEmote in l_Emotes)
                {
                    if(l_FirstFiltering.Count(x => x.Id == l_CurrentEmote.Id) == 0)
                        l_FirstFiltering.Add(l_CurrentEmote);
                }

                List<IChatEmote> l_SecondFiltering = new List<IChatEmote>();
                foreach (IChatEmote e in l_FirstFiltering)
                {
                    if (m_ComboState2.ContainsKey(e.Id))
                    {
                        if (Environment.TickCount - m_ComboState2[e.Id].Item2 < Config.ChatEmoteRain.ComboTimer * 1000 &&
                            !m_ComboState2[e.Id].Item1.Contains(l_Sender))
                        {
                            m_ComboState2[e.Id].Item1.Add(l_Sender);
                            m_ComboState2[e.Id] = new Tuple<List<string>, int>(m_ComboState2[e.Id].Item1, Environment.TickCount & int.MaxValue);
                        }
                        else
                        {
                            m_ComboState2.Remove(e.Id);
                            List<string> temp = new List<string>();
                            temp.Add(l_Sender);
                            m_ComboState2.Add(e.Id, new Tuple<List<string>, int>(temp, Environment.TickCount & int.MaxValue));
                        }

                        if(m_ComboState2[e.Id].Item1.Count >= Config.ChatEmoteRain.ComboCount)
                        {
                            l_SecondFiltering.Add(e);
                        }
                    }
                    else
                    {
                        List<string> temp = new List<string>();
                        temp.Add(l_Sender);
                        m_ComboState2.Add(e.Id, new Tuple<List<string>, int>(temp, Environment.TickCount & int.MaxValue));
                    }
                }
                returner = l_SecondFiltering.ToArray();
            }
            else
            {
                IChatEmote[] l_Emotes = p_Message.Emotes;

                List<IChatEmote> l_FirstFiltering = new List<IChatEmote>();
                foreach (IChatEmote l_CurrentEmote in l_Emotes)
                {
                    if (l_FirstFiltering.Count(x => x.Id == l_CurrentEmote.Id) == 0)
                        l_FirstFiltering.Add(l_CurrentEmote);
                }

                List<IChatEmote> l_SecondFiltering = new List<IChatEmote>();
                foreach (IChatEmote l_CurrentEmote in l_FirstFiltering)
                {
                    if (m_ComboState.ContainsKey(l_CurrentEmote.Id))
                    {
                        if (Environment.TickCount - m_ComboState[l_CurrentEmote.Id].Item2 < Config.ChatEmoteRain.ComboTimer * 1000)
                            m_ComboState[l_CurrentEmote.Id] = new Tuple<int, int>(m_ComboState[l_CurrentEmote.Id].Item1 + 1, Environment.TickCount & int.MaxValue);
                        else
                        {
                            m_ComboState.Remove(l_CurrentEmote.Id);
                            m_ComboState.Add(l_CurrentEmote.Id, new Tuple<int, int>(1, Environment.TickCount & int.MaxValue));
                        }

                        if (m_ComboState[l_CurrentEmote.Id].Item1 >= Config.ChatEmoteRain.ComboCount)
                            l_SecondFiltering.Add(l_CurrentEmote);
                    }
                    else
                        m_ComboState.Add(l_CurrentEmote.Id, new Tuple<int, int>(1, Environment.TickCount & int.MaxValue));
                }

                returner = l_SecondFiltering.ToArray();
            }

            return returner;
        }
    }
}
