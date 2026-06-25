using Colossal.IO.AssetDatabase;
using Game;
using Game.Assets;
using Game.SceneFlow;
using HarmonyLib;
using MapTextureReplacer.Helpers;
using MapTextureReplacer.Systems;
using System;
using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace MapTextureReplacer.Patches
{
    [HarmonyPatch(typeof(GameManager))]
    public static class SavePatch
    {
        //saving can rewrite the terrain shader globals; re-assert the mod's overrides afterwards
        [HarmonyPatch("Save", new Type[] { typeof(string), typeof(SaveInfo), typeof(ILocalAssetDatabase), typeof(Texture) })]
        [HarmonyPostfix]
        public static void Postfix()
        {
            StaticCoroutine.Start(ReapplyAfterSave());
        }

        static IEnumerator ReapplyAfterSave()
        {
            yield return new WaitForEndOfFrame();
            World.DefaultGameObjectInjectionWorld?
                .GetExistingSystemManaged<MapTextureReplacerSystem>()?
                .ApplyAllOverrides();
        }
    }
}
