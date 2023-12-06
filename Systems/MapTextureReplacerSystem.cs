using Game;
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
        Dictionary<string, Texture> mapTextureCache = new Dictionary<string, Texture>();

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

            var existingTexture = Shader.GetGlobalTexture(Shader.PropertyToID(shaderProperty));
            if (!mapTextureCache.ContainsKey(shaderProperty))
            {
                mapTextureCache.Add(shaderProperty, existingTexture);
            }

            byte[] fileData;

            if (!string.IsNullOrEmpty(file))
            {
                fileData = File.ReadAllBytes(file);
                LoadTextureInGame(shaderProperty, fileData);
            }

        }

        private static void LoadTextureInGame(string shaderProperty, byte[] fileData)
        {
            Texture2D newTexture = new Texture2D(4096, 4096);
            newTexture.LoadImage(fileData);
            Shader.SetGlobalTexture(Shader.PropertyToID(shaderProperty), newTexture);
        }

        public void ResetTexture(string shaderProperty)
        {
            mapTextureCache.TryGetValue(shaderProperty, out Texture texture);
            if (texture != null)
            {
                Shader.SetGlobalTexture(Shader.PropertyToID(shaderProperty), texture);
            }
        }

        public void OpenTextureZip()
        {
            var zipFilePath = OpenFileDialog.ShowDialog("Zip archives\0*.zip\0");

            if (!string.IsNullOrEmpty(zipFilePath))
            {
                using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry entry = archive.GetEntry("Grass_BaseColor.png");

                    if (entry != null)
                    {
                        using (Stream entryStream = entry.Open())
                        {
                            Debug.Log("called entry!!");
                            byte[] data = new byte[entry.Length];
                            entryStream.Read(data, 0, data.Length);
                            LoadTextureInGame("colossal_TerrainGrassDiffuse", data);
                        }
                    }
                }
            }

        }
    }
}
