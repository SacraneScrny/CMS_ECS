using Logic.CMS.CMSComponents;

using UnityEngine;

namespace Logic.CMS.ECS.PrefabTypes
{
    public class ParticlesPrefab : ScriptableObject, IPrefabTypeBase
    {
        public float LifeTime = 1;
        public float Scale = 1;
            
        public string DisplayName => "Particles";
    }
}