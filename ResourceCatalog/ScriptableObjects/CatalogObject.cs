using System.Collections.Generic;
using System.Linq;

using Unity.Collections;

using UnityEngine;

namespace Logic.CMS.ResourceCatalog.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Catalog Object", menuName = "New Catalog Object")]
    public class CatalogObject : ScriptableObject
    {
        public List<CatalogEntry> Catalog = new List<CatalogEntry>();
        
        public List<CatalogEntryObject> GetOrCreate(CatalogType type)
        {
            var entry = Catalog.FirstOrDefault(e => e.Type == type);
            if (entry == null)
            {
                entry = new CatalogEntry { Type = type, Objects = new List<CatalogEntryObject>() };
                Catalog.Add(entry);
            }
            return entry.Objects;
        }
    }

    [System.Serializable]
    public class CatalogEntry
    {
        public CatalogType Type;

        public List<CatalogEntryObject> Objects = new ();
    }

    [System.Serializable]
    public class CatalogEntryObject
    {
        public uint HashKey;
        public GameObject Object;
    }
}