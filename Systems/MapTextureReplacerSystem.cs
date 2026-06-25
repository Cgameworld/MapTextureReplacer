using Colossal.IO.AssetDatabase;
using Colossal.PSI.Environment;
using Game;
using Game.Prefabs;
using Game.Prefabs.Terrain;
using Game.Rendering;
using Game.Simulation;
using MapTextureReplacer.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace MapTextureReplacer.Systems
{
    public partial class MapTextureReplacerSystem : GameSystemBase
    {
        private PrefabSystem m_prefabSystem;
        private CameraUpdateSystem m_cameraUpdateSystem;
        private TerrainSystem m_terrainSystem;
        private TerrainMaterialSystem m_terrainMaterialSystem;

        public float CurrentCameraHeightAboveGround { get; private set; }

        public string PackImportedText = "";

        public Dictionary<string, string> importedPacks = new Dictionary<string, string>();
        public Dictionary<string, string> packSources = new Dictionary<string, string>();
        //slot indices (into SlotOrder) each pack actually supplies a texture for; filters the per-slot dropdowns
        public Dictionary<string, List<int>> packValidSlots = new Dictionary<string, List<int>>();
        public string importedPacksJsonString;

        //serialized float fields of the active terrain render settings prefab, sent to the slider UI
        public string textureFloatsJsonString = "[]";
        //pristine per-field float defaults per prefab, captured once before the mod mutates them
        private readonly Dictionary<string, Dictionary<string, float>> m_defaultsByPrefab = new Dictionary<string, Dictionary<string, float>>();
        private Dictionary<string, float> m_defaultFloats;

        public string textureSelectDataJsonString;
        public List<KeyValuePair<string, string>> textureSelectData;

        //runtime shader-global texture overrides; key = shader property, value = (texture, isLegacyFormat)
        //re-asserted after every game ApplyRenderSettings via the Harmony postfix so they stay durable
        private readonly Dictionary<string, (Texture tex, bool legacy)> m_overrides = new Dictionary<string, (Texture, bool)>();

        private bool m_tilingSaveScheduled;

        //ordered slot list; index is the stable id used by the UI and by textureSelectData
        public static readonly string[] SlotOrder = new string[]
        {
            "colossal_TerrainGrassDiffuse",
            "colossal_TerrainGrassNormal",
            "colossal_TerrainDirtDiffuse",
            "colossal_TerrainDirtNormal",
            "colossal_TerrainRockDiffuse",
            "colossal_TerrainRockNormal",
            "colossal_TerrainExtra1Diffuse",
            "colossal_TerrainExtra1Normal",
            "colossal_TerrainExtra2Diffuse",
            "colossal_TerrainExtra2Normal",
            "colossal_TerrainExtra3Diffuse",
            "colossal_TerrainExtra3Normal",
            "colossal_TerrainExtra4Diffuse",
            "colossal_TerrainExtra4Normal",
        };

        //shader property -> expected pack filename
        public readonly Dictionary<string, string> textureTypes = new Dictionary<string, string>()
        {
            {"colossal_TerrainGrassDiffuse", "Grass_BaseColor.png"},
            {"colossal_TerrainGrassNormal", "Grass_Normal.png"},
            {"colossal_TerrainDirtDiffuse", "Dirt_BaseColor.png"},
            {"colossal_TerrainDirtNormal", "Dirt_Normal.png"},
            {"colossal_TerrainRockDiffuse", "Cliff_BaseColor.png"},
            {"colossal_TerrainRockNormal", "Cliff_Normal.png"},
            {"colossal_TerrainExtra1Diffuse", "Extra1_BaseColor.png"},
            {"colossal_TerrainExtra1Normal", "Extra1_Normal.png"},
            {"colossal_TerrainExtra2Diffuse", "Extra2_BaseColor.png"},
            {"colossal_TerrainExtra2Normal", "Extra2_Normal.png"},
            {"colossal_TerrainExtra3Diffuse", "Extra3_BaseColor.png"},
            {"colossal_TerrainExtra3Normal", "Extra3_Normal.png"},
            {"colossal_TerrainExtra4Diffuse", "Extra4_BaseColor.png"},
            {"colossal_TerrainExtra4Normal", "Extra4_Normal.png"},
        };

        protected override void OnCreate()
        {
            base.OnCreate();

            m_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
            m_terrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_terrainMaterialSystem = World.GetOrCreateSystemManaged<TerrainMaterialSystem>();

            MigrateSavedPaths();
            MigrateLegacyTiling();

            if (Mod.Options.TextureSelectData == null)
            {
                Mod.Options.ActiveDropdown = "none";
                textureSelectData = DefaultSelectData();
                SetTextureSelectDataJson();
            }
            else
            {
                textureSelectData = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(Mod.Options.TextureSelectData);
                PadSelectData();
            }

            Dictionary<string, string> texturePackFolderSources = new Dictionary<string, string>();

            string[] modsFolders =
            {
                Path.Combine(EnvPath.kCacheDataPath, "Mods", "pdx_mods"),
                Path.Combine(EnvPath.kUserDataPath, "Mods"),
            };

            foreach (string modsFolder in modsFolders)
            {
                if (!Directory.Exists(modsFolder))
                {
                    Mod.log.Info("Skipping missing mods folder: " + modsFolder);
                    continue;
                }
                Mod.log.Info("Scanning mods folder: " + modsFolder);
                string source = modsFolder == modsFolders[0] ? "pdx" : "local";
                foreach (string filePath in Directory.GetFiles(modsFolder, "maptextureconfig.json", SearchOption.AllDirectories))
                {
                    texturePackFolderSources[Directory.GetParent(filePath).FullName] = source;
                }
            }

            foreach (var entry in texturePackFolderSources)
            {
                foreach (string filePath in Directory.GetFiles(entry.Key))
                {
                    if (Path.GetFileName(filePath) == "maptextureconfig.json")
                    {
                        try
                        {
                            MapTextureConfig mapTheme = JsonConvert.DeserializeObject<MapTextureConfig>(File.ReadAllText(filePath));
                            importedPacks.Add(filePath, mapTheme.pack_name);
                            packSources[filePath] = entry.Value;
                            packValidSlots[filePath] = ValidSlotsForFolder(Directory.GetParent(filePath).FullName);
                        }
                        catch (Exception ex)
                        {
                            string packId = TryReadPackName(filePath) ?? Directory.GetParent(filePath).Name;
                            Mod.errorLog.Error($"Malformed pack config '{packId}', check the JSON file: {ex.Message}");
                        }
                    }
                }
            }
            SerializeImportedPacksWithSource();

            textureSelectDataJsonString = JsonConvert.SerializeObject(textureSelectData);
        }

        private static List<KeyValuePair<string, string>> DefaultSelectData()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(SlotOrder.Length);
            for (int i = 0; i < SlotOrder.Length; i++)
            {
                list.Add(new KeyValuePair<string, string>("Default", "none"));
            }
            return list;
        }

        //old saves stored 6 entries (grass/dirt/rock); pad to the current slot count so indices line up
        private void PadSelectData()
        {
            if (textureSelectData == null)
            {
                textureSelectData = DefaultSelectData();
                return;
            }
            while (textureSelectData.Count < SlotOrder.Length)
            {
                textureSelectData.Add(new KeyValuePair<string, string>("Default", "none"));
            }
        }

        public void SerializeImportedPacksWithSource()
        {
            var payload = new Dictionary<string, object>(importedPacks.Count);
            foreach (var entry in importedPacks.OrderBy(e =>
                packSources.TryGetValue(e.Key, out var s) && s == "local" ? 1 : 0))
            {
                var src = packSources.TryGetValue(entry.Key, out var s) ? s : "pdx";
                if ((src == "local" && !Mod.Options.ShowLocalPacks)
                    || (src != "local" && !Mod.Options.ShowDownloadedPacks)) continue;
                payload[entry.Key] = new
                {
                    name = entry.Value,
                    source = src,
                    slots = packValidSlots.TryGetValue(entry.Key, out List<int> vs) ? vs : null,
                };
            }
            importedPacksJsonString = JsonConvert.SerializeObject(payload);
        }

        protected override void OnUpdate()
        {
            if (m_cameraUpdateSystem?.activeCameraController != null
                && m_terrainSystem.GetHeightData().heights.IsCreated && Mod.Options != null && Mod.Options.ShowCameraHeight)
            {
                TerrainHeightData heightData = m_terrainSystem.GetHeightData();
                IGameCameraController controller = m_cameraUpdateSystem.activeCameraController;
                float3 pivot = controller.pivot;
                float groundY = TerrainUtils.SampleHeight(ref heightData, pivot);
                float heightAboveGround = (pivot.y - groundY) + controller.zoom;
                CurrentCameraHeightAboveGround = heightAboveGround;
            }
        }

        // ----- slot helpers -----

        public static string ShaderPropertyAt(int index) =>
            (index >= 0 && index < SlotOrder.Length) ? SlotOrder[index] : null;

        public static int IndexOfSlot(string shaderProperty) => Array.IndexOf(SlotOrder, shaderProperty);

        //"colossal_TerrainGrassDiffuse" -> "GrassDiffuse"
        private static string SlotCore(string shaderProperty) => shaderProperty.Replace("colossal_Terrain", "");

        //"colossal_TerrainGrassNormal" -> "Grass", "colossal_TerrainExtra1Diffuse" -> "Extra1"
        private static string MaterialGroup(string shaderProperty)
        {
            string core = SlotCore(shaderProperty);
            if (core.EndsWith("Diffuse")) return core.Substring(0, core.Length - "Diffuse".Length);
            if (core.EndsWith("Normal")) return core.Substring(0, core.Length - "Normal".Length);
            return core;
        }

        private static bool IsLegacyGroup(string group) => group == "Grass" || group == "Dirt" || group == "Rock";

        // ----- texture override application (durable, re-asserted by the ApplyRenderSettings postfix) -----

        //re-applies every mod override after the game rewrites the shader globals; never calls ApplyRenderSettings
        public void ApplyAllOverrides()
        {
            if (m_overrides.Count == 0) return;

            foreach (var kv in m_overrides)
            {
                Shader.SetGlobalTexture(Shader.PropertyToID(kv.Key), kv.Value.tex);
            }
            RefreshGroupLegacy("Grass");
            RefreshGroupLegacy("Dirt");
            RefreshGroupLegacy("Rock");
        }

        //per-material legacy flag: 1 if any overridden slot in the group is legacy, 0 if overridden non-legacy,
        //untouched (game's value stands) if no slot in the group is overridden
        private void RefreshGroupLegacy(string group)
        {
            if (!IsLegacyGroup(group)) return;

            bool any = false;
            bool legacy = false;
            foreach (var kv in m_overrides)
            {
                if (MaterialGroup(kv.Key) == group)
                {
                    any = true;
                    legacy |= kv.Value.legacy;
                }
            }
            if (any)
            {
                Shader.SetGlobalFloat(Shader.PropertyToID($"_TerrainLegacy{group}Texture"), legacy ? 1f : 0f);
            }
        }

        private void SetOverrideTexture(string shaderProperty, Texture tex, bool legacy)
        {
            m_overrides[shaderProperty] = (tex, legacy);
            Shader.SetGlobalTexture(Shader.PropertyToID(shaderProperty), tex);
            RefreshGroupLegacy(MaterialGroup(shaderProperty));
        }

        public void ClearAllOverrides() => m_overrides.Clear();

        //decode + validate; on failure leave the slot vanilla (graceful fallback) and log an actionable error
        private bool TryDecodeTexture(byte[] data, string sourceName, out Texture2D tex)
        {
            tex = null;
            if (data == null || !(IsPng(data) || IsJpg(data)))
            {
                Mod.errorLog.Error($"'{sourceName}' isn't a valid PNG/JPG ({GuessFormat(data)}), using the default for this slot");
                return false;
            }
            Texture2D t = new Texture2D(4096, 4096);
            if (!t.LoadImage(data))
            {
                UnityEngine.Object.Destroy(t);
                Mod.errorLog.Error($"'{sourceName}' could not be decoded as PNG/JPG, using the default for this slot");
                return false;
            }
            tex = t;
            return true;
        }

        private static bool IsPng(byte[] d) => d.Length >= 8 && d[0] == 0x89 && d[1] == 0x50 && d[2] == 0x4E && d[3] == 0x47;
        private static bool IsJpg(byte[] d) => d.Length >= 3 && d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF;
        private static bool IsDds(byte[] d) => d.Length >= 4 && d[0] == 0x44 && d[1] == 0x44 && d[2] == 0x53 && d[3] == 0x20;
        private static string GuessFormat(byte[] d) => IsDds(d) ? "looks like a DDS renamed to .png" : "unsupported format";

        private void LoadTextureFromBytes(string shaderProperty, byte[] data, string sourceName)
        {
            if (TryDecodeTexture(data, sourceName, out Texture2D tex))
            {
                SetOverrideTexture(shaderProperty, tex, true);
            }
        }

        //generic: derive the prefab field from the shader property (colossal_TerrainExtra1Normal -> m_Extra1Normal)
        private Texture LoadPrefabSlotTexture(string shaderProperty, TerrainRenderSettingsPrefab prefab)
        {
            FieldInfo field = typeof(TerrainRenderSettingsPrefab).GetField("m_" + SlotCore(shaderProperty),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return null;

            AssetReference<TextureAsset> assetRef = (AssetReference<TextureAsset>)field.GetValue(prefab);
            TextureAsset asset = assetRef;
            return asset?.Load();
        }

        //slot indices a folder pack supplies a texture file for
        private List<int> ValidSlotsForFolder(string directory)
        {
            List<int> slots = new List<int>();
            for (int i = 0; i < SlotOrder.Length; i++)
            {
                if (File.Exists(Path.Combine(directory, textureTypes[SlotOrder[i]]))) slots.Add(i);
            }
            return slots;
        }

        //slot indices a prefab pack has a texture reference assigned for
        public List<int> ValidSlotsForPrefab(TerrainRenderSettingsPrefab prefab)
        {
            List<int> slots = new List<int>();
            for (int i = 0; i < SlotOrder.Length; i++)
            {
                if (PrefabHasSlot(prefab, SlotOrder[i])) slots.Add(i);
            }
            return slots;
        }

        private static bool PrefabHasSlot(TerrainRenderSettingsPrefab prefab, string shaderProperty)
        {
            FieldInfo field = typeof(TerrainRenderSettingsPrefab).GetField("m_" + SlotCore(shaderProperty),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return false;
            try
            {
                AssetReference<TextureAsset> assetRef = (AssetReference<TextureAsset>)field.GetValue(prefab);
                TextureAsset asset = assetRef;
                return asset != null;
            }
            catch { return false; }
        }

        // ----- pack selection (base-pack dropdown) -----

        public void ChangePack(string current)
        {
            //start every pack selection from the map's pristine tiling so float values don't carry over between packs
            RestoreTilingDefaults();

            if (current == "none")
            {
                ClearAllOverrides();
                if (HasActiveTerrain()) m_terrainMaterialSystem.ApplyRenderSettings();
            }
            else if (current.EndsWith(".zip"))
            {
                string label = current.Split(',')[0];
                string zipPath = current.Split(',')[1];
                LoadAllFromZip(zipPath, label, current);
            }
            else if (current.EndsWith(".json"))
            {
                try
                {
                    MapTextureConfig config = JsonConvert.DeserializeObject<MapTextureConfig>(File.ReadAllText(current));
                    LoadAllFromFolder(Path.GetDirectoryName(current), config.pack_name, current);
                    ApplyJsonPackTiling(config);
                }
                catch (Exception ex)
                {
                    string packId = TryReadPackName(current) ?? Directory.GetParent(current).Name;
                    Mod.errorLog.Error($"Malformed pack config '{packId}', check the JSON file. {ex.Message}");
                }
            }
            else if (m_prefabSystem.TryGetPrefab(PrefabIDParse(current), out PrefabBase newPrefab) && newPrefab is TerrainRenderSettingsPrefab terrainSettings)
            {
                LoadAllFromPrefab(terrainSettings, PrefabIDParse(current).GetName(), current);
            }

            PrepareTextureFloatSliders();
            PersistTiling();
        }

        private void LoadAllFromZip(string zipPath, string label, string packPath)
        {
            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
            {
                Mod.errorLog.Error($"Zip file not found: '{zipPath}'");
                return;
            }
            int loaded = 0;
            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                foreach (string shaderProperty in SlotOrder)
                {
                    ZipArchiveEntry entry = archive.GetEntry(textureTypes[shaderProperty]);
                    if (entry == null) continue;
                    byte[] data = ReadZipEntry(entry);
                    if (TryDecodeTexture(data, entry.Name, out Texture2D tex))
                    {
                        SetOverrideTexture(shaderProperty, tex, true);
                        SetSlotLabel(IndexOfSlot(shaderProperty), label, packPath);
                        loaded++;
                    }
                }
            }
            if (loaded == 0) Mod.errorLog.Error($"No terrain textures found in '{label}' (expected Grass_BaseColor.png etc.)");
        }

        private void LoadAllFromFolder(string directory, string label, string packPath)
        {
            int loaded = 0;
            foreach (string shaderProperty in SlotOrder)
            {
                string filePath = Path.Combine(directory, textureTypes[shaderProperty]);
                if (!File.Exists(filePath)) continue;
                if (TryDecodeTexture(File.ReadAllBytes(filePath), Path.GetFileName(filePath), out Texture2D tex))
                {
                    SetOverrideTexture(shaderProperty, tex, true);
                    SetSlotLabel(IndexOfSlot(shaderProperty), label, packPath);
                    loaded++;
                }
            }
            if (loaded == 0) Mod.errorLog.Error($"No terrain textures found in '{label}'");
        }

        private void LoadAllFromPrefab(TerrainRenderSettingsPrefab prefab, string label, string packPath)
        {
            bool legacy = prefab.isLegacyFormat;
            foreach (string shaderProperty in SlotOrder)
            {
                Texture tex = LoadPrefabSlotTexture(shaderProperty, prefab);
                if (tex == null) continue;
                SetOverrideTexture(shaderProperty, tex, legacy);
                SetSlotLabel(IndexOfSlot(shaderProperty), label, packPath);
            }
        }

        // ----- per-slot selection (texture dropdown) -----

        public void OpenImage(int index, string path)
        {
            string shaderProperty = ShaderPropertyAt(index);
            if (shaderProperty == null) return;
            string filenameTexture = textureTypes[shaderProperty];

            if (path == "")
            {
                string file = OpenFileDialog.ShowDialog("Image files\0*.jpg;*.png\0");
                if (!string.IsNullOrEmpty(file) && TryDecodeTexture(File.ReadAllBytes(file), Path.GetFileName(file), out Texture2D tex))
                {
                    SetOverrideTexture(shaderProperty, tex, true);
                    SetSlotLabel(index, ShortenDisplayedFilename(file), file);
                }
            }
            else if (path.EndsWith(".zip"))
            {
                using (ZipArchive archive = ZipFile.Open(path.Split(',')[1], ZipArchiveMode.Read))
                {
                    ZipArchiveEntry entry = archive.GetEntry(filenameTexture);
                    if (entry != null && TryDecodeTexture(ReadZipEntry(entry), entry.Name, out Texture2D tex))
                    {
                        SetOverrideTexture(shaderProperty, tex, true);
                        SetSlotLabel(index, path.Split(',')[0], path);
                    }
                }
            }
            else if (m_prefabSystem.TryGetPrefab(PrefabIDParse(path), out PrefabBase newPrefab) && newPrefab is TerrainRenderSettingsPrefab terrainSettings)
            {
                Texture tex = LoadPrefabSlotTexture(shaderProperty, terrainSettings);
                if (tex != null)
                {
                    SetOverrideTexture(shaderProperty, tex, terrainSettings.isLegacyFormat);
                    SetSlotLabel(index, PrefabIDParse(path).GetName(), path);
                }
            }
            else if (path.EndsWith(".json"))
            {
                string filePath = Path.Combine(Path.GetDirectoryName(path), filenameTexture);
                string label = importedPacks.TryGetValue(path, out string v) ? v : ShortenDisplayedFilename(Path.GetFileName(path));
                if (File.Exists(filePath) && TryDecodeTexture(File.ReadAllBytes(filePath), filenameTexture, out Texture2D tex))
                {
                    SetOverrideTexture(shaderProperty, tex, true);
                    SetSlotLabel(index, label, path);
                }
            }
            else if (File.Exists(path))
            {
                if (TryDecodeTexture(File.ReadAllBytes(path), Path.GetFileName(path), out Texture2D tex))
                {
                    SetOverrideTexture(shaderProperty, tex, true);
                    SetSlotLabel(index, ShortenDisplayedFilename(Path.GetFileName(path)), path);
                }
            }
        }

        public void ResetTexture(int index)
        {
            string shaderProperty = ShaderPropertyAt(index);
            if (shaderProperty == null) return;

            m_overrides.Remove(shaderProperty);
            textureSelectData[index] = new KeyValuePair<string, string>("Default", "none");
            SetTextureSelectDataJson();

            //let the game restore this slot's vanilla texture; the postfix re-applies any remaining overrides
            if (HasActiveTerrain()) m_terrainMaterialSystem.ApplyRenderSettings();
        }

        public void ResetTextureSelectData()
        {
            textureSelectData = DefaultSelectData();
            SetTextureSelectDataJson();
        }

        private void SetSlotLabel(int index, string key, string path)
        {
            if (index < 0 || index >= textureSelectData.Count) return;
            textureSelectData[index] = new KeyValuePair<string, string>(key, path);
            SetTextureSelectDataJson();
        }

        private void SetTextureSelectDataJson()
        {
            textureSelectDataJsonString = JsonConvert.SerializeObject(textureSelectData);
            Mod.Options.TextureSelectData = textureSelectDataJsonString;
            AssetDatabase.global.SaveSettings();
        }

        private static byte[] ReadZipEntry(ZipArchiveEntry entry)
        {
            using (Stream s = entry.Open())
            using (MemoryStream ms = new MemoryStream())
            {
                s.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static string ShortenDisplayedFilename(string file)
        {
            string fileName = Path.GetFileName(file);
            return fileName.Length > 15 ? fileName.Substring(0, 15) : fileName;
        }

        public void GetTextureZip()
        {
            string zipFilePath = OpenFileDialog.ShowDialog("Zip archives\0*.zip\0");
            PackImportedText = Path.GetFileNameWithoutExtension(zipFilePath) + "," + zipFilePath;
        }

        public static PrefabID PrefabIDParse(string s)
        {
            try
            {
                int i = s.IndexOf(':');
                int j = s.LastIndexOf(" (", StringComparison.Ordinal);
                return new PrefabID(s[..i], s[(i + 1)..j], Colossal.Hash128.Parse(s[(j + 2)..^1]));
            }
            catch
            {
                return new PrefabID(null, "empty");
            }
        }

        // ----- active prefab / settings persistence -----

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

        public string GetActivePackDropdown() => Mod.Options.ActiveDropdown;

        private bool HasActiveTerrain() =>
            m_terrainMaterialSystem != null && m_terrainMaterialSystem.currentTerrainRenderSettings != Entity.Null;

        private TerrainRenderSettingsPrefab ActivePrefab() =>
            HasActiveTerrain() ? m_prefabSystem.GetPrefab<TerrainRenderSettingsPrefab>(m_terrainMaterialSystem.currentTerrainRenderSettings) : null;

        // ----- tiling: read / apply / persist / reset -----

        //ordered material tokens; "Extra" must precede the others so painted fields like m_Extra1DirtOverride
        //land in the painted group instead of Dirt. A field matching no token is a global "common" setting.
        private static readonly (string token, string group)[] FieldGroupRules = new (string, string)[]
        {
            ("Extra", "extra"),
            ("Grass", "grass"),
            ("Dirt", "dirt"),
            ("Rock", "rock"),
        };

        //single authority for which UI tab a float field belongs to (display + reset both read this)
        public static string GroupForField(string fieldName)
        {
            foreach ((string token, string group) in FieldGroupRules)
            {
                if (fieldName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) return group;
            }
            return "common";
        }

        //parent Extra index (1-4) for an extra override field, else 0 — lets the UI nest each extra's sliders under its textures
        private static int ExtraIndexOf(string fieldName)
        {
            Match m = Regex.Match(fieldName, "Extra([1-4])");
            return m.Success ? int.Parse(m.Groups[1].Value) : 0;
        }

        //display order within a tab. grass/dirt/rock: tiling, then depth scale, then the rest (lod blend).
        //common: triplanar + splat, then the rest (blur depth). extra/painted keeps declaration order.
        private static int FieldSortRank(string fieldName)
        {
            string group = GroupForField(fieldName);
            if (group == "grass" || group == "dirt" || group == "rock")
            {
                if (fieldName.Contains("Tiling")) return 0;
                if (fieldName.Contains("DepthScale")) return 1;
                return 2;
            }
            if (group == "common")
            {
                if (fieldName.Contains("Triplanar") || fieldName.Contains("Splat")) return 0;
                return 1;
            }
            return 0;
        }

        public void PrepareTextureFloatSliders()
        {
            var entries = new List<object>();
            foreach (KeyValuePair<string, float> entry in ReadFloatFields().OrderBy(e => FieldSortRank(e.Key)))
            {
                float min, max;
                var matchingRange = SliderRangeOverrides.Ranges.FirstOrDefault(r => entry.Key.Contains(r.Key));
                if (!string.IsNullOrEmpty(matchingRange.Key))
                {
                    min = matchingRange.Value.min;
                    max = matchingRange.Value.max;
                }
                else
                {
                    RangeAttribute rangeAttr = GetLowestField(entry.Key).GetCustomAttribute<RangeAttribute>();
                    min = rangeAttr?.min ?? 0f;
                    max = rangeAttr?.max ?? 100f;
                }

                entries.Add(new
                {
                    name = entry.Key,
                    label = PrettyFieldLabel(entry.Key),
                    value = entry.Value,
                    min,
                    max,
                    group = GroupForField(entry.Key),
                    extra = ExtraIndexOf(entry.Key),
                });
            }
            textureFloatsJsonString = JsonConvert.SerializeObject(entries);
        }

        private Dictionary<string, float> ReadFloatFields()
        {
            Dictionary<string, float> textureSettingFloats = new Dictionary<string, float>();
            TerrainRenderSettingsPrefab prefab = ActivePrefab();
            if (prefab == null) return textureSettingFloats;

            foreach (FieldInfo field in prefab.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object value = field.GetValue(prefab);

                //skip the private m_Legacy*Tiling fields; they only feed Initialize's legacy->Near/Mid/Far migration, editing them at runtime does nothing
                if (value is float floatValue && !field.Name.Contains("Legacy"))
                {
                    textureSettingFloats[field.Name] = floatValue;
                }
                if (value is TerrainTilingData terrainTilingData)
                {
                    textureSettingFloats[$"{field.Name}.m_FarTiling"] = terrainTilingData.m_FarTiling;
                    textureSettingFloats[$"{field.Name}.m_MidTiling"] = terrainTilingData.m_MidTiling;
                    textureSettingFloats[$"{field.Name}.m_NearTiling"] = terrainTilingData.m_NearTiling;
                }
                if (value is TerrainLodBlendData terrainLodBlendData)
                {
                    textureSettingFloats[$"{field.Name}.m_MidBlendStart"] = terrainLodBlendData.m_MidBlendStart;
                    textureSettingFloats[$"{field.Name}.m_MidBlendEnd"] = terrainLodBlendData.m_MidBlendEnd;
                    textureSettingFloats[$"{field.Name}.m_FarBlendStart"] = terrainLodBlendData.m_FarBlendStart;
                    textureSettingFloats[$"{field.Name}.m_FarBlendEnd"] = terrainLodBlendData.m_FarBlendEnd;
                }
                if (value is TerrainDepthScaleSetting terrainDepthScaleSetting)
                {
                    foreach (FieldInfo depthScalingValues in typeof(TerrainDepthScaleSetting).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (depthScalingValues.GetValue(terrainDepthScaleSetting) is Vector2 scaleVector)
                        {
                            textureSettingFloats[$"{field.Name}.{depthScalingValues.Name}.x"] = scaleVector.x;
                            textureSettingFloats[$"{field.Name}.{depthScalingValues.Name}.y"] = scaleVector.y;
                        }
                    }
                }
                if (value is TerrainBlurDepthData terrainBlurDepthData)
                {
                    textureSettingFloats[$"{field.Name}.m_NearBlurDepth"] = terrainBlurDepthData.m_NearBlurDepth;
                    textureSettingFloats[$"{field.Name}.m_FarBlurDepth"] = terrainBlurDepthData.m_FarBlurDepth;
                    textureSettingFloats[$"{field.Name}.m_FarDistance"] = terrainBlurDepthData.m_FarDistance;
                    textureSettingFloats[$"{field.Name}.m_NearDistance"] = terrainBlurDepthData.m_NearDistance;
                }
            }
            return textureSettingFloats;
        }

        public FieldInfo GetLowestField(string path)
        {
            object obj = ActivePrefab();
            FieldInfo field = null;
            foreach (string name in path.Split('.'))
            {
                field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? throw new Exception($"Field not found: {name}");
                obj = field.GetValue(obj);
            }
            return field;
        }

        public void ChangeFloatField(string path, float amount)
        {
            if (!HasActiveTerrain()) return;
            SetFieldValue(path, amount);
            RefreshShaderForField(path);
            PrepareTextureFloatSliders();
            ScheduleTilingSave();
        }

        //walk the reflection path and assign, copying nested value-type (struct) fields back into their parent
        private void SetFieldValue(string path, float amount)
        {
            object obj = ActivePrefab();
            if (obj == null) return;
            var chain = new List<(object parent, FieldInfo field)>();
            foreach (string name in path.Split('.'))
            {
                FieldInfo field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? throw new Exception($"Field not found: {name}");
                chain.Add((obj, field));
                obj = field.GetValue(obj);
            }

            object boxed = amount;
            for (int i = chain.Count - 1; i >= 0; i--)
            {
                chain[i].field.SetValue(chain[i].parent, boxed);
                boxed = chain[i].parent;
            }
        }

        private void RefreshShaderForField(string path)
        {
            TerrainRenderSettingsPrefab p = ActivePrefab();
            if (p == null) return;
            string lower = path.ToLowerInvariant();

            if (lower.Contains("grasstiling") || lower.Contains("grasslodblend")) PushGrassTiling(p);
            else if (lower.Contains("dirttiling") || lower.Contains("dirtlodblend")) PushDirtTiling(p);
            else if (lower.Contains("rocktiling") || lower.Contains("rocklodblend")) PushRockTiling(p);
            else if (lower.Contains("extra")) RefreshExtraShaderVectors(p);
            else if (lower.Contains("triplanar") || lower.Contains("splatmultiplier") || lower.Contains("splatpower")) PushCommonShaderFloats(p);
            //depth-scale / blur-depth: applied per-frame by the game's UpdateMaterial(); no push needed
        }

        //triplanar + splat globals the game sets only inside ApplyRenderSettings (not per-frame), so push them on edit/reload
        private static void PushCommonShaderFloats(TerrainRenderSettingsPrefab p)
        {
            Shader.SetGlobalFloat(Shader.PropertyToID("_TriplanarHeightmapBlending"), p.m_TriplanarHeightMapBlending);
            Shader.SetGlobalFloat(Shader.PropertyToID("_TerrainTriplanarBlendStrengthY"), p.m_TriplanarBlendStrengthY);
            Shader.SetGlobalFloat(Shader.PropertyToID("_SplatMultiplier"), p.m_SplatMultiplier);
            Shader.SetGlobalFloat(Shader.PropertyToID("_SplatPower"), p.m_SplatPower);
        }

        private static void PushGrassTiling(TerrainRenderSettingsPrefab p)
        {
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainGrassTextureTiling"), p.m_TerrainGrassTiling.ShaderData);
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainGrassTextureTilingBlend"), p.m_TerrainGrassLodBlend.ShaderData);
        }

        private static void PushDirtTiling(TerrainRenderSettingsPrefab p)
        {
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainDirtTextureTiling"), p.m_TerrainDirtTiling.ShaderData);
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainDirtTextureTilingBlend"), p.m_TerrainDirtLodBlend.ShaderData);
        }

        private static void PushRockTiling(TerrainRenderSettingsPrefab p)
        {
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainRockTextureTiling"), p.m_TerrainRockTiling.ShaderData);
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainRockTextureTilingBlend"), p.m_TerrainRockLodBlend.ShaderData);
        }

        //mirrors the extra block of the game's ApplyRenderSettings without reloading textures
        private static void RefreshExtraShaderVectors(TerrainRenderSettingsPrefab p)
        {
            Vector4 dirt = default, blur = default, splat = default, height = default;
            dirt.x = p.m_Extra1DirtOverride; blur.x = p.m_Extra1BlurDepth; splat.x = Mathf.Max(p.m_Extra1SplatRange, p.m_Extra1BlurDepth); height.x = p.m_Extra1HeightOffset;
            dirt.y = p.m_Extra2DirtOverride; blur.y = p.m_Extra2BlurDepth; splat.y = Mathf.Max(p.m_Extra2SplatRange, p.m_Extra2BlurDepth); height.y = p.m_Extra2HeightOffset;
            dirt.z = p.m_Extra3DirtOverride; blur.z = p.m_Extra3BlurDepth; splat.z = Mathf.Max(p.m_Extra3SplatRange, p.m_Extra3BlurDepth); height.z = p.m_Extra3HeightOffset;
            dirt.w = p.m_Extra4DirtOverride; blur.w = p.m_Extra4BlurDepth; splat.w = Mathf.Max(p.m_Extra4SplatRange, p.m_Extra4BlurDepth); height.w = p.m_Extra4HeightOffset;
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainExtraDirtOverride"), dirt);
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainExtraBlurDepth"), blur);
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainExtraSplatRange"), splat);
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainExtraHeightOffset"), height);
            Shader.SetGlobalVector(Shader.PropertyToID("_TerrainExtraTextureTiling"), p.m_TerrainGrassTiling.ShaderData);
        }

        private void PushAllTilingShaders()
        {
            TerrainRenderSettingsPrefab p = ActivePrefab();
            if (p == null) return;
            PushGrassTiling(p);
            PushDirtTiling(p);
            PushRockTiling(p);
            RefreshExtraShaderVectors(p);
            PushCommonShaderFloats(p);
        }

        //capture pristine defaults once per prefab (before the mod mutates it), used by the reset buttons
        public void CaptureFloatDefaults()
        {
            TerrainRenderSettingsPrefab prefab = ActivePrefab();
            if (prefab == null) return;
            if (!m_defaultsByPrefab.TryGetValue(prefab.name, out Dictionary<string, float> def))
            {
                def = ReadFloatFields();
                m_defaultsByPrefab[prefab.name] = def;
            }
            m_defaultFloats = def;
        }

        public void ApplySavedTiling()
        {
            if (string.IsNullOrEmpty(Mod.Options.TilingFloatData)) return;
            Dictionary<string, float> saved = JsonConvert.DeserializeObject<Dictionary<string, float>>(Mod.Options.TilingFloatData);
            if (saved == null) return;
            foreach (var kv in saved) SetFieldValue(kv.Key, kv.Value);
            PushAllTilingShaders();
            PrepareTextureFloatSliders();
        }

        private void ScheduleTilingSave()
        {
            if (m_tilingSaveScheduled) return;
            m_tilingSaveScheduled = true;
            StaticCoroutine.Start(SaveTilingAfterDelay());
        }

        private IEnumerator SaveTilingAfterDelay()
        {
            yield return new WaitForSeconds(1.5f);
            PersistTiling();
            m_tilingSaveScheduled = false;
        }

        private void PersistTiling()
        {
            if (m_defaultFloats == null) return;
            Dictionary<string, float> diff = new Dictionary<string, float>();
            foreach (var kv in ReadFloatFields())
            {
                if (!m_defaultFloats.TryGetValue(kv.Key, out float d) || !Mathf.Approximately(d, kv.Value))
                {
                    diff[kv.Key] = kv.Value;
                }
            }
            Mod.Options.TilingFloatData = diff.Count > 0 ? JsonConvert.SerializeObject(diff) : "";
            AssetDatabase.global.SaveSettings();
        }

        //per-tab reset: the UI passes the active tab's group id (grass/dirt/rock/extra/common)
        public void ResetTextureFloats(string group)
        {
            Dictionary<string, float> baseline = ResetBaseline();
            if (baseline != null && !string.IsNullOrEmpty(group))
            {
                foreach (var kv in baseline)
                {
                    if (GroupForField(kv.Key) == group) ChangeFloatField(kv.Key, kv.Value);
                }
            }
            PrepareTextureFloatSliders();
        }

        //reset target: the map's pristine defaults, overlaid with the active legacy pack's inferred tiling
        private Dictionary<string, float> ResetBaseline()
        {
            if (m_defaultFloats == null) return null;
            Dictionary<string, float> baseline = new Dictionary<string, float>(m_defaultFloats);
            foreach (var kv in InferredTilingForActivePack()) baseline[kv.Key] = kv.Value;
            return baseline;
        }

        //restore the active prefab's pristine tiling without touching saved persistence
        public void RestoreTilingDefaults()
        {
            if (m_defaultFloats == null) return;
            foreach (var kv in m_defaultFloats) SetFieldValue(kv.Key, kv.Value);
            PushAllTilingShaders();
        }

        private void ApplyJsonPackTiling(MapTextureConfig config)
        {
            foreach (var kv in InferredTilingFromConfig(config)) ChangeFloatField(kv.Key, kv.Value);
        }

        //inferred Near/Mid/Far tiling for a legacy JSON pack (far/close/closeDirt), or empty if the pack defines none
        private static Dictionary<string, float> InferredTilingFromConfig(MapTextureConfig config)
        {
            if (config != null
                && float.TryParse(config.far_tiling, out float far)
                && float.TryParse(config.close_tiling, out float close)
                && float.TryParse(config.close_dirt_tiling, out float closeDirt))
            {
                return BuildLegacyTilingDict(far, close, closeDirt);
            }
            return new Dictionary<string, float>();
        }

        //same inference for whichever base pack is currently active (read live, so it survives a reload)
        private Dictionary<string, float> InferredTilingForActivePack()
        {
            string active = Mod.Options.ActiveDropdown;
            if (string.IsNullOrEmpty(active) || !active.EndsWith(".json") || !File.Exists(active))
                return new Dictionary<string, float>();
            try
            {
                return InferredTilingFromConfig(JsonConvert.DeserializeObject<MapTextureConfig>(File.ReadAllText(active)));
            }
            catch { return new Dictionary<string, float>(); }
        }

        // ----- full reset (Options "Reset All Settings" button) -----

        public void ResetAll()
        {
            ResetTextureSelectData();
            ClearAllOverrides();
            if (HasActiveTerrain())
            {
                RestoreTilingDefaults();
                m_terrainMaterialSystem.ApplyRenderSettings();
            }
        }

        // ----- exit to menu: drop overrides + pristine tiling, restore vanilla (saved data reapplies on next load) -----

        public void OnExitToMenu()
        {
            ClearAllOverrides();
            if (HasActiveTerrain())
            {
                RestoreTilingDefaults();
                m_terrainMaterialSystem.ApplyRenderSettings();
            }
        }

        // ----- migration / helpers -----

        //best-effort migration of the old Vector4 (far, close, closeDirt) into the new Near/Mid/Far model,
        //using the same formula the game applies to legacy prefabs in TerrainRenderSettingsPrefab.Initialize
        private static Dictionary<string, float> BuildLegacyTilingDict(float far, float close, float closeDirt)
        {
            float mid = far * 2f;
            return new Dictionary<string, float>
            {
                {"m_TerrainGrassTiling.m_FarTiling", far},
                {"m_TerrainGrassTiling.m_MidTiling", Mathf.Min(mid, close)},
                {"m_TerrainGrassTiling.m_NearTiling", close},
                {"m_TerrainDirtTiling.m_FarTiling", far},
                {"m_TerrainDirtTiling.m_MidTiling", Mathf.Min(mid, closeDirt)},
                {"m_TerrainDirtTiling.m_NearTiling", closeDirt},
                {"m_TerrainRockTiling.m_FarTiling", far},
                {"m_TerrainRockTiling.m_MidTiling", Mathf.Min(mid, close)},
                {"m_TerrainRockTiling.m_NearTiling", close},
            };
        }

        private static void MigrateLegacyTiling()
        {
            if (!string.IsNullOrEmpty(Mod.Options.TilingFloatData)) return;
            Vector4 v = Mod.Options.CurrentTilingVector;
            if (v == Vector4.zero) return;

            Mod.Options.TilingFloatData = JsonConvert.SerializeObject(BuildLegacyTilingDict(v.x, v.y, v.z));
            Mod.Options.CurrentTilingVector = Vector4.zero;
            AssetDatabase.global.SaveSettings();
        }

        private static string PrettyFieldLabel(string fieldName)
        {
            string[] p = fieldName.Split('.')
                .Select(x => x.StartsWith("m_") ? x.Substring(2) : x)
                .ToArray();

            string name = p[p.Length - 1];
            if (name.Length == 1) name = name.ToUpperInvariant();

            if (p.Length > 2 && p[0].EndsWith("Settings"))
                name = p[p.Length - 2] + " " + p[0].Substring(0, p[0].Length - "Settings".Length) + " " + name;
            else if (p.Length > 1)
                name = Regex.Replace(p[p.Length - 2], "^Terrain|Tiling$|LodBlend$", "") + " " + name;

            return Regex.Replace(name, "(?<=[a-z0-9])(?=[A-Z])", " ").Trim();
        }

        private static string TryReadPackName(string jsonPath)
        {
            try
            {
                Match match = Regex.Match(File.ReadAllText(jsonPath), @"""pack_name""\s*:\s*""([^""]+)""");
                return match.Success ? match.Groups[1].Value : null;
            }
            catch { return null; }
        }

        private static void MigrateSavedPaths()
        {
            Regex pattern = new Regex(@"(Mods[\\/]+)mods_subscribed");
            string replacement = "$1pdx_mods";
            bool changed = false;

            if (!string.IsNullOrEmpty(Mod.Options.ActiveDropdown))
            {
                string updated = pattern.Replace(Mod.Options.ActiveDropdown, replacement);
                if (updated != Mod.Options.ActiveDropdown)
                {
                    Mod.Options.ActiveDropdown = updated;
                    changed = true;
                }
            }
            if (!string.IsNullOrEmpty(Mod.Options.TextureSelectData))
            {
                string updated = pattern.Replace(Mod.Options.TextureSelectData, replacement);
                if (updated != Mod.Options.TextureSelectData)
                {
                    Mod.Options.TextureSelectData = updated;
                    changed = true;
                }
            }
            if (changed) AssetDatabase.global.SaveSettings();
        }
    }
}
