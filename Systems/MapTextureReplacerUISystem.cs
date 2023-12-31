using Colossal.UI.Binding;
using Game;
using Game.Common;
using Game.Debug;
using Game.Prefabs;
using Game.Routes;
using Game.Serialization;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace MapTextureReplacer.Systems
{
    public class MapTextureReplacerUISystem : UISystemBase
    {
        public int current_vehicle_count = 0;
        public MapTextureReplacerSystem systemManaged;
        private Dictionary<string, Action<int>> handlers = new Dictionary<string, Action<int>>();

        protected override void OnCreate()
        {
            this.systemManaged = this.World.GetExistingSystemManaged<MapTextureReplacerSystem>();

            base.OnCreate();
            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "texture_pack", () =>
            {
                return systemManaged.PackImportedText;
            }));

            this.AddBinding(new TriggerBinding("map_texture", "open_texture_zip", this.systemManaged.OpenTextureZip));

            this.AddBinding(new TriggerBinding<string>("map_texture", "change_pack", this.systemManaged.ChangePack));

            this.AddBinding(new TriggerBinding("map_texture", "open_image_gd", () => this.systemManaged.OpenImage("colossal_TerrainGrassDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "open_image_gn", () => this.systemManaged.OpenImage("colossal_TerrainGrassNormal")));
            this.AddBinding(new TriggerBinding("map_texture", "open_image_dd", () => this.systemManaged.OpenImage("colossal_TerrainDirtDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "open_image_dn", () => this.systemManaged.OpenImage("colossal_TerrainDirtNormal")));
            this.AddBinding(new TriggerBinding("map_texture", "open_image_cd", () => this.systemManaged.OpenImage("colossal_TerrainRockDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "open_image_cn", () => this.systemManaged.OpenImage("colossal_TerrainRockNormal")));

            //reset
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_gd", () => this.systemManaged.ResetTexture("colossal_TerrainGrassDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_gn", () => this.systemManaged.ResetTexture("colossal_TerrainGrassNormal")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_dd", () => this.systemManaged.ResetTexture("colossal_TerrainDirtDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_dn", () => this.systemManaged.ResetTexture("colossal_TerrainDirtNormal")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_cd", () => this.systemManaged.ResetTexture("colossal_TerrainRockDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_cn", () => this.systemManaged.ResetTexture("colossal_TerrainRockNormal")));

            //reset tiling
            this.AddBinding(new TriggerBinding("map_texture", "reset_tiling", ResetTiling));

            AddSlider("slider1", "colossal_TerrainTextureTiling", 0);
            AddSlider("slider2", "colossal_TerrainTextureTiling", 1);
            AddSlider("slider3", "colossal_TerrainTextureTiling", 2);
        }

        private void ResetTiling()
        {
            Shader.SetGlobalVector(Shader.PropertyToID("colossal_TerrainTextureTiling"), new Vector4(160f, 1600f, 2400f, 1f));
        }

        private void AddSlider(string sliderName, string shaderProperty, int vectorIndex)
        {
            this.AddUpdateBinding(new GetterValueBinding<int>("map_texture", $"{sliderName}_Pos", () =>
            {
                return (int)Shader.GetGlobalVector(Shader.PropertyToID(shaderProperty))[vectorIndex];
            }));

            this.AddBinding(new TriggerBinding<int>("map_texture", $"{sliderName}_UpdatedValue", (tileValue) => TileVectorChange(shaderProperty, vectorIndex, tileValue)));
        }

        private void TileVectorChange(string shaderProperty, int vectorIndex, int tileValue)
        {
            int propertyID = Shader.PropertyToID(shaderProperty);
            Vector4 currentVector = Shader.GetGlobalVector(propertyID);
            currentVector[vectorIndex] = tileValue;
            Shader.SetGlobalVector(propertyID, currentVector);
        }
    }
}
