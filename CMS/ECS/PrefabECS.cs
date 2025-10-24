using Logic.CMS.CMSComponents;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace Logic.CMS.ECS
{
    public static class PrefabECS
    {
        public interface IPrefabBuffer { public uint GetHash { get; } public Entity GetEntity { get; } }
    }
}