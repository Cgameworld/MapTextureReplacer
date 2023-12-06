using Colossal.UI.Binding;
using Game;
using Game.Common;
using Game.Prefabs;
using Game.Routes;
using Game.Tools;
using Game.Vehicles;
using MapTextureReplacer.Helpers;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

// This system is responsible for querying constantly for data about how many entities
// exists with a specific set of components, and for adding the component `Deleted` to
// all of them if RemoveVehicles is called.

namespace MapTextureReplacer.Systems
{
    public class VehicleCounterSystem : GameSystemBase
    {

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {

        }

        public void OpenImage()
        {
            var file = OpenFileDialog.ShowDialog("Image files\0*.jpg;*.png\0");
            Texture2D newTexture = null;
            byte[] fileData;

            if (!string.IsNullOrEmpty(file))
            {
                fileData = File.ReadAllBytes(file);
                newTexture = new Texture2D(4096, 4096);
                newTexture.LoadImage(fileData);
            }

            Shader.SetGlobalTexture(Shader.PropertyToID("colossal_TerrainGrassDiffuse"), newTexture);
        }
    }
}
