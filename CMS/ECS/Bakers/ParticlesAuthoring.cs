using System;
using System.Linq;

using Unity.Entities;

using UnityEngine;

namespace Logic.CMS.ECS.Bakers
{
    public class ParticlesAuthoring : MonoBehaviour
    {
        public CMS CMS;
        private void OnValidate()
        {
            gameObject.name = this.GetType().Name;
        }

        private class ParticlesBaker : Baker<ParticlesAuthoring>
        {
            public override void Bake(ParticlesAuthoring authoring)
            {
                if (authoring.CMS == null) return;
                DependsOn(authoring.CMS);
                
                var _entity = GetEntity(authoring, TransformUsageFlags.None);
                AddBuffer<ParticlePrefabBuffer>(_entity);
                
                foreach (var a in authoring.CMS.Entries.SelectMany(x => x.Objects))
                {
                    if (a.PrefabType is not PrefabTypes.ParticlesPrefab p) continue;
                    var prefab = new ParticlePrefabBuffer()
                    {
                        HashKey = a.HashKey,
                        Prefab = GetEntity(a.Object, TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable),
                        LifeTime = p.LifeTime,
                        Scale = p.Scale
                    };
                    AppendToBuffer(_entity, prefab);
                }
            }
        }
    }

    public struct ParticlePrefabBuffer : IBufferElementData, PrefabECS.IPrefabBuffer
    {
        public uint HashKey;
        public Entity Prefab;
        public float LifeTime;
        public float Scale;

        public uint GetHash => HashKey;
        public Entity GetEntity => Prefab;
    }
}