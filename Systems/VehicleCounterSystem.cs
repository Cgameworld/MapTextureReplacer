using Colossal.UI.Binding;
using Game;
using Game.Common;
using Game.Prefabs;
using Game.Routes;
using Game.Tools;
using Game.Vehicles;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

// This system is responsible for querying constantly for data about how many entities
// exists with a specific set of components, and for adding the component `Deleted` to
// all of them if RemoveVehicles is called.

namespace MapTextureReplacer.Systems
{
    public class VehicleCounterSystem : GameSystemBase
    {
        private EntityQuery m_VehicleQuery;
        public int current_vehicle_count = 0;

        protected override void OnCreate()
        {
            base.OnCreate();
            // Query for getting all Vehicles in the game
            this.m_VehicleQuery = this.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[1] {
                    ComponentType.ReadOnly<Vehicle>()
                },
                None = new ComponentType[2] {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>()
                }
            });
        }

        protected override void OnUpdate()
        {
            this.current_vehicle_count = this.m_VehicleQuery.CalculateEntityCount();
        }

        public void RemoveVehicles()
        {
            NativeArray<Entity> entityArray = this.m_VehicleQuery.ToEntityArray(Allocator.TempJob);
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var entity in entityArray)
            {
                commandBuffer.AddComponent<Deleted>(entity);
            }
            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
            entityArray.Dispose();
        }
    }
}
