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
        var m_mapTextureReplacerUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerUISystem>();

        m_mapTextureReplacerUISystem.SetWindowOpen(false);

        foreach (var item in m_mapTextureReplacerSystem.textureTypes)
        {
            m_mapTextureReplacerTextureCacheSystem.mapTextureCache.TryGetValue(item.Key, out Texture texture);

            if (texture != null)
            {
                Shader.SetGlobalTexture(Shader.PropertyToID(item.Key), texture);
            }
        }
        //tiling no longer reset here: saved float values are reapplied on the next map load
        //(MapTextureReplacerInGameLoadedSystem), and routing a reset through ChangeFloatField
        //would overwrite the persisted tiling.

        Mod.log.Info("Restored Texture Cache");
    }
}