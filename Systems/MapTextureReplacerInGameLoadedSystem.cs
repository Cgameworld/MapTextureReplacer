using Game;
using MapTextureReplacer.Helpers;
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
        protected override void OnCreate()
        {          
        }

        public void RunAction()
        {
            m_mapTextureReplacerSystem = World.GetOrCreateSystemManaged<MapTextureReplacerSystem>();
            m_mapTextureTextureCacheSystem = World.GetOrCreateSystemManaged<MapTextureReplacerTextureCacheSystem>();

            base.OnCreate();

            m_mapTextureTextureCacheSystem.StartCache();
            StaticCoroutine.Start(ReapplyTexture(m_mapTextureReplacerSystem));
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
            


            yield break;
        }
        protected override void OnUpdate()
        {

        }
    }
}
