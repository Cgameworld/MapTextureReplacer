using Colossal.UI.Binding;
using Game;
using Game.Common;
using Game.Debug;
using Game.Prefabs;
using Game.Routes;
using Game.Serialization;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System.Threading.Tasks;
using Unity.Entities;


namespace MapTextureReplacer.Systems
{
    public class MapTextureReplacerUISystem : UISystemBase
    {
        public int current_vehicle_count = 0;
        public MapTextureReplacerSystem systemManaged;

        protected override void OnCreate()
        {
            this.systemManaged = this.World.GetExistingSystemManaged<MapTextureReplacerSystem>();

            base.OnCreate();
            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "texture_pack", () =>
            {
                return "None";
            }));
            //replace
            this.AddBinding(new TriggerBinding("map_texture", "open_image_gd", () => this.systemManaged.OpenImage("colossal_TerrainGrassDiffuse"))); ;
            this.AddBinding(new TriggerBinding("map_texture", "open_image_gn", () => this.systemManaged.OpenImage("colossal_TerrainGrassNormal"))); ;
            this.AddBinding(new TriggerBinding("map_texture", "open_image_dd", () => this.systemManaged.OpenImage("colossal_TerrainDirtDiffuse"))); ;
            this.AddBinding(new TriggerBinding("map_texture", "open_image_dn", () => this.systemManaged.OpenImage("colossal_TerrainDirtNormal"))); ;
            this.AddBinding(new TriggerBinding("map_texture", "open_image_cd", () => this.systemManaged.OpenImage("colossal_TerrainRockDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "open_image_cn", () => this.systemManaged.OpenImage("colossal_TerrainRockNormal"))); ;

            //reset
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_gd", () => this.systemManaged.ResetTexture("colossal_TerrainGrassDiffuse"))); ;
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_gn", () => this.systemManaged.ResetTexture("colossal_TerrainGrassNormal"))); ;
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_dd", () => this.systemManaged.ResetTexture("colossal_TerrainDirtDiffuse"))); ;
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_dn", () => this.systemManaged.ResetTexture("colossal_TerrainDirtNormal"))); ;
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_cd", () => this.systemManaged.ResetTexture("colossal_TerrainRockDiffuse")));
            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_cn", () => this.systemManaged.ResetTexture("colossal_TerrainRockNormal"))); ;
        }


    }
}
