using Logic.CMS.ECS.Generated;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Logic.CMS.ECS.Events
{
    public partial struct ParticleEventSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ParticleEvent>();
            state.RequireForUpdate<ParticlesPrefabBuffer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var prefabBuffer = SystemAPI.GetSingletonBuffer<ParticlesPrefabBuffer>();
            var ecbES = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var e in SystemAPI.Query<RefRO<ParticleEvent>>())
            {
                if (!prefabBuffer.GetPrefab(e.ValueRO.HashKey, out var prefab)) continue;
                var entity = ecbES.Instantiate(prefab.Prefab);
                ecbES.AddComponent(entity, new ParticleSimulation { TimeLeft = prefab.LifeTime });
                ecbES.SetComponent(entity, new LocalTransform()
                {
                    Position = e.ValueRO.Position,
                    Rotation = e.ValueRO.Rotation,
                    Scale = prefab.Scale * e.ValueRO.ScaleMultiplier
                });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
    
    public partial struct ParticleLifetimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ParticleSimulation>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbES = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var dt = SystemAPI.Time.DeltaTime;
            
            foreach (var (particle, entity) in SystemAPI.Query<RefRW<ParticleSimulation>>().WithEntityAccess())
            {
                if (particle.ValueRO.TimeLeft > 0)
                {
                    particle.ValueRW.TimeLeft -= dt;
                    continue;
                }
                ecbES.DestroyEntity(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    public struct ParticleEvent : IComponentData
    {
        public uint HashKey;
        public float3 Position;
        public quaternion Rotation;
        public float ScaleMultiplier;
    }
    public struct ParticleSimulation : IComponentData
    {
        public float TimeLeft;
    }
}