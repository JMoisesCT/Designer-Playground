using UnityEditor;
using UnityEngine;

namespace TopDown.Editor
{
    // Puntos de la ruta de un PathMover que se arrastran en la escena (se ajustan a medio tile) y botón para añadir más.
    [CustomEditor(typeof(PathMover))]
    public class PathMoverEditor : UnityEditor.Editor
    {
        private const float Snap = 0.5f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button(new GUIContent("Añadir punto", "Añade un punto al final de la ruta, 2 tiles a la derecha del último.")))
            {
                SerializedProperty points = serializedObject.FindProperty("_points");
                Vector2 last = points.arraySize > 0 ? points.GetArrayElementAtIndex(points.arraySize - 1).vector2Value : Vector2.zero;
                points.arraySize++;
                points.GetArrayElementAtIndex(points.arraySize - 1).vector2Value = last + new Vector2(2f, 0f);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnSceneGUI()
        {
            if (Application.isPlaying) return;

            var mover = (PathMover)target;
            Vector2 start = mover.GetPoint(0);
            serializedObject.Update();
            SerializedProperty points = serializedObject.FindProperty("_points");

            Handles.color = Color.yellow;
            for (int i = 0; i < points.arraySize; i++)
            {
                Vector3 point = mover.GetPoint(i + 1);
                float size = HandleUtility.GetHandleSize(point) * 0.12f;
                Handles.Label(point + new Vector3(0.3f, 0.3f), (i + 1).ToString());

                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.FreeMoveHandle(point, size, Vector3.zero, Handles.CircleHandleCap);
                if (!EditorGUI.EndChangeCheck()) continue;

                Vector2 offset = (Vector2)moved - start;
                offset.x = Mathf.Round(offset.x / Snap) * Snap;
                offset.y = Mathf.Round(offset.y / Snap) * Snap;
                points.GetArrayElementAtIndex(i).vector2Value = offset;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
