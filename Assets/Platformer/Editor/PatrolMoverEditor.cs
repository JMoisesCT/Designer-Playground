using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    // Permite arrastrar en la escena el punto final del recorrido de un PatrolMover (se ajusta a medio tile).
    [CustomEditor(typeof(PatrolMover))]
    public class PatrolMoverEditor : UnityEditor.Editor
    {
        private const float Snap = 0.5f;

        private void OnSceneGUI()
        {
            if (Application.isPlaying) return;

            var mover = (PatrolMover)target;
            Vector2 start = mover.StartPosition;
            Vector3 end = start + mover.EndOffset;

            Handles.color = Color.yellow;
            float size = HandleUtility.GetHandleSize(end) * 0.12f;

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.FreeMoveHandle(end, size, Vector3.zero, Handles.CircleHandleCap);
            if (!EditorGUI.EndChangeCheck()) return;

            Vector2 offset = (Vector2)moved - start;
            offset.x = Mathf.Round(offset.x / Snap) * Snap;
            offset.y = Mathf.Round(offset.y / Snap) * Snap;

            serializedObject.Update();
            serializedObject.FindProperty("_endOffset").vector2Value = offset;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
