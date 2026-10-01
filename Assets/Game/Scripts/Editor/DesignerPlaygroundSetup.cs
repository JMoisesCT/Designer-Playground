using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Setup del proyecto (Fase 0). Todos los pasos son idempotentes: se pueden ejecutar
/// tantas veces como haga falta sin duplicar nada.
/// </summary>
public static class DesignerPlaygroundSetup
{
    private const string MenuRoot = "Designer Playground/Setup/";
    private const int FirstUserLayer = 6;

    private static readonly string[] Folders =
    {
        "Assets/Game/Scripts/Core",
        "Assets/Game/Scripts/Player",
        "Assets/Game/Scripts/Gameplay",
        "Assets/Game/Scripts/Hazards",
        "Assets/Game/Scripts/Collectibles",
        "Assets/Game/Scripts/Platforms",
        "Assets/Game/Scripts/Camera",
        "Assets/Game/Scripts/Audio",
        "Assets/Game/Scripts/UI",
        "Assets/Game/Scripts/Level",
        "Assets/Game/Scripts/Listeners",
        "Assets/Game/Scripts/Editor",
        "Assets/Game/Data",
        "Assets/Game/Prefabs/Player",
        "Assets/Game/Prefabs/Hazards",
        "Assets/Game/Prefabs/Platforms",
        "Assets/Game/Prefabs/Collectibles",
        "Assets/Game/Prefabs/Checkpoints",
        "Assets/Game/Prefabs/Systems",
        "Assets/Game/Prefabs/UI",
        "Assets/Game/Art/Animations",
        "Assets/Game/Art/Tiles",
        "Assets/Game/Art/Palettes",
        "Assets/Game/Audio/Cues",
        "Assets/Game/Audio/Mixer",
        "Assets/Game/Scenes/Levels",
        "Assets/Game/Scenes/Templates",
        "Assets/Game/Scenes/Sandbox",
    };

    // Pares de capas que NO colisionan entre sí. El resto colisiona (valor por defecto de Unity).
    private static readonly (string, string)[] IgnoredCollisions =
    {
        (GameLayers.Physics.Player, GameLayers.Physics.Player),
        (GameLayers.Physics.Hazard, GameLayers.Physics.Hazard),
        (GameLayers.Physics.Collectible, GameLayers.Physics.Collectible),
        (GameLayers.Physics.Collectible, GameLayers.Physics.Hazard),
        (GameLayers.Physics.Collectible, GameLayers.Physics.Interactable),
    };

    [MenuItem(MenuRoot + "Ejecutar setup completo (Fase 0)", priority = 0)]
    public static void RunFullSetup()
    {
        CreateFolders();
        ConfigurePhysicsLayers();
        ConfigureSortingLayers();
        ConfigureCollisionMatrix();
        ConfigureQualityForPixelArt();
        ReimportPixelArt();
        AssetDatabase.SaveAssets();
        Debug.Log("[Setup] Fase 0 completada.");
    }

    [MenuItem(MenuRoot + "Crear estructura de carpetas", priority = 20)]
    public static void CreateFolders()
    {
        int created = 0;
        foreach (string folder in Folders)
        {
            if (EnsureFolder(folder))
            {
                created++;
            }
        }
        Debug.Log($"[Setup] Carpetas: {created} creadas, {Folders.Length - created} ya existían.");
    }

    [MenuItem(MenuRoot + "Configurar capas de física", priority = 21)]
    public static void ConfigurePhysicsLayers()
    {
        SerializedObject tagManager = LoadTagManager();
        SerializedProperty layers = tagManager.FindProperty("layers");

        foreach (string layerName in GameLayers.Physics.All)
        {
            if (FindLayerIndex(layers, layerName) >= 0)
            {
                continue;
            }

            int freeIndex = FindFreeUserLayer(layers);
            if (freeIndex < 0)
            {
                Debug.LogError($"[Setup] No quedan capas libres para '{layerName}'.");
                continue;
            }

            layers.GetArrayElementAtIndex(freeIndex).stringValue = layerName;
            Debug.Log($"[Setup] Capa '{layerName}' creada en el índice {freeIndex}.");
        }

        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    [MenuItem(MenuRoot + "Configurar Sorting Layers", priority = 22)]
    public static void ConfigureSortingLayers()
    {
        SerializedObject tagManager = LoadTagManager();
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");

        var existing = new List<(string name, uint id, bool locked)>();
        for (int i = 0; i < sortingLayers.arraySize; i++)
        {
            SerializedProperty element = sortingLayers.GetArrayElementAtIndex(i);
            existing.Add((element.FindPropertyRelative("name").stringValue,
                          element.FindPropertyRelative("uniqueID").uintValue,
                          element.FindPropertyRelative("locked").boolValue));
        }

        // Orden final: las del proyecto en su orden, y después las que haya creado alguien a mano.
        var ordered = new List<(string name, uint id, bool locked)>();
        var usedIds = new HashSet<uint>();
        foreach ((string _, uint id, bool _) in existing)
        {
            usedIds.Add(id);
        }

        foreach (string layerName in GameLayers.Sorting.All)
        {
            int found = existing.FindIndex(l => l.name == layerName);
            if (found >= 0)
            {
                ordered.Add(existing[found]);
                existing.RemoveAt(found);
                continue;
            }

            uint newId = StableId(layerName);
            while (newId == 0 || usedIds.Contains(newId))
            {
                newId++;
            }
            usedIds.Add(newId);
            ordered.Add((layerName, newId, false));
            Debug.Log($"[Setup] Sorting Layer '{layerName}' creada.");
        }
        ordered.AddRange(existing);

        sortingLayers.arraySize = ordered.Count;
        for (int i = 0; i < ordered.Count; i++)
        {
            SerializedProperty element = sortingLayers.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("name").stringValue = ordered[i].name;
            element.FindPropertyRelative("uniqueID").uintValue = ordered[i].id;
            element.FindPropertyRelative("locked").boolValue = ordered[i].locked;
        }

        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    [MenuItem(MenuRoot + "Configurar matriz de colisiones 2D", priority = 23)]
    public static void ConfigureCollisionMatrix()
    {
        foreach ((string a, string b) in IgnoredCollisions)
        {
            int layerA = LayerMask.NameToLayer(a);
            int layerB = LayerMask.NameToLayer(b);
            if (layerA < 0 || layerB < 0)
            {
                Debug.LogWarning($"[Setup] Falta la capa '{a}' o '{b}'. Ejecuta primero 'Configurar capas de física'.");
                continue;
            }
            Physics2D.IgnoreLayerCollision(layerA, layerB, true);
        }

        Object physics2DSettings = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/Physics2DSettings.asset");
        if (physics2DSettings != null)
        {
            EditorUtility.SetDirty(physics2DSettings);
        }
        Debug.Log("[Setup] Matriz de colisiones 2D configurada.");
    }

    [MenuItem(MenuRoot + "Configurar calidad para pixel art", priority = 24)]
    public static void ConfigureQualityForPixelArt()
    {
        // Antialiasing y filtrado anisotrópico difuminan los bordes de los píxeles.
        Object qualityAsset = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/QualitySettings.asset");
        var quality = new SerializedObject(qualityAsset);
        SerializedProperty levels = quality.FindProperty("m_QualitySettings");
        for (int i = 0; i < levels.arraySize; i++)
        {
            SerializedProperty level = levels.GetArrayElementAtIndex(i);
            level.FindPropertyRelative("antiAliasing").intValue = 0;
            level.FindPropertyRelative("anisotropicTextures").intValue = (int)AnisotropicFiltering.Disable;
        }
        quality.ApplyModifiedPropertiesWithoutUndo();

        var pipelines = new HashSet<UniversalRenderPipelineAsset>();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            if (QualitySettings.GetRenderPipelineAssetAt(i) is UniversalRenderPipelineAsset urp)
            {
                pipelines.Add(urp);
            }
        }
        if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset defaultUrp)
        {
            pipelines.Add(defaultUrp);
        }

        foreach (UniversalRenderPipelineAsset urp in pipelines)
        {
            urp.msaaSampleCount = 1;
            EditorUtility.SetDirty(urp);
        }

        Debug.Log($"[Setup] Calidad: AA y anisotrópico desactivados en {levels.arraySize} niveles, MSAA off en {pipelines.Count} asset(s) URP.");
    }

    [MenuItem(MenuRoot + "Reimportar sprites de pixel art", priority = 40)]
    public static void ReimportPixelArt()
    {
        var folders = new List<string>();
        foreach (string folder in PixelArtImportPostprocessor.PixelArtFolders)
        {
            string trimmed = folder.TrimEnd('/');
            if (AssetDatabase.IsValidFolder(trimmed))
            {
                folders.Add(trimmed);
            }
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", folders.ToArray());
        int reimported = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is TextureImporter importer &&
                    !Mathf.Approximately(importer.spritePixelsPerUnit, GameConstants.PixelsPerUnit))
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    reimported++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        Debug.Log($"[Setup] Pixel art: {reimported} texturas reimportadas de {guids.Length}.");
    }

    private static bool EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return false;
        }

        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        return true;
    }

    private static SerializedObject LoadTagManager()
    {
        return new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
    }

    private static int FindLayerIndex(SerializedProperty layers, string layerName)
    {
        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
            {
                return i;
            }
        }
        return -1;
    }

    private static int FindFreeUserLayer(SerializedProperty layers)
    {
        for (int i = FirstUserLayer; i < layers.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
            {
                return i;
            }
        }
        return -1;
    }

    // Hash FNV-1a: el mismo nombre genera el mismo id en cualquier máquina.
    private static uint StableId(string text)
    {
        uint hash = 2166136261;
        foreach (char c in text)
        {
            hash ^= c;
            hash *= 16777619;
        }
        return hash;
    }
}
