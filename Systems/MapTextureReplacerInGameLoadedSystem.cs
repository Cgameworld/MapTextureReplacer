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
        private MapTextureReplacerTextureCacheSystem m_mapTextureTextureCacheSystem;
        private PrefabSystem m_prefabSystem;

        protected override void OnCreate()
        {          
        }

        public void RunAction()
        {
            m_mapTextureReplacerSystem = World.GetOrCreateSystemManaged<MapTextureReplacerSystem>();
            m_mapTextureTextureCacheSystem = World.GetOrCreateSystemManaged<MapTextureReplacerTextureCacheSystem>();
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


            //cache existing textures?
            m_mapTextureTextureCacheSystem.StartCache();
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

            List<string> textureTypeKeys = new List<string>(m_mapTextureReplacerSystem.textureTypes.Keys);
            for (int i = 0; i < textureTypeKeys.Count; i++)
            {
                //if filepath none, don't reapply
                if (m_mapTextureReplacerSystem.textureSelectData[i].Value != "none")
                {
                    m_mapTextureReplacerSystem.OpenImage(textureTypeKeys[i], m_mapTextureReplacerSystem.textureSelectData[i].Value);
                }
            }

            //apply tiling values

            if (Mod.Options.CurrentTilingVector != Vector4.zero)
            {
                Shader.SetGlobalVector(Shader.PropertyToID("colossal_TerrainTextureTiling"), Mod.Options.CurrentTilingVector);
            }

            m_mapTextureReplacerSystem.RestoreFarTilingBreakpoints();

            yield break;
        }
        protected override void OnUpdate()
        {

        }
    }
}
