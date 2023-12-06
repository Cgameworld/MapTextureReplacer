﻿using Game;
using Game.UI;
using MapTextureReplacer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace MapTextureReplacer.Systems
{
    public class MapTextureReplacerSystem : GameSystemBase
    {
        static Dictionary<string, Texture> mapTextureCache = new Dictionary<string, Texture>();

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {

        }
        public void OpenImage(string shaderProperty)
        {
            var file = OpenFileDialog.ShowDialog("Image files\0*.jpg;*.png\0");

            CacheExistingTexture(shaderProperty);

            byte[] fileData;

            if (!string.IsNullOrEmpty(file))
            {
                fileData = File.ReadAllBytes(file);
                LoadTextureInGame(shaderProperty, fileData);
            }

        }
        public void OpenTextureZip()
        {
            var zipFilePath = OpenFileDialog.ShowDialog("Zip archives\0*.zip\0");

            if (!string.IsNullOrEmpty(zipFilePath))
            {
                using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Read))
                {
                    ExtractEntry(archive, "Grass_BaseColor.png", "colossal_TerrainGrassDiffuse");
                    ExtractEntry(archive, "Grass_Normal.png", "colossal_TerrainGrassNormal");
                    ExtractEntry(archive, "Dirt_BaseColor.png", "colossal_TerrainDirtDiffuse");
                    ExtractEntry(archive, "Dirt_Normal.png", "colossal_TerrainDirtNormal");
                    ExtractEntry(archive, "Cliff_BaseColor.png", "colossal_TerrainRockDiffuse");
                    ExtractEntry(archive, "Cliff_Normal.png", "colossal_TerrainRockNormal");
                }
            }

        }
        private static void CacheExistingTexture(string shaderProperty)
        {
            var existingTexture = Shader.GetGlobalTexture(Shader.PropertyToID(shaderProperty));
            if (!mapTextureCache.ContainsKey(shaderProperty))
            {
                mapTextureCache.Add(shaderProperty, existingTexture);
            }
        }

        private static void LoadTextureInGame(string shaderProperty, byte[] fileData)
        {
            Texture2D newTexture = new Texture2D(4096, 4096);
            newTexture.LoadImage(fileData);
            Shader.SetGlobalTexture(Shader.PropertyToID(shaderProperty), newTexture);
            Debug.Log("Replaced " + shaderProperty + " ingame");
        }

        public void ResetTexture(string shaderProperty)
        {
            mapTextureCache.TryGetValue(shaderProperty, out Texture texture);
            if (texture != null)
            {
                Shader.SetGlobalTexture(Shader.PropertyToID(shaderProperty), texture);
            }
        }

       
        private static void ExtractEntry(ZipArchive archive, string entryName, string shaderProperty)
        {
            CacheExistingTexture(shaderProperty);

            ZipArchiveEntry entry = archive.GetEntry(entryName);

            if (entry != null)
            {
                using (Stream entryStream = entry.Open())
                {
                    byte[] data = new byte[entry.Length];
                    entryStream.Read(data, 0, data.Length);
                    LoadTextureInGame(shaderProperty, data);
                }
            }
        }
    }
}