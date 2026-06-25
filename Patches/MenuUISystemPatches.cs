using Game.UI.Menu;
using HarmonyLib;
using MapTextureReplacer;
using MapTextureReplacer.Systems;
using Unity.Entities;

[HarmonyPatch(typeof(MenuUISystem), "ExitToMainMenu")]
public static class MenuUISystem_ExitToMainMenu_Prefix
{
    static void Prefix(MenuUISystem __instance)
    {
        var sys = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerSystem>();
        var ui = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerUISystem>();

        ui?.SetWindowOpen(false);

        //drop overrides + restore pristine tiling so the shared prefab doesn't leak into the next map;
        //saved selections/tiling reapply on the next load (MapTextureReplacerInGameLoadedSystem)
        sys?.OnExitToMenu();

        Mod.log.Info("Restored vanilla terrain on exit to menu");
    }
}
