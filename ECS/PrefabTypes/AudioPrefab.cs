using Logic.CMS.CMSComponents;

using UnityEngine;

namespace Logic.CMS.ECS.PrefabTypes
{
    public class AudioPrefab : ScriptableObject, IPrefabTypeBase
    {
        public float LifeTime = 1;
        public float Volume = 1;
            
        public string DisplayName => "Audio";
    }
}