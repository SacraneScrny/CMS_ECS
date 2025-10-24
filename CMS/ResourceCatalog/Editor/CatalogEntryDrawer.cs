using Logic.CMS.ResourceCatalog.ScriptableObjects;

using UnityEditor;

using UnityEngine;

[CustomPropertyDrawer(typeof(CatalogEntry))]
public class CatalogEntryDrawer : PropertyDrawer
{
    private const float KeyWidth = 100f;
    private const float Padding = 3f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("Type");
        var objectsProp = property.FindPropertyRelative("Objects");

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            typeProp.enumDisplayNames[typeProp.enumValueIndex],
            true
        );

        if (!property.isExpanded) return;

        EditorGUI.indentLevel++;

        float y = position.y + EditorGUIUtility.singleLineHeight + Padding;

        for (int i = 0; i < objectsProp.arraySize; i++)
        {
            var element = objectsProp.GetArrayElementAtIndex(i);
            var keyProp = element.FindPropertyRelative("HashKey");
            var objProp = element.FindPropertyRelative("Object");

            Rect keyRect = new Rect(position.x + 15, y, KeyWidth, EditorGUIUtility.singleLineHeight);
            Rect objRect = new Rect(keyRect.xMax + 10, y, position.width - keyRect.width - 35, EditorGUIUtility.singleLineHeight);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            }

            EditorGUI.PropertyField(objRect, objProp, GUIContent.none);

            y += EditorGUIUtility.singleLineHeight + Padding;
        }

        // Кнопка добавить элемент
        Rect addRect = new Rect(position.x + 15, y + 2, position.width - 30, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(addRect, "Add Object"))
        {
            objectsProp.InsertArrayElementAtIndex(objectsProp.arraySize);
        }

        EditorGUI.indentLevel--;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + Padding;

        if (property.isExpanded)
        {
            var objectsProp = property.FindPropertyRelative("Objects");
            height += (EditorGUIUtility.singleLineHeight + Padding) * objectsProp.arraySize;
            height += EditorGUIUtility.singleLineHeight + Padding; // кнопка Add
        }

        return height;
    }
}