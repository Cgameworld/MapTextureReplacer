using Game.Rendering;
using HarmonyLib;
using MapTextureReplacer.Systems;
using Unity.Entities;

namespace MapTextureReplacer.Patches
{
    //the game rewrites every terrain shader global in ApplyRenderSettings (theme change, save deserialize,
    //graphics/mip change); re-assert the mod's overrides right after so they stay durable
    [HarmonyPatch(typeof(TerrainMaterialSystem), "ApplyRenderSettings")]
    public static class TerrainMaterialSystem_ApplyRenderSettings
    {
        static void Postfix()
        {
            World.DefaultGameObjectInjectionWorld?
                .GetExistingSystemManaged<MapTextureReplacerSystem>()?
                .ApplyAllOverrides();
        }
    }
}
