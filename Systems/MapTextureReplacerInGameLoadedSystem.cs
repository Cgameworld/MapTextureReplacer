using Colossal.IO.AssetDatabase;
using Game;
using Game.Prefabs;
using Game.Prefabs.Terrain;
using Game.UI.InGame;
using MapTextureReplacer.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

namespace MapTextureReplacer.Systems
{
    public partial class MapTextureReplacerInGameLoadedSystem : GameSystemBase
    {
        private MapTextureReplacerSystem m_mapTextureReplacerSystem;
        private PrefabSystem m_prefabSystem;

        protected override void OnCreate()
        {
        }

        public void RunAction()
        {
            m_mapTextureReplacerSystem = World.GetOrCreateSystemManaged<MapTextureReplacerSystem>();
            m_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            base.OnCreate();

            //grab asset packs

            //check local
            foreach (PrefabAsset asset in AssetDatabase.user.GetAssets(default(SearchFilter<PrefabAsset>)))
            {
                CheckPrefabs(asset, "local");
            }

            //check downloaded from paradox mods
            foreach (PrefabAsset asset in AssetDatabase<ParadoxMods>.instance.GetAssets(default(SearchFilter<PrefabAsset>)))
            {
                CheckPrefabs(asset, "pdx");
            }

            StaticCoroutine.Start(ReapplyTexture(m_mapTextureReplacerSystem));
        }
        private void CheckPrefabs(PrefabAsset asset, string source)
        {
            PrefabBase prefab = asset.Load<PrefabBase>(Array.Empty<IAssetDatabase>());

            if (prefab is TerrainRenderSettingsPrefab settings && settings != null)
            {
                Mod.log.Info(prefab.GetPrefabID().ToString());

                string key = prefab.GetPrefabID().ToString();
                if (m_mapTextureReplacerSystem.importedPacks.TryAdd(key, settings.name))
                {
                    m_mapTextureReplacerSystem.packSources[key] = source;
                    m_mapTextureReplacerSystem.packValidSlots[key] = m_mapTextureReplacerSystem.ValidSlotsForPrefab(settings);
                }
                m_mapTextureReplacerSystem.SerializeImportedPacksWithSource();
            }
        }

        static IEnumerator ReapplyTexture(MapTextureReplacerSystem m_mapTextureReplacerSystem)
        {
            //wait for the game world textures to show?
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //snapshot this map's pristine float defaults (for Reset) before applying any saved tiling
            m_mapTextureReplacerSystem.CaptureFloatDefaults();
            m_mapTextureReplacerSystem.ApplySavedTiling();
            m_mapTextureReplacerSystem.PrepareTextureFloatSliders();

            for (int i = 0; i < m_mapTextureReplacerSystem.textureSelectData.Count; i++)
            {
                if (m_mapTextureReplacerSystem.textureSelectData[i].Value != "none")
                {
                    m_mapTextureReplacerSystem.OpenImage(i, m_mapTextureReplacerSystem.textureSelectData[i].Value);
                }
            }

            yield break;
        }
        protected override void OnUpdate()
        {

        }
    }
}
