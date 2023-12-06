using Game;
using Game.UI;
using MapTextureReplacer.Helpers;
using System.Collections.Generic;
using System.IO;
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

            Texture2D newTexture = null;
            byte[] fileData;


            if (!string.IsNullOrEmpty(file))
            {
                fileData = File.ReadAllBytes(file);
                newTexture = new Texture2D(4096, 4096);
                newTexture.LoadImage(fileData);
                Shader.SetGlobalTexture(Shader.PropertyToID(shaderProperty), newTexture);
            }

        }

        public void ResetTexture(string shaderProperty)
        {
            mapTextureCache.TryGetValue(shaderProperty, out Texture texture);
            if (texture != null)
            {
                Shader.SetGlobalTexture(Shader.PropertyToID(shaderProperty), texture);
            }
        }
    }
}
