using UnityEditor;
using UnityEngine;

namespace TopDown.Editor
{
    // Salida del teletransporte que se arrastra en la escena (se ajusta a medio tile).
    [CustomEditor(typeof(Teleporter))]
    public class TeleporterEditor : UnityEditor.Editor
    {
        private const float Snap = 0.5f;

        private void OnSceneGUI()
        {
            if (Application.isPlaying) return;

            var teleporter = (Teleporter)target;
            Vector3 exit = teleporter.Exit;
            Handles.color = new Color(0.8f, 0.3f, 1f);
            Handles.Label(exit + new Vector3(0.4f, 0.4f), "Salida");

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.FreeMoveHandle(exit, HandleUtility.GetHandleSize(exit) * 0.12f, Vector3.zero, Handles.CircleHandleCap);
            if (!EditorGUI.EndChangeCheck()) return;

            Vector2 offset = moved - teleporter.transform.position;
            offset.x = Mathf.Round(offset.x / Snap) * Snap;
            offset.y = Mathf.Round(offset.y / Snap) * Snap;
            serializedObject.Update();
            serializedObject.FindProperty("_exitOffset").vector2Value = offset;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
