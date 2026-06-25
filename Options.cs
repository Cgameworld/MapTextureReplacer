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
using System.Reflection;

namespace MapTextureReplacer
{
    [FileLocation(nameof(MapTextureReplacer))]
    [SettingsUIShowGroupName(DisplayGroup, CreatorToolsGroup, OtherGroup)]
    public class MapTextureReplacerOptions : ModSetting
    {
        public const string DisplayGroup = "DisplayGroup";
        public const string CreatorToolsGroup = "CreatorToolsGroup";
        public const string OtherGroup = "OtherGroup";

        private MapTextureReplacerSystem m_MapTextureReplacerSystem;
        private bool _InUniversalModMenu;
        private bool _ShowDownloadedPacks;
        private bool _ShowLocalPacks;

        public MapTextureReplacerOptions(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        [SettingsUISection(DisplayGroup)]
        public bool InUniversalModMenu
        {
            get => _InUniversalModMenu;
            set
            {
                if (_InUniversalModMenu == value) return;
                _InUniversalModMenu = value;
                RefreshUI();
            }
        }

        [SettingsUISection(DisplayGroup)]
        public bool ShowDownloadedPacks
        {
            get => _ShowDownloadedPacks;
            set
            {
                if (_ShowDownloadedPacks == value) return;
                _ShowDownloadedPacks = value;
                World.DefaultGameObjectInjectionWorld?
                    .GetExistingSystemManaged<MapTextureReplacerSystem>()?
                    .SerializeImportedPacksWithSource();
                RefreshUI();
            }
        }

        [SettingsUISection(DisplayGroup)]
        public bool ShowLocalPacks
        {
            get => _ShowLocalPacks;
            set
            {
                if (_ShowLocalPacks == value) return;
                _ShowLocalPacks = value;
                World.DefaultGameObjectInjectionWorld?
                    .GetExistingSystemManaged<MapTextureReplacerSystem>()?
                    .SerializeImportedPacksWithSource();
                RefreshUI();
            }
        }

        [SettingsUISection(CreatorToolsGroup)]
        public bool ShowCameraHeight { get; set; }

        [SettingsUISection(OtherGroup)]
        public string ModVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(OtherGroup)]
        public bool ResetModSettings
        {
            set
            {
                MakeSureSave = new System.Random().Next();
                ActiveDropdown = "none";
                TilingFloatData = "";
                CurrentTilingVector = Vector4.zero;

                m_MapTextureReplacerSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerSystem>();
                m_MapTextureReplacerSystem.ResetAll();
            }
        }

        [SettingsUIHidden]
        public string ActiveDropdown { get; set; }

        [SettingsUIHidden]
        public string TextureSelectData { get; set; }

        //serialized { fieldName: value } of the terrain render settings floats, reapplied on load
        [SettingsUIHidden]
        public string TilingFloatData { get; set; }

        //migration-only: absorbs the old Vector4 tiling from pre-1.6.0 saves so it can be converted
        //to TilingFloatData once on load, then zeroed and never written again
        [SettingsUIHidden]
        public Vector4 CurrentTilingVector { get; set; }

        //sometimes saving doesn't happen when changing values to their default? - hack to guarantee
        [SettingsUIHidden]
        public int MakeSureSave { get; set; }
        public override void SetDefaults()
        {
            MakeSureSave = 0;
            InUniversalModMenu = false;
            ShowDownloadedPacks = true;
            ShowLocalPacks = true;
            ShowCameraHeight = false;
            CurrentTilingVector = Vector4.zero;
        }
        private static void RefreshUI()
        {
            GameManager.instance?.userInterface?.view?.View?.ExecuteScript("window.location.reload();");
        }
    }
}
