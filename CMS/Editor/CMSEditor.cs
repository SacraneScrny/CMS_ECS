#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using Logic.CMS.ResourceCatalog;
using Logic.CMS.CMSComponents;

namespace Logic.CMS.Editor
{
    [CustomEditor(typeof(CMS))]
    public class CMSEditor : UnityEditor.Editor
    {
        private SerializedProperty catalogProp;
        private SerializedProperty entriesProp;
        private Dictionary<CatalogType, bool> foldouts = new();
        private Dictionary<int, bool> prefabFoldouts = new();
        private Dictionary<int, UnityEditor.Editor> prefabEditors = new();
        private bool showEntries = true;

        private const string PrefabStoragePath = "Assets/Resources/CMSdata";

        private void OnEnable()
        {
            catalogProp = serializedObject.FindProperty("Catalog");
            entriesProp = serializedObject.FindProperty("Entries");

            if (!Directory.Exists(PrefabStoragePath))
                Directory.CreateDirectory(PrefabStoragePath);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(catalogProp);
            var cms = (CMS)target;

            if (cms.Catalog != null)
                SyncEntries(cms);

            EditorGUILayout.Space(8);

            showEntries = EditorGUILayout.Foldout(showEntries, "Entries", true);
            if (showEntries)
            {
                for (int i = 0; i < entriesProp.arraySize; i++)
                {
                    var element = entriesProp.GetArrayElementAtIndex(i);
                    DrawEntry(element);
                    EditorGUILayout.Space(5);
                }
            }

            serializedObject.ApplyModifiedProperties();
            CleanupOrphanPrefabTypes(cms);
        }

        private void DrawEntry(SerializedProperty element)
        {
            var typeProp = element.FindPropertyRelative("Type");
            var objListProp = element.FindPropertyRelative("Objects");
            var typeValue = (CatalogType)typeProp.enumValueIndex;

            if (!foldouts.ContainsKey(typeValue))
                foldouts[typeValue] = true;

            foldouts[typeValue] = EditorGUILayout.Foldout(foldouts[typeValue], typeValue.ToString(), true);
            if (!foldouts[typeValue]) return;

            EditorGUI.indentLevel++;

            for (int j = 0; j < objListProp.arraySize; j++)
            {
                var objElement = objListProp.GetArrayElementAtIndex(j);
                var hashProp = objElement.FindPropertyRelative("HashKey");
                var objProp = objElement.FindPropertyRelative("Object");
                var prefabTypeProp = objElement.FindPropertyRelative("PrefabType"); // ScriptableObject implementing IPrefabTypeBase

                EditorGUILayout.BeginVertical(GUI.skin.box);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(hashProp, new GUIContent("Hash"));
                    EditorGUILayout.PropertyField(objProp, new GUIContent("Object"));
                }

                var prefabType = prefabTypeProp?.objectReferenceValue as ScriptableObject;
                if (prefabType != null)
                {
                    string foldoutLabel = (prefabType as IPrefabTypeBase)?.DisplayName ?? prefabType.name;

                    if (!prefabFoldouts.ContainsKey(prefabType.GetInstanceID()))
                        prefabFoldouts[prefabType.GetInstanceID()] = true;

                    prefabFoldouts[prefabType.GetInstanceID()] =
                        EditorGUILayout.Foldout(prefabFoldouts[prefabType.GetInstanceID()], foldoutLabel, true);

                    if (prefabFoldouts[prefabType.GetInstanceID()])
                    {
                        EditorGUI.indentLevel++;
                        if (!prefabEditors.TryGetValue(prefabType.GetInstanceID(), out var editor))
                            prefabEditors[prefabType.GetInstanceID()] = editor = UnityEditor.Editor.CreateEditor(prefabType);

                        editor?.OnInspectorGUI();
                        EditorGUI.indentLevel--;

                        if (GUILayout.Button("Delete Prefab Type"))
                        {
                            string path = AssetDatabase.GetAssetPath(prefabType);
                            if (!string.IsNullOrEmpty(path))
                                AssetDatabase.DeleteAsset(path);

                            prefabTypeProp.objectReferenceValue = null;
                            prefabEditors.Remove(prefabType.GetInstanceID());
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Create Prefab Type"))
                    {
                        GenericMenu menu = new GenericMenu();
                        var types = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                            .Where(t => typeof(IPrefabTypeBase).IsAssignableFrom(t));

                        foreach (var t in types)
                        {
                            var localType = t;
                            menu.AddItem(new GUIContent(localType.Name), false, () =>
                            {
                                string objName = objProp.objectReferenceValue ? objProp.objectReferenceValue.name : "Null";
                                string fileName = $"{localType.Name}_{objName}_{hashProp.uintValue}.asset";
                                string path = Path.Combine(PrefabStoragePath, fileName);

                                // создаём standalone asset
                                var instance = ScriptableObject.CreateInstance(localType);
                                AssetDatabase.CreateAsset(instance, path);
                                EditorUtility.SetDirty(instance);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                                prefabTypeProp.objectReferenceValue = instance;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        menu.ShowAsContext();
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
        }

        private void SyncEntries(CMS cms)
        {
            // удаляем энтри с несуществующими CatalogType
            var validTypes = cms.Catalog.Catalog.Select(c => c.Type).ToHashSet();
            cms.Entries.RemoveAll(e => !validTypes.Contains(e.Type));

            var catalogObjects = cms.Catalog.Catalog
                .SelectMany(e => e.Objects.Select(o => new { e.Type, o }))
                .ToList();

            var existingEntries = cms.Entries.ToDictionary(e => e.Type);

            foreach (var group in catalogObjects.GroupBy(x => x.Type))
            {
                if (!existingEntries.TryGetValue(group.Key, out var cmsEntry))
                {
                    cmsEntry = new CMSEntry { Type = group.Key, Objects = new List<CMSEntryObject>() };
                    cms.Entries.Add(cmsEntry);
                }

                var existingHashes = cmsEntry.Objects.Select(o => o.HashKey).ToHashSet();
                foreach (var obj in group)
                {
                    if (!existingHashes.Contains(obj.o.HashKey))
                    {
                        cmsEntry.Objects.Add(new CMSEntryObject
                        {
                            HashKey = obj.o.HashKey,
                            Object = obj.o.Object
                        });
                    }
                }

                cmsEntry.Objects.RemoveAll(o => group.All(g => g.o.HashKey != o.HashKey));
            }

            cms.Entries = cms.Entries.GroupBy(e => e.Type).Select(g => g.First()).ToList();
            EditorUtility.SetDirty(cms);
        }

        private void CleanupOrphanPrefabTypes(CMS cms)
        {
            if (!Directory.Exists(PrefabStoragePath)) return;

            var allAssets = Directory.GetFiles(PrefabStoragePath, "*.asset", SearchOption.AllDirectories)
                .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
                .Where(s => s is IPrefabTypeBase)
                .ToList();

            var validRefs = cms.Entries
                .SelectMany(e => e.Objects)
                .Select(o => o.PrefabType)
                .Where(x => x != null)
                .ToHashSet();

            foreach (var asset in allAssets)
            {
                if (!validRefs.Contains(asset))
                {
                    string path = AssetDatabase.GetAssetPath(asset);
                    if (!string.IsNullOrEmpty(path))
                        AssetDatabase.DeleteAsset(path);

                    prefabEditors.Remove(asset.GetInstanceID());
                }
            }
        }
    }
}
#endif
