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
        private MapTextureReplacerSystem m_MapTextureReplacerSystem;

        public MapTextureReplacerOptions(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        [SettingsUIButton]
        [SettingsUIConfirmation]
        public bool ResetModSettings
        {
            set
            {
                MakeSureSave = new System.Random().Next();
                TextureSelectData = "[{\"Key\":\"Default\",\"Value\":\"none\"},{\"Key\":\"Default\",\"Value\":\"none\"},{\"Key\":\"Default\",\"Value\":\"none\"},{\"Key\":\"Default\",\"Value\":\"none\"},{\"Key\":\"Default\",\"Value\":\"none\"},{\"Key\":\"Default\",\"Value\":\"none\"}]";
                ActiveDropdown = "none";
                CurrentTilingVector = Vector4.zero;

                m_MapTextureReplacerSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerSystem>();
                m_MapTextureReplacerSystem.ChangePack("none");

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
        }
    }
}
