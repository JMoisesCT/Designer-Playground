using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Hace que las luces 2D iluminen todas las Sorting Layers (si no, los sprites fuera de "Default" se ven negros).
public static class Light2DSetup
{
    [MenuItem("Designer Playground/Escena/Luces 2D: iluminar todas las Sorting Layers", priority = 200)]
    private static void ApplyToOpenScenesFromMenu()
    {
        int updated = ApplyToOpenScenes();
        Debug.Log($"[Setup] {updated} luz(es) 2D actualizadas para iluminar todas las Sorting Layers.");
    }

    public static int ApplyToOpenScenes()
    {
        int updated = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            int updatedInScene = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Light2D light in root.GetComponentsInChildren<Light2D>(true))
                {
                    if (ApplyToAllSortingLayers(light)) updatedInScene++;
                }
            }

            if (updatedInScene > 0) EditorSceneManager.MarkSceneDirty(scene);
            updated += updatedInScene;
        }
        return updated;
    }

    // Devuelve true si la luz cambió.
    public static bool ApplyToAllSortingLayers(Light2D light)
    {
        SortingLayer[] layers = SortingLayer.layers;
        var serializedLight = new SerializedObject(light);
        SerializedProperty targets = serializedLight.FindProperty("m_ApplyToSortingLayers");

        bool alreadyAll = targets.arraySize == layers.Length;
        for (int i = 0; alreadyAll && i < layers.Length; i++)
        {
            alreadyAll = targets.GetArrayElementAtIndex(i).intValue == layers[i].id;
        }
        if (alreadyAll) return false;

        targets.arraySize = layers.Length;
        for (int i = 0; i < layers.Length; i++)
        {
            targets.GetArrayElementAtIndex(i).intValue = layers[i].id;
        }
        serializedLight.ApplyModifiedProperties();
        return true;
    }
}
