using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TopDown.Editor
{
    // Límites de la cámara: esquinas del rectángulo verde que se arrastran en la escena (se ajustan a tiles enteros)
    // y un botón para ajustarlo al terreno pintado.
    [CustomEditor(typeof(CameraFollow))]
    public class CameraFollowEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!serializedObject.FindProperty("_useBounds").boolValue) return;

            if (GUILayout.Button(new GUIContent("Ajustar al terreno pintado",
                    "Pone el rectángulo verde alrededor de todo lo pintado.")))
            {
                FitToTerrain();
            }
        }

        private void OnSceneGUI()
        {
            if (Application.isPlaying) return;
            serializedObject.Update();
            if (!serializedObject.FindProperty("_useBounds").boolValue) return;

            SerializedProperty min = serializedObject.FindProperty("_boundsMin");
            SerializedProperty max = serializedObject.FindProperty("_boundsMax");

            Handles.color = Color.green;
            EditorGUI.BeginChangeCheck();
            Vector2 newMin = CornerHandle(min.vector2Value);
            Vector2 newMax = CornerHandle(max.vector2Value);
            if (!EditorGUI.EndChangeCheck()) return;

            // Si se cruzan las esquinas, se intercambian para que el rectángulo siga siendo válido.
            min.vector2Value = Vector2.Min(newMin, newMax);
            max.vector2Value = Vector2.Max(newMin, newMax);
            serializedObject.ApplyModifiedProperties();
        }

        private static Vector2 CornerHandle(Vector2 corner)
        {
            float size = HandleUtility.GetHandleSize(corner) * 0.12f;
            Vector3 moved = Handles.FreeMoveHandle(corner, size, Vector3.zero, Handles.CircleHandleCap);
            return new Vector2(Mathf.Round(moved.x), Mathf.Round(moved.y));
        }

        private void FitToTerrain()
        {
            bool found = false;
            var bounds = new Bounds();
            foreach (Tilemap tilemap in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                tilemap.CompressBounds();
                if (tilemap.cellBounds.size.x == 0) continue;

                Bounds tilemapBounds = tilemap.localBounds;
                tilemapBounds.center = tilemap.transform.TransformPoint(tilemapBounds.center);
                if (found) bounds.Encapsulate(tilemapBounds);
                else bounds = tilemapBounds;
                found = true;
            }

            if (!found)
            {
                EditorUtility.DisplayDialog("Límites de la cámara", "Todavía no hay nada pintado en el nivel.", "Vale");
                return;
            }

            serializedObject.FindProperty("_boundsMin").vector2Value = bounds.min;
            serializedObject.FindProperty("_boundsMax").vector2Value = (Vector2)bounds.max;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
