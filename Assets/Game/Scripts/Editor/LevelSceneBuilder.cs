using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Crea escenas de nivel listas para pintar (jugador, cámara, luz, fondo y Tilemaps con colisión) y la paleta de terreno.
public static class LevelSceneBuilder
{
    private const string LevelsFolder = "Assets/Game/Scenes/Levels";
    private const string PalettesFolder = "Assets/Game/Art/Palettes";
    private const string PaletteName = "Terreno";
    private const string PalettePath = PalettesFolder + "/" + PaletteName + ".prefab";
    private const string TilesFolder = "Assets/Game/Art/Tiles/Terrain";
    private const string PlayerPrefabPath = "Assets/Game/Prefabs/Player/Player.prefab";
    private const string BackgroundSpritePath = "Assets/ThirdParty/Pixel Adventure 1/Assets/Background/Blue.png";

    public const string TerrainTilemapName = "Terreno";
    public const string OneWayTilemapName = "Plataformas (atravesables)";
    public const string DecorationTilemapName = "Decoración (sin colisión)";

    // Posición de cada tile en la paleta.
    private static readonly (string Tile, Vector3Int Cell)[] PaletteLayout =
    {
        ("Grass_TL", new Vector3Int(0, 2, 0)), ("Grass_TC", new Vector3Int(1, 2, 0)), ("Grass_TR", new Vector3Int(2, 2, 0)),
        ("Grass_ML", new Vector3Int(0, 1, 0)), ("Grass_MC", new Vector3Int(1, 1, 0)), ("Grass_MR", new Vector3Int(2, 1, 0)),
        ("Grass_BL", new Vector3Int(0, 0, 0)), ("Grass_BC", new Vector3Int(1, 0, 0)), ("Grass_BR", new Vector3Int(2, 0, 0)),
        ("OneWayWood_L", new Vector3Int(4, 2, 0)), ("OneWayWood_C", new Vector3Int(5, 2, 0)), ("OneWayWood_R", new Vector3Int(6, 2, 0)),
    };

    [MenuItem("Designer Playground/Nivel/Crear nivel nuevo", priority = 400)]
    private static void CreateLevelFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder(LevelsFolder);
        string path = EditorUtility.SaveFilePanelInProject(
            "Crear nivel nuevo", "Nivel_01", "unity", "Elige el nombre del nivel.", LevelsFolder);
        if (string.IsNullOrEmpty(path)) return;

        CreateLevel(path);
    }

    [MenuItem("Designer Playground/Nivel/Crear o actualizar paleta de terreno", priority = 401)]
    private static void BuildTerrainPaletteFromMenu()
    {
        GameObject palette = BuildTerrainPalette();
        EditorGUIUtility.PingObject(palette);
        Debug.Log($"[Nivel] Paleta '{PaletteName}' lista en {PalettePath}.", palette);
    }

    public static Scene CreateLevel(string scenePath)
    {
        GameObject palette = BuildTerrainPalette();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var lightObject = new GameObject("Global Light 2D");
        var light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        Light2DSetup.ApplyToAllSortingLayers(light);

        var grid = new GameObject("Grid").AddComponent<Grid>();
        CreateTilemap(grid, DecorationTilemapName, GameLayers.Sorting.Background, 10, null);
        Tilemap terrain = CreateTilemap(grid, TerrainTilemapName, GameLayers.Sorting.Terrain, 0, GameLayers.Physics.Ground);
        Tilemap oneWay = CreateTilemap(grid, OneWayTilemapName, GameLayers.Sorting.Terrain, 1, GameLayers.Physics.OneWayPlatform);

        // Se pinta antes de añadir los colliders: si no, la escena se guarda con el collider vacío.
        PaintStarterGround(terrain, oneWay);
        AddCollision(terrain, oneWay: false);
        AddCollision(oneWay, oneWay: true);

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        GameObject player = null;
        if (playerPrefab != null)
        {
            player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.position = Vector3.zero;
        }
        else
        {
            Debug.LogWarning($"[Nivel] No se encontró el prefab del jugador en {PlayerPrefabPath}. " +
                             "Ejecuta Designer Playground/Jugador/Generar assets del jugador.");
        }

        CreateCamera(player != null ? player.transform : null);

        EditorSceneManager.SaveScene(scene, scenePath);
        FocusTerrainPainting(terrain, palette);
        Debug.Log($"[Nivel] Nivel creado en {scenePath}. Pinta el terreno con la paleta '{PaletteName}' y dale a Play.");
        return scene;
    }

    // Crea la paleta si falta y vuelve a colocar todos los tiles del terreno en ella.
    public static GameObject BuildTerrainPalette()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) == null)
        {
            EnsureFolder(PalettesFolder);
            GridPaletteUtility.CreateNewPalette(PalettesFolder, PaletteName, GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Automatic, Vector3.one, GridLayout.CellSwizzle.XYZ);
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(PalettePath);
        Tilemap tilemap = contents.GetComponentInChildren<Tilemap>();
        tilemap.ClearAllTiles();
        foreach (var (tileName, cell) in PaletteLayout)
        {
            tilemap.SetTile(cell, LoadTile(tileName));
        }
        PrefabUtility.SaveAsPrefabAsset(contents, PalettePath);
        PrefabUtility.UnloadPrefabContents(contents);

        return AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
    }

    private static Tilemap CreateTilemap(Grid grid, string name, string sortingLayer, int sortingOrder, string physicsLayer)
    {
        var tilemapObject = new GameObject(name);
        tilemapObject.transform.SetParent(grid.transform, false);
        if (physicsLayer != null) tilemapObject.layer = LayerMask.NameToLayer(physicsLayer);

        var tilemap = tilemapObject.AddComponent<Tilemap>();
        var tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingLayerName = sortingLayer;
        tilemapRenderer.sortingOrder = sortingOrder;
        return tilemap;
    }

    private static void AddCollision(Tilemap tilemap, bool oneWay)
    {
        GameObject tilemapObject = tilemap.gameObject;
        var body = tilemapObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Static;
        var tilemapCollider = tilemapObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        var composite = tilemapObject.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

        if (oneWay)
        {
            composite.usedByEffector = true;
            var effector = tilemapObject.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 160f;
        }

        tilemapCollider.ProcessTilemapChanges();
        composite.GenerateGeometry();
    }

    // Suelo de una pantalla de ancho y una plataforma atravesable, para poder dar a Play nada más crear el nivel.
    private static void PaintStarterGround(Tilemap terrain, Tilemap oneWay)
    {
        const int left = -6;
        const int right = 17;
        string[] rows = { "T", "M", "B" };
        for (int row = 0; row < rows.Length; row++)
        {
            for (int x = left; x <= right; x++)
            {
                string column = x == left ? "L" : x == right ? "R" : "C";
                terrain.SetTile(new Vector3Int(x, -1 - row, 0), LoadTile($"Grass_{rows[row]}{column}"));
            }
        }

        oneWay.SetTile(new Vector3Int(5, 2, 0), LoadTile("OneWayWood_L"));
        oneWay.SetTile(new Vector3Int(6, 2, 0), LoadTile("OneWayWood_C"));
        oneWay.SetTile(new Vector3Int(7, 2, 0), LoadTile("OneWayWood_R"));
    }

    // La cámara va como hija del jugador hasta que exista el sistema de cámara (fase 6). El fondo la acompaña.
    private static void CreateCamera(Transform player)
    {
        var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
        if (player != null) cameraObject.transform.SetParent(player, false);
        cameraObject.transform.localPosition = new Vector3(0f, 2f, -10f);

        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        cameraObject.AddComponent<AudioListener>();
        camera.GetUniversalAdditionalCameraData();
        PixelPerfectCameraSetup.Configure(camera);

        var background = new GameObject("Fondo");
        background.transform.SetParent(cameraObject.transform, false);
        background.transform.localPosition = new Vector3(0f, 0f, 20f);
        var spriteRenderer = background.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.size = new Vector2(32f, 20f);
        spriteRenderer.sortingLayerName = GameLayers.Sorting.Background;
    }

    // Deja la Tile Palette abierta, con la paleta de terreno y el Tilemap "Terreno" como destino.
    private static void FocusTerrainPainting(Tilemap terrain, GameObject palette)
    {
        EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
        try
        {
            GridPaintingState.palette = palette;
        }
        catch (ArgumentException)
        {
            // La lista de paletas aún no incluye la recién creada: el alumno la elige en el desplegable.
        }
        GridPaintingState.scenePaintTarget = terrain.gameObject;
        Selection.activeGameObject = terrain.gameObject;
    }

    private static TileBase LoadTile(string tileName)
    {
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TilesFolder}/{tileName}.asset");
        if (tile == null) Debug.LogWarning($"[Nivel] Falta el tile {TilesFolder}/{tileName}.asset.");
        return tile;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }
}
