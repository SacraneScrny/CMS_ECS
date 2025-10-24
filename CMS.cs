using System.Collections.Generic;

using Logic.CMS.CMSComponents;
using Logic.CMS.ResourceCatalog;
using Logic.CMS.ResourceCatalog.ScriptableObjects;

using UnityEngine;

namespace Logic.CMS
{
    [CreateAssetMenu(menuName = "add CMS", fileName = "CMS")]
    public class CMS : ScriptableObject
    {
        public CatalogObject Catalog;
        public List<CMSEntry> Entries;
    }

    [System.Serializable]
    public class CMSEntry
    {
        public CatalogType Type;
        public List<CMSEntryObject> Objects;
    }
    
    [System.Serializable]
    public class CMSEntryObject
    {
        public uint HashKey;
        public GameObject Object;
        public ScriptableObject PrefabType;
    }
}