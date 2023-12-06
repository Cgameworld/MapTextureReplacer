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

// This system is responsible for binding the value from VehicleCounterSystem to our Game UI.

// It also sets up a TriggerBinding that calls RemoveVehicles when UI tells it to.

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
            this.AddUpdateBinding(new GetterValueBinding<int>("vehicle_counter", "current_vehicle_count", () =>
            {
                return this.systemManaged.current_vehicle_count;
            }));

            this.AddBinding(new TriggerBinding("vehicle_counter", "remove_vehicles", this.systemManaged.RemoveVehicles));
        }


    }
}
