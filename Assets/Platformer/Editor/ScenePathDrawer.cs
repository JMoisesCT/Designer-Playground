using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    // Dibuja los campos [ScenePath] como una casilla de escena; guarda la ruta de la escena como texto.
    [CustomPropertyDrawer(typeof(ScenePathAttribute))]
    public class ScenePathDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var current = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var scene = (SceneAsset)EditorGUI.ObjectField(position, label, current, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck()) property.stringValue = scene != null ? AssetDatabase.GetAssetPath(scene) : "";
            EditorGUI.EndProperty();
        }
    }
}
