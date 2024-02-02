using Colossal.IO.AssetDatabase.Internal;
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
using MapTextureReplacer.Helpers;
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

            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "get_detected_packs", () =>
            {
                return systemManaged.importedPacksJsonString;
            }));

            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "get_texture_select_data", () =>
            {
                return systemManaged.textureSelectDataJsonString;
            }));

            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_select_data", this.systemManaged.ResetTextureSelectData));

            this.AddBinding(new TriggerBinding("map_texture", "open_texture_zip", this.systemManaged.GetTextureZip));

            this.AddBinding(new TriggerBinding<string>("map_texture", "change_pack", this.systemManaged.ChangePack));


            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "get_active_pack_dropdown", () => this.systemManaged.GetActivePackDropdown()));
            this.AddBinding(new TriggerBinding<string>("map_texture", "set_active_pack_dropdown", this.systemManaged.SetActivePackDropdown));
            

            this.AddBinding(new TriggerBinding<string>("map_texture", "open_image_gd", (imageFile) => this.systemManaged.OpenImage("colossal_TerrainGrassDiffuse", imageFile)));
         
            this.AddBinding(new TriggerBinding<string>("map_texture", "open_image_gn", (imageFile) => this.systemManaged.OpenImage("colossal_TerrainGrassNormal", imageFile)));
            
            this.AddBinding(new TriggerBinding<string>("map_texture", "open_image_dd", (imageFile) => this.systemManaged.OpenImage("colossal_TerrainDirtDiffuse", imageFile)));
    
            this.AddBinding(new TriggerBinding<string>("map_texture", "open_image_dn", (imageFile) => this.systemManaged.OpenImage("colossal_TerrainDirtNormal", imageFile)));
         
            this.AddBinding(new TriggerBinding<string>("map_texture", "open_image_cd", (imageFile) => this.systemManaged.OpenImage("colossal_TerrainRockDiffuse", imageFile)));

            this.AddBinding(new TriggerBinding<string>("map_texture", "open_image_cn", (imageFile) => this.systemManaged.OpenImage("colossal_TerrainRockNormal", imageFile)));

            //reset
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_gd", () => this.systemManaged.ResetTexture("colossal_TerrainGrassDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_gn", () => this.systemManaged.ResetTexture("colossal_TerrainGrassNormal")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_dd", () => this.systemManaged.ResetTexture("colossal_TerrainDirtDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_dn", () => this.systemManaged.ResetTexture("colossal_TerrainDirtNormal")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_cd", () => this.systemManaged.ResetTexture("colossal_TerrainRockDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_cn", () => this.systemManaged.ResetTexture("colossal_TerrainRockNormal")));

            //reset tiling
            this.AddBinding(new TriggerBinding("map_texture", "reset_tiling", this.systemManaged.SetTilingValueDefault));

            AddSlider("slider1", "colossal_TerrainTextureTiling", 0);
            AddSlider("slider2", "colossal_TerrainTextureTiling", 1);
            AddSlider("slider3", "colossal_TerrainTextureTiling", 2);
        }



        private void AddSlider(string sliderName, string shaderProperty, int vectorIndex)
        {
            this.AddUpdateBinding(new GetterValueBinding<int>("map_texture", $"{sliderName}_Pos", () =>
            {
                return (int)Shader.GetGlobalVector(Shader.PropertyToID(shaderProperty))[vectorIndex];
            }));

            this.AddBinding(new TriggerBinding<int>("map_texture", $"{sliderName}_UpdatedValue", (tileValue) => this.systemManaged.TileVectorChange(shaderProperty, vectorIndex, tileValue)));
        }
 
    }
}
