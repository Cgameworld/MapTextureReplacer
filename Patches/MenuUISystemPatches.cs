using Game.SceneFlow;
using Game.UI.Menu;
using HarmonyLib;
using MapTextureReplacer;
using MapTextureReplacer.Systems;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

[HarmonyPatch(typeof(MenuUISystem), "ExitToMainMenu")]
public static class MenuUISystem_ExitToMainMenu_Prefix
{
    static void Prefix(MenuUISystem __instance)
    {
        var m_mapTextureReplacerSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerSystem>();
        var m_mapTextureReplacerTextureCacheSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerTextureCacheSystem>();

        foreach (var item in m_mapTextureReplacerSystem.textureTypes)
        {
            m_mapTextureReplacerTextureCacheSystem.mapTextureCache.TryGetValue(item.Key, out Texture texture);

            if (texture != null)
            {
                Shader.SetGlobalTexture(Shader.PropertyToID(item.Key), texture);
            }
        }
        Shader.SetGlobalVector(Shader.PropertyToID("colossal_TerrainTextureTiling"), new Vector4(160, 1600, 2400, 0));


        Mod.log.Info("Restored Texture Cache");
    }
}