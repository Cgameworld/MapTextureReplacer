using Game;
using MapTextureReplacer.Helpers;
using System.IO;
using UnityEngine;

namespace MapTextureReplacer.Systems
{
    public class MapTextureReplacerSystem : GameSystemBase
    {

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
    }
}
