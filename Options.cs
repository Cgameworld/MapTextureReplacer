using MapTextureReplacer.Locale;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using Game;
using System;
using MapTextureReplacer.Systems;
using Unity.Entities;
using MapTextureReplacer.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace MapTextureReplacer
{
    [FileLocation(nameof(MapTextureReplacer))]
    public class MapTextureReplacerOptions : ModSetting
    {
        //private MapTextureReplacerSystem m_MapTextureReplacerSystem;
        private bool persistentSettings;

        public MapTextureReplacerOptions(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        [SettingsUISection("PersistentSettings")]
        public bool PersistentSettings
        {
            get => persistentSettings;
            set
            {
                persistentSettings = value;
                MakeSureSave = new System.Random().Next();
            }
        }

        [SettingsUIHidden]
        public string ActiveDropdown { get; set; }

        [SettingsUIHidden]
        public string TextureSelectData { get; set; }

        [SettingsUIHidden]
        public Vector4 CurrentTilingVector { get; set; }

        //sometimes saving doesn't happen when changing values to their default? - hack to guarantee
        [SettingsUIHidden]
        public int MakeSureSave { get; set; }


        public override void SetDefaults()
        {
            MakeSureSave = 0;
            PersistentSettings = true;
        }
    }
    public class MapTextureReplacerMod : IMod
    {
        public static MapTextureReplacerOptions Options { get; set; }

        public static MapTextureReplacerMod Instance { get; private set; }

        public void OnDisable()
        {
        }

        public void OnDispose()
        {
        }

        public void OnEnable()
        {
        }

        public void OnLoad()
        {
            Instance = this;
        }
        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            UnityEngine.Debug.Log("MapTextureReplacer Options Loaded");
            Options = new(this);
            Options.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings("MapTextureOptions", Options, new MapTextureReplacerOptions(this));

            foreach (var lang in GameManager.instance.localizationManager.GetSupportedLocales())
            {
                GameManager.instance.localizationManager.AddSource(lang, new LocaleEN(Options));
            }

            AssetDatabase.global.SaveSettingsNow();
        }
    }
}
