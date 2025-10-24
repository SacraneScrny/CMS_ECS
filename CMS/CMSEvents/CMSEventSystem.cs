using Unity.Burst;
using Unity.Entities;

namespace Logic.CMS.CMSEvents
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CMSEventSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CMSEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb =
                SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (eventComponent, entity) in SystemAPI.Query<RefRO<CMSEvent>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
        }
        
        public void OnDestroy(ref SystemState state)
        {
            
        }
    }

    public struct CMSEvent : IComponentData
    {
        public Entity CalledFrom;
    }

    public static class CMSE
    {
        /// <summary>
        /// CREATE ON END SIMULATION
        /// </summary>
        public static Entity CreateEvent<T>(EntityCommandBuffer ecb, Entity calledFrom, T component) where T : unmanaged, IComponentData
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new CMSEvent { CalledFrom = calledFrom });
            ecb.AddComponent(entity, component);
            return entity;
        }
        
        /// <summary>
        /// CREATE ON END SIMULATION
        /// </summary>
        public static Entity CreateEvent<T>(EntityCommandBuffer ecb, T component) where T : unmanaged, IComponentData
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new CMSEvent());
            ecb.AddComponent(entity, component);
            return entity;
        }
    }
}