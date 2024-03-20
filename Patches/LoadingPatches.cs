using Game.Audio;
using Game.Common;
using Game;
using HarmonyLib;
using MapTextureReplacer.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace MapTextureReplacer.Patches
{
    [HarmonyPatch(typeof(SystemOrder), nameof(SystemOrder.Initialize))]
    internal class InjectSystemsPatch
    {
        //sometimes crashes if the patch runs after goee loads?
        [HarmonyBefore(new string[] { "Gooee_Cities2Harmony" })]
        static void Postfix(UpdateSystem updateSystem)
        {
            updateSystem.UpdateAt<MapTextureReplacerSystem>(SystemUpdatePhase.PostSimulation);
            updateSystem.UpdateAt<MapTextureReplacerUISystem>(SystemUpdatePhase.UIUpdate);
        }
    }

    [HarmonyPatch(typeof(AudioManager), "OnGameLoadingComplete")]
    internal class AudioManager_OnGameLoadingComplete
    {
        static void Postfix(AudioManager __instance, Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {

            if (!mode.IsGameOrEditor())
                return;
            World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerInGameLoadedSystem>().RunAction();

            if (mode.IsEditor())
            {
                World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerEditorUISystem>().CreateAssetEditorButton();
            }
        }
    }
}
