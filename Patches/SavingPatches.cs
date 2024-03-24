using Colossal.IO.AssetDatabase;
using Game;
using Game.Assets;
using Game.Audio;
using Game.Common;
using Game.SceneFlow;
using HarmonyLib;
using MapTextureReplacer.Helpers;
using MapTextureReplacer.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Entities;
using UnityEngine;

namespace MapTextureReplacer.Patches
{

    [HarmonyPatch(typeof(GameManager))]
    public static class SavePatch
    {
        static Texture[] setTexturesCache = new Texture2D[6];
        static readonly string[] propertyNames = new string[]
        {
            "colossal_TerrainGrassDiffuse",
            "colossal_TerrainGrassNormal",
            "colossal_TerrainDirtDiffuse",
            "colossal_TerrainDirtNormal",
            "colossal_TerrainRockDiffuse",
            "colossal_TerrainRockNormal"
        };

        [HarmonyPatch("Save", new Type[] { typeof(string), typeof(SaveInfo), typeof(ILocalAssetDatabase), typeof(Texture) })]
        [HarmonyPrefix]
        public static void Prefix()
        {
            for (int i = 0; i < setTexturesCache.Length; i++)
            {
                setTexturesCache[i] = Shader.GetGlobalTexture(Shader.PropertyToID(propertyNames[i]));
            }
        }

        //hacky way for now

        //don't know how to properly patch this? after savegame method runs, texture is not reset, but sometime after before the next frame is rendered? Is something special required with harmony regarding async methods since this method calls them? If you know a better way please make a new issue

        [HarmonyPatch("Save", new Type[] { typeof(string), typeof(SaveInfo), typeof(ILocalAssetDatabase), typeof(Texture) })]
        [HarmonyPostfix]
        public static void Postfix()
        {
            StaticCoroutine.Start(ReapplyTexture());
        }
        static IEnumerator ReapplyTexture()
        {
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < setTexturesCache.Length; i++)
            {
                Shader.SetGlobalTexture(Shader.PropertyToID(propertyNames[i]), setTexturesCache[i]);
                Mod.log.Info("Map Textures Reapplied After Save");
            }
            yield break;
        }
    }
}