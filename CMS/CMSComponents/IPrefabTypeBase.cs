using UnityEngine;

namespace Logic.CMS.CMSComponents
{
    public interface IPrefabTypeBase
    {
        public string DisplayName { get; }
        public int GetInstanceID();
    }
}