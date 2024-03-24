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

    [HarmonyPatch(typeof(AudioManager), "OnGameLoadingComplete")]
    internal class AudioManager_OnGameLoadingComplete
    {
        static void Postfix(AudioManager __instance, Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {

            if (!mode.IsGameOrEditor())
                return;
            World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerInGameLoadedSystem>().RunAction(); World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerLegacyUIInjectSystem>().SpawnMainWindow();

            if (mode.IsEditor())
            {
                World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<MapTextureReplacerEditorUISystem>().CreateAssetEditorButton();
            }

        }
    }
}
