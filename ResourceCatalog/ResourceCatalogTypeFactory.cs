using System;
using System.IO;

using Logic.CMS.ResourceCatalog.ScriptableObjects;
using Logic.Unit.UnitWorld;

using UnityEngine;

namespace Logic.CMS.ResourceCatalog
{
    public static class ResourceCatalogTypeFactory
    {
        public static CatalogType GetCatalogType(string folderName)
        {
            folderName = folderName.Replace("\\", "/");

            int index = folderName.IndexOf("CMS/", StringComparison.Ordinal);
            if (index == -1)
            {
                Debug.LogWarning($"Folder not under CMS: {folderName}");
                return default;
            }

            string relative = folderName.Substring(index + 4);
            string enumName = relative.Replace("/", "_");

            if (Enum.TryParse(enumName, true, out CatalogType result))
                return result;

            Debug.LogWarning($"Unknown catalog folder: {folderName} -> {enumName}");
            return default;
        }
    }
}