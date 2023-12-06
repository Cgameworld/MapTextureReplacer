using Game;
using Game.Common;
using HarmonyLib;
using MapTextureReplacer.Systems;

namespace MapTextureReplacer.Patches
{
    [HarmonyPatch(typeof(SystemOrder))]
    public static class SystemOrderPatch
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        public static void Postfix(UpdateSystem updateSystem)
        {
            updateSystem.UpdateAt<VehicleCounterSystem>(SystemUpdatePhase.PostSimulation);
            updateSystem.UpdateAt<VehicleCounterUISystem>(SystemUpdatePhase.UIUpdate);
        }
    }
}