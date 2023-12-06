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
    public class VehicleCounterUISystem : UISystemBase
    {
        public int current_vehicle_count = 0;
        public VehicleCounterSystem systemManaged;

        protected override void OnCreate()
        {
            this.systemManaged = this.World.GetExistingSystemManaged<VehicleCounterSystem>();

            base.OnCreate();
            this.AddUpdateBinding(new GetterValueBinding<int>("map_texture", "current_vehicle_count", () =>
            {
                return 0;
            }));

            this.AddBinding(new TriggerBinding("map_texture", "open_image", this.systemManaged.OpenImage));
        }


    }
}
