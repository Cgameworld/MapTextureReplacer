using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MapTextureReplacer.Systems
{
    public partial class MapTextureReplacerTextureCacheSystem : GameSystemBase
    {
        public readonly Dictionary<string, string> textureTypes = new Dictionary<string, string>() {
            {"colossal_TerrainGrassDiffuse", "Grass_BaseColor.png"},
            {"colossal_TerrainGrassNormal", "Grass_Normal.png"},
            {"colossal_TerrainDirtDiffuse", "Dirt_BaseColor.png"},
            {"colossal_TerrainDirtNormal", "Dirt_Normal.png"},
            {"colossal_TerrainRockDiffuse", "Cliff_BaseColor.png"},
            {"colossal_TerrainRockNormal", "Cliff_Normal.png"},
        };

        public Dictionary<string, Texture> mapTextureCache = new Dictionary<string, Texture>();
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void StartCache()
        {
            //cache original textures for reset function
            foreach (var item in textureTypes)
            {
                CacheExistingTexture(item.Key);
            }
        }

        private void CacheExistingTexture(string shaderProperty)
        {
            var existingTexture = Shader.GetGlobalTexture(Shader.PropertyToID(shaderProperty));
            if (!mapTextureCache.ContainsKey(shaderProperty))
            {
                mapTextureCache.Add(shaderProperty, existingTexture);
            }
        }

        protected override void OnUpdate() {}

    }
}
