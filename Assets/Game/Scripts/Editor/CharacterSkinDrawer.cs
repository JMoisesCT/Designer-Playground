using System.Linq;
using UnityEditor;
using UnityEngine;

// Muestra cualquier campo CharacterSkinSO como un desplegable con todos los personajes del proyecto.
[CustomPropertyDrawer(typeof(CharacterSkinSO))]
public class CharacterSkinDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CharacterSkinSO[] skins = AssetDatabase.FindAssets($"t:{nameof(CharacterSkinSO)}")
            .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterSkinSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(skin => skin != null)
            .OrderBy(skin => skin.DisplayName)
            .ToArray();

        var options = new GUIContent[skins.Length + 1];
        options[0] = new GUIContent("(Ninguno)");
        for (int i = 0; i < skins.Length; i++)
        {
            options[i + 1] = new GUIContent(skins[i].DisplayName);
        }

        var current = property.objectReferenceValue as CharacterSkinSO;
        int currentIndex = current == null ? 0 : System.Array.IndexOf(skins, current) + 1;

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUI.Popup(position, label, currentIndex, options);
        if (EditorGUI.EndChangeCheck())
        {
            property.objectReferenceValue = newIndex == 0 ? null : skins[newIndex - 1];
        }
        EditorGUI.EndProperty();
    }
}
