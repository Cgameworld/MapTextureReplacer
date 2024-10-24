﻿using Colossal.IO.AssetDatabase;
using Game;
using MapTextureReplacer.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace MapTextureReplacer.Systems
{
    public partial class MapTextureReplacerSystem : GameSystemBase
    {
        private MapTextureReplacerTextureCacheSystem m_mapTextureTextureCacheSystem;

        public string PackImportedText = "";

        private Dictionary<string, string> importedPacks = new Dictionary<string, string>();
        public string importedPacksJsonString;
        
        public string textureSelectDataJsonString;
        public List<KeyValuePair<string, string>> textureSelectData = new List<KeyValuePair<string, string>>() {
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            };
        
        private static bool isOver;
        
        public readonly Dictionary<string, string> textureTypes = new Dictionary<string, string>() {
            {"colossal_TerrainGrassDiffuse", "Grass_BaseColor.png"},
            {"colossal_TerrainGrassNormal", "Grass_Normal.png"},
            {"colossal_TerrainDirtDiffuse", "Dirt_BaseColor.png"},
            {"colossal_TerrainDirtNormal", "Dirt_Normal.png"},
            {"colossal_TerrainRockDiffuse", "Cliff_BaseColor.png"},
            {"colossal_TerrainRockNormal", "Cliff_Normal.png"},
        };

        protected override void OnCreate()
        {
            base.OnCreate();

            m_mapTextureTextureCacheSystem = World.GetOrCreateSystemManaged<MapTextureReplacerTextureCacheSystem>();

            //initialize textureTypes
            if (Mod.Options.TextureSelectData == null)
            {
                //UnityEngine.Debug.Log("MapTextureReplacerMod.Options.TextureSelectData == null");
                Mod.Options.ActiveDropdown = "none";
                SetTextureSelectDataJson();
            }
            else
            {
                //Mod.log.Info("Mod.Options.TextureSelectData NOT null" + Mod.Options.TextureSelectData);

                textureSelectData = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(Mod.Options.TextureSelectData);
            }

            List<string> texturePackFolders = new List<string>();

            DirectoryInfo modsFolderDirectory = Directory.GetParent(Directory.GetParent(Mod.ModPath).FullName);

            Mod.log.Info("modsFolderDirectory.FullName: " + modsFolderDirectory.FullName);

            //find folders that contain pack config json files
            foreach (string filePath in Directory.GetFiles(modsFolderDirectory.FullName, "maptextureconfig.json", SearchOption.AllDirectories))
            {
                Mod.log.Info($"{filePath}");
                var filename = Path.GetFileName(filePath);              
                if (filename == "maptextureconfig.json")
                {            
                    texturePackFolders.Add(Directory.GetParent(filePath).FullName);
                }
            }

            //read pack config json files in the folders that have them
            foreach (var folder in texturePackFolders)
            {
                foreach (string filePath in Directory.GetFiles(folder))
                {
                    var filename = Path.GetFileName(filePath);
                    if (filename == "maptextureconfig.json")
                    {
                        try
                        {
                            MapTextureConfig mapTheme = JsonConvert.DeserializeObject<MapTextureConfig>(File.ReadAllText(filePath));
                            importedPacks.Add(filePath, mapTheme.pack_name);
                        }
                        catch (IOException ex)
                        {
                            Mod.log.Error($"Failed to read .json file {filePath} due to: {ex.Message}");
                        }
                    }
                }
            }
            importedPacksJsonString = JsonConvert.SerializeObject(importedPacks);

            //populate string
            textureSelectDataJsonString = JsonConvert.SerializeObject(textureSelectData);
        }

        protected override void OnUpdate()
        {

        }
        public void ChangePack(string current)
        {

            if (current == "none")
            {
                foreach (var item in textureTypes)
                {
                    ResetTexture(item.Key);
                }
                SetTilingValueDefault();
                //SetSelectImageAllText("Select Image");
            }
            else
            {
                if (current.EndsWith(".zip"))
                {
                    OpenTextureZip(current.Split(',')[1]);
                    SetSelectImageAllText(current.Split(',')[0], current);
                }
                else if (current.EndsWith(".json"))
                {
                    var directory = Path.GetDirectoryName(current);
                    Mod.log.Info("loading (json) folder: " + directory);

                    foreach (string filePath in Directory.GetFiles(directory))
                    {
                        foreach (var item in textureTypes)
                        {
                            LoadImageFile(filePath, item.Value, item.Key);
                        }
                    }

                    MapTextureConfig config = JsonConvert.DeserializeObject<MapTextureConfig>(File.ReadAllText(current));
                    SetTilingValues(config.far_tiling, config.close_tiling, config.close_dirt_tiling);                   
                    SetSelectImageAllText(config.pack_name, current);
                }
            }
        }

        private void SetSelectImageAllText(string key, string path)
        {
            //set select image text labels
            for (int i = 0; i < textureSelectData.Count; i++)
            {
                textureSelectData[i] = new KeyValuePair<string, string>(key, path);
            }
            SetTextureSelectDataJson();
        }

        private void SetTextureSelectDataJson()
        {
            //Debug.Log("SetTextureSelectDataJson() Called");
            textureSelectDataJsonString = JsonConvert.SerializeObject(textureSelectData);
            Mod.Options.TextureSelectData = textureSelectDataJsonString;
            AssetDatabase.global.SaveSettings();
        }

        public void SetTilingValues(string far, string close, string dirtClose)
        {
            //Shader.SetGlobalVector(Shader.PropertyToID("colossal_TerrainTextureTiling"), new Vector4(float.Parse(far), float.Parse(close), float.Parse(dirtClose), 1f));

            TileVectorChange("colossal_TerrainTextureTiling", 0, int.Parse(far));
            TileVectorChange("colossal_TerrainTextureTiling", 1, int.Parse(close));
            TileVectorChange("colossal_TerrainTextureTiling", 2, int.Parse(dirtClose));
        }
        public void SetTilingValueDefault()
        {
            //Shader.SetGlobalVector(Shader.PropertyToID("colossal_TerrainTextureTiling"), new Vector4(160f, 1600f, 2400f, 1f));

            TileVectorChange("colossal_TerrainTextureTiling", 0, 160);
            TileVectorChange("colossal_TerrainTextureTiling", 1, 1600);
            TileVectorChange("colossal_TerrainTextureTiling", 2, 2400);
        }
        private static void LoadImageFile(string filePath, string textureFile, string shaderProperty)
        {
            if (Path.GetFileName(filePath) == textureFile)
            {
                byte[] data = File.ReadAllBytes(filePath);

                LoadTextureInGame(shaderProperty, data);
            }
        }

        public void OpenImage(string shaderProperty, string packPath)
        {
            var filenameTexture = "";
            foreach (var item in textureTypes)
            {
                if (item.Key == shaderProperty)
                {
                    filenameTexture = item.Value;
                }
            }

            if (packPath == "")
            {
                var file = OpenFileDialog.ShowDialog("Image files\0*.jpg;*.png\0");

                byte[] fileData;

                if (!string.IsNullOrEmpty(file))
                {
                    fileData = File.ReadAllBytes(file);
                    LoadTextureInGame(shaderProperty, fileData);

                    int index = textureTypes.Keys.ToList().IndexOf(shaderProperty);
                    string fileName = ShortenDisplayedFilename(file);
                    textureSelectData[index] = new KeyValuePair<string, string>(fileName, file);
                    SetTextureSelectDataJson();
                }
            }
            else if (packPath.EndsWith(".zip"))
            {
                int index = textureTypes.Keys.ToList().IndexOf(shaderProperty);
                textureSelectData[index] = new KeyValuePair<string, string>(packPath.Split(',')[0],packPath);
                SetTextureSelectDataJson();

                using (ZipArchive archive = ZipFile.Open(packPath.Split(',')[1], ZipArchiveMode.Read))
                {
                    ExtractEntry(archive, filenameTexture, shaderProperty);
                }
            }
            else
            {
                int index = textureTypes.Keys.ToList().IndexOf(shaderProperty);

                string labelName = importedPacks.TryGetValue(packPath, out string value) ? value :  ShortenDisplayedFilename(Path.GetFileName(packPath));

               //Debug.Log("packPath: " + packPath);
                //Debug.Log("importedPacks[packPath]:  " + labelName);

                textureSelectData[index] = new KeyValuePair<string, string>(labelName, packPath);
                SetTextureSelectDataJson();

                if (packPath.EndsWith(".json"))
                {
                    var directory = Path.GetDirectoryName(packPath);

                    foreach (string filePath in Directory.GetFiles(directory))
                    {
                        LoadImageFile(filePath, filenameTexture, shaderProperty);
                    }
                }
                else
                {
                    byte[] data = File.ReadAllBytes(packPath);
                    LoadTextureInGame(shaderProperty, data);
                }
            }
            
        }

        private static string ShortenDisplayedFilename(string file)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.Length > 15)
            {
                fileName = fileName.Substring(0, 15);
            }

            return fileName;
        }

        public void GetTextureZip()
        {
            var zipFilePath = OpenFileDialog.ShowDialog("Zip archives\0*.zip\0");
            PackImportedText = Path.GetFileNameWithoutExtension(zipFilePath) + "," + zipFilePath;
        }
        public void OpenTextureZip(string zipFilePath)
        {
            //var zipFilePath = OpenFileDialog.ShowDialog("Zip archives\0*.zip\0");
            
            if (!string.IsNullOrEmpty(zipFilePath))
            {
                using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Read))
                {
                    List<string> notFoundFiles = new List<string>();

                    if (!ExtractEntry(archive, "Grass_BaseColor.png", "colossal_TerrainGrassDiffuse"))
                        notFoundFiles.Add("Grass_BaseColor.png");
                    if (!ExtractEntry(archive, "Grass_Normal.png", "colossal_TerrainGrassNormal"))
                        notFoundFiles.Add("Grass_Normal.png");
                    if (!ExtractEntry(archive, "Dirt_BaseColor.png", "colossal_TerrainDirtDiffuse"))
                        notFoundFiles.Add("Dirt_BaseColor.png");
                    if (!ExtractEntry(archive, "Dirt_Normal.png", "colossal_TerrainDirtNormal"))
                        notFoundFiles.Add("Dirt_Normal.png");
                    if (!ExtractEntry(archive, "Cliff_BaseColor.png", "colossal_TerrainRockDiffuse"))
                        notFoundFiles.Add("Cliff_BaseColor.png");
                    if (!ExtractEntry(archive, "Cliff_Normal.png", "colossal_TerrainRockNormal"))
                        notFoundFiles.Add("Cliff_Normal.png");

                    //change this!
                    if (notFoundFiles.Count > 0)
                    {
                        string outputError = "Files not found in .zip file:\n";
                        foreach (string file in notFoundFiles)
                        {
                            outputError = outputError + "\n" + file;
                        }


                        throw new Exception(outputError + "\n\n");
                    }

                    //add json file reading??
                }

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
            m_mapTextureTextureCacheSystem.mapTextureCache.TryGetValue(shaderProperty, out Texture texture);
            if (texture != null)
                {
                    Shader.SetGlobalTexture(Shader.PropertyToID(shaderProperty), texture);
                }
                //reset neighboring button text to select image
                int index = textureTypes.Keys.ToList().IndexOf(shaderProperty);
                textureSelectData[index] = new KeyValuePair<string, string>("Default", "none");
                SetTextureSelectDataJson();
        }

        private static bool ExtractEntry(ZipArchive archive, string entryName, string shaderProperty)
        {
            ZipArchiveEntry entry = archive.GetEntry(entryName);

            if (entry != null)
            {
                using (Stream entryStream = entry.Open())
                {
                    byte[] data = new byte[entry.Length];
                    entryStream.Read(data, 0, data.Length);
                    LoadTextureInGame(shaderProperty, data);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetTile(int v)
        {
            //UnityEngine.Debug.Log("SetTile Pressed!");
            //UnityEngine.Debug.Log("BF colossal_TerrainTextureTiling: " + Shader.GetGlobalVector(Shader.PropertyToID("colossal_TerrainTextureTiling")));
            Shader.SetGlobalVector(Shader.PropertyToID("colossal_TerrainTextureTiling"), new Vector4(new System.Random().Next(0, 10000), new System.Random().Next(0, 10000), new System.Random().Next(0, 10000), 1f));


        }

        public void ResetTextureSelectData()
        {
            textureSelectData = new List<KeyValuePair<string, string>>() {
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            new KeyValuePair<string, string>("Default", "none"),
            };


            SetTextureSelectDataJson();
        }

        public void SetTexturesGameLoad()
        {
            //change textures on load based on saved COC data
            List<string> textureTypeKeys = new List<string>(textureTypes.Keys);
            for (int i = 0; i < textureTypeKeys.Count; i++)
            {
                OpenImage(textureTypeKeys[i], textureSelectData[i].Value);
            }
        }

            public void SetActivePackDropdown(string data)
        {
            try
            {
                Mod.Options.ActiveDropdown = data;
                AssetDatabase.global.SaveSettings();
            }
            catch
            {
                Mod.log.Info("ActivePackDropdown Call Cancelled");
            }
        }

        public string GetActivePackDropdown()
        {
            return Mod.Options.ActiveDropdown;
        }

        public void TileVectorChange(string shaderProperty, int vectorIndex, int tileValue)
        {
            int propertyID = Shader.PropertyToID(shaderProperty);
            Vector4 currentVector = Shader.GetGlobalVector(propertyID);
            currentVector[vectorIndex] = tileValue;
            Shader.SetGlobalVector(propertyID, currentVector);
            Mod.Options.CurrentTilingVector = currentVector;
            if (!isOver)
            {
                StaticCoroutine.Start(SaveSettingsOnceAfterDelay());
            }
            isOver = true;
                        
        }

        static IEnumerator SaveSettingsOnceAfterDelay()
        {
            //instead of saving the settings file after each value in the slider changes, save it only once after delay
            Mod.log.Info("SaveSettingsOnceAfterDelay! Triggered");
            yield return new WaitForSeconds(2f);
            AssetDatabase.global.SaveSettings();
            isOver = false;
            yield break;
        }

    }
}
