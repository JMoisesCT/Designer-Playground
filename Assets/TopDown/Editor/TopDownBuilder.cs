using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TopDown.Editor
{
    // Genera todo el kit top-down (importación del arte, animaciones, tiles, paleta, prefabs) y crea niveles listos para pintar.
    public static class TopDownBuilder
    {
        private const string Root = "Assets/TopDown";
        private const string AnimationsFolder = Root + "/Art/Animations";
        private const string TilesFolder = Root + "/Art/Tiles";
        private const string PaletteFolder = Root + "/Art/Palettes";
        private const string PaletteName = "TopDown";
        private const string PalettePath = PaletteFolder + "/" + PaletteName + ".prefab";
        private const string PrefabsFolder = Root + "/Prefabs";
        private const string PhysicsMaterialPath = Root + "/Art/SinFriccion.physicsMaterial2D";
        private const string ScenesFolder = Root + "/Scenes";
        public const string ExampleScenePath = ScenesFolder + "/Nivel_01.unity";

        private const string Art = "Assets/ThirdParty/NinjaAdventure";
        private const string PlayerArt = Art + "/Actor/Character/NinjaGreen/SeparateAnim";
        private const string FloorSheet = Art + "/Backgrounds/Tilesets/TilesetFloor.png";
        private const string NatureSheet = Art + "/Backgrounds/Tilesets/TilesetNature.png";
        private const string DungeonSheet = Art + "/Backgrounds/Tilesets/TilesetDungeon.png";
        private const string FlagOffSheet = Art + "/Backgrounds/Animated/Flag/FlagGray16x16.png";
        private const string FlagOnSheet = Art + "/Backgrounds/Animated/Flag/FlagRed16x16.png";
        private const string CoinSheet = Art + "/Items/Treasure/Coin2.png";
        private const string ChestSheet = Art + "/Items/Treasure/BigTreasureChest.png";
        private const string HeartSheet = Art + "/Ui/Receptacle/Heart.png";
        private const string MusicFolder = Art + "/Audio/Musics";
        private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";

        private const string PlayerPrefabPath = PrefabsFolder + "/Jugador.prefab";
        private const string CoinPrefabPath = PrefabsFolder + "/Moneda.prefab";
        private const string CheckpointPrefabPath = PrefabsFolder + "/Bandera.prefab";
        private const string GoalPrefabPath = PrefabsFolder + "/Cofre.prefab";
        private const string HudPrefabPath = PrefabsFolder + "/Interfaz.prefab";

        // Sonidos y música de Ninja Adventure (el alumno puede cambiarlos en el Inspector).
        private const string Sfx = Art + "/Audio/Sounds";
        private const string CoinSound = Sfx + "/Bonus/Coin.wav";
        private const string CheckpointSound = Sfx + "/Bonus/Bonus.wav";
        private const string HurtSound = Sfx + "/Hit & Impact/Hit1.wav";
        private const string DeathSound = Art + "/Audio/Jingles/GameOver.wav";
        private const string GoalSound = Art + "/Audio/Jingles/Success1.wav";
        private const string MusicPath = MusicFolder + "/1 - Adventure Begin.ogg";

        private const string FloorTilemapName = "Suelo";
        private const string DecorTilemapName = "Decorado";

        // Hojas que se cortan en celdas iguales. Cada trozo se llama "<archivo>_<n>", contando desde arriba a la izquierda.
        private static readonly (string Path, int Width, int Height)[] SpriteSheets =
        {
            (PlayerArt + "/Idle.png", 16, 16),
            (PlayerArt + "/Walk.png", 16, 16),
            (PlayerArt + "/Dead.png", 16, 16),
            (FloorSheet, 16, 16),
            (NatureSheet, 16, 16),
            (DungeonSheet, 16, 16),
            (FlagOffSheet, 16, 16),
            (FlagOnSheet, 16, 16),
            (CoinSheet, 10, 10),
            (ChestSheet, 16, 14),
            (HeartSheet, 16, 16),
        };

        // Piezas de TilesetNature (columna, fila) que van en el Tilemap "Decorado".
        // Los de 2×2 tiles se pintan seleccionando el bloque entero en la paleta.
        private static readonly (string Name, int Column, int Row, int Size, bool Solid)[] DecorTiles =
        {
            ("Arbol", 16, 0, 2, true),
            ("Pino", 2, 0, 2, true),
            ("Tocon", 0, 8, 2, true),
            ("Roca", 13, 8, 2, true),
            ("RocaGris", 16, 8, 2, true),
            ("Arbusto", 10, 9, 1, true),
            ("RocaPequena", 15, 9, 1, true),
            ("RocaGrisPequena", 18, 9, 1, true),
            ("ToconPequeno", 4, 8, 1, true),
            ("Girasol", 0, 11, 1, false),
            ("FlorRoja", 3, 11, 1, false),
            ("FlorBlanca", 6, 11, 1, false),
            ("Hierba", 3, 10, 1, false),
            ("Hierba2", 4, 10, 1, false),
        };

        private const string GrassTileName = "Pasto";
        private const string PathTileName = "Camino";
        private const string SpikesTileName = "Pinchos";

        [MenuItem("TopDown/Crear nivel nuevo", priority = 0)]
        private static void CreateLevelFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolder(ScenesFolder);
            string path = EditorUtility.SaveFilePanelInProject(
                "Crear nivel nuevo", "Nivel_02", "unity", "Elige el nombre del nivel.", ScenesFolder);
            if (string.IsNullOrEmpty(path)) return;

            CreateLevel(path, exampleLayout: false);
        }

        [MenuItem("TopDown/Avanzado/Regenerar assets del kit", priority = 100)]
        public static void BuildAllAssets()
        {
            ImportArt();
            BuildAnimations();
            BuildTiles();
            BuildPalette();
            BuildPrefabs();
            Debug.Log("[TopDown] Assets del kit generados.");
        }

        [MenuItem("TopDown/Avanzado/Regenerar nivel de ejemplo", priority = 101)]
        private static void BuildExampleLevelFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildAllAssets();
            CreateLevel(ExampleScenePath, exampleLayout: true);
        }

        // ---------- Importación del arte ----------

        // Pixel art nítido (16 píxeles = 1 tile, sin filtro ni compresión) y hojas cortadas en trozos.
        public static void ImportArt()
        {
            foreach (var (path, width, height) in SpriteSheets)
            {
                ImportSpriteSheet(path, width, height);
            }

            // La música se lee del disco mientras suena: no ocupa memoria descomprimida.
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { MusicFolder }))
            {
                var audio = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                AudioImporterSampleSettings settings = audio.defaultSampleSettings;
                if (settings.loadType == AudioClipLoadType.Streaming) continue;
                settings.loadType = AudioClipLoadType.Streaming;
                audio.defaultSampleSettings = settings;
                audio.SaveAndReimport();
            }
        }

        private static void ImportSpriteSheet(string path, int cellWidth, int cellHeight)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            // Se conserva el ID de cada trozo que ya existía: así los tiles y animaciones generados antes siguen funcionando.
            Dictionary<string, GUID> oldIds = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);

            var pixels = new Texture2D(2, 2);
            pixels.LoadImage(File.ReadAllBytes(path));
            string fileName = Path.GetFileNameWithoutExtension(path);
            int columns = pixels.width / cellWidth;
            int rows = pixels.height / cellHeight;

            var rects = new List<SpriteRect>();
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    // Las filas se cuentan desde arriba; en Unity la y de un trozo se mide desde abajo.
                    var rect = new Rect(column * cellWidth, pixels.height - (row + 1) * cellHeight, cellWidth, cellHeight);
                    if (IsEmpty(pixels, rect)) continue;

                    string spriteName = $"{fileName}_{row * columns + column}";
                    rects.Add(new SpriteRect
                    {
                        name = spriteName,
                        rect = rect,
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                        spriteID = oldIds.TryGetValue(spriteName, out GUID id) ? id : GUID.Generate(),
                    });
                }
            }
            Object.DestroyImmediate(pixels);

            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
                .SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static bool IsEmpty(Texture2D texture, Rect rect)
        {
            Color[] block = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            return block.All(color => color.a == 0f);
        }

        // ---------- Animaciones ----------

        public static void BuildAnimations()
        {
            // Hojas del personaje: una columna por dirección (abajo, arriba, izquierda, derecha); en Walk, una fila por frame.
            string[] directions = { "Down", "Up", "Left", "Right" };
            var playerClips = new List<AnimationClip>();
            for (int i = 0; i < directions.Length; i++)
            {
                playerClips.Add(BuildClip("Jugador", "Idle" + directions[i], 1, false, Frames(PlayerArt + "/Idle.png", i)));
            }
            for (int i = 0; i < directions.Length; i++)
            {
                playerClips.Add(BuildClip("Jugador", "Walk" + directions[i], 8, true,
                    Frames(PlayerArt + "/Walk.png", i, i + 4, i + 8, i + 12)));
            }
            playerClips.Add(BuildClip("Jugador", "Dead", 1, false, Frames(PlayerArt + "/Dead.png", 0)));
            BuildController("Jugador", playerClips.ToArray());

            BuildController("Bandera",
                BuildClip("Bandera", "Off", 8, true, Frames(FlagOffSheet, 0, 1, 2, 3)),
                BuildClip("Bandera", "On", 8, true, Frames(FlagOnSheet, 0, 1, 2, 3)));

            BuildController("Moneda", BuildClip("Moneda", "Spin", 8, true, Frames(CoinSheet, 0, 1, 2, 3)));

            AssetDatabase.SaveAssets();
        }

        private static AnimationClip BuildClip(string character, string clipName, int frameRate, bool loop, Sprite[] frames)
        {
            string folder = $"{AnimationsFolder}/{character}";
            EnsureFolder(folder);
            string path = $"{folder}/{clipName}.anim";

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = (float)i / frameRate, value = frames[i] };
            }
            // Clave extra al final para que el último frame dure lo mismo que los demás.
            keys[frames.Length] = new ObjectReferenceKeyframe { time = (float)frames.Length / frameRate, value = frames[^1] };

            clip.frameRate = frameRate;
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        // Un estado por clip, sin transiciones: los scripts eligen la animación con Animator.Play.
        private static void BuildController(string character, params AnimationClip[] clips)
        {
            string path = $"{AnimationsFolder}/{character}/{character}.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path)
                             ?? AnimatorController.CreateAnimatorControllerAtPath(path);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState child in stateMachine.states)
            {
                stateMachine.RemoveState(child.state);
            }

            foreach (AnimationClip clip in clips)
            {
                AnimatorState state = stateMachine.AddState(clip.name);
                state.motion = clip;
                state.writeDefaultValues = false;
            }
            stateMachine.defaultState = stateMachine.states[0].state;
            EditorUtility.SetDirty(controller);
        }

        // ---------- Tiles y paleta ----------

        public static void BuildTiles()
        {
            EnsureFolder(TilesFolder);
            BuildGrassTile();
            BuildPathTile();

            var spikes = GetOrCreateAsset<HazardTile>($"{TilesFolder}/{SpikesTileName}.asset");
            spikes.sprite = Piece(DungeonSheet, 4, 1);
            spikes.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(spikes);

            foreach (var (tileName, column, row, size, solid) in DecorTiles)
            {
                for (int dy = 0; dy < size; dy++)
                {
                    for (int dx = 0; dx < size; dx++)
                    {
                        var tile = GetOrCreateAsset<Tile>($"{TilesFolder}/{DecorTileName(tileName, size, dx, dy)}.asset");
                        tile.sprite = Piece(NatureSheet, column + dx, row + dy);
                        tile.colliderType = solid ? Tile.ColliderType.Grid : Tile.ColliderType.None;
                        EditorUtility.SetDirty(tile);
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }

        // Los bloques de 2×2 se guardan como 4 tiles: Arbol_0 (arriba izquierda) ... Arbol_3 (abajo derecha).
        private static string DecorTileName(string tileName, int size, int dx, int dy) =>
            size == 1 ? tileName : $"{tileName}_{dy * size + dx}";

        // Pasto liso casi siempre y, de vez en cuando, con alguna hierba o florecilla.
        private static void BuildGrassTile()
        {
            Sprite plain = Piece(FloorSheet, 0, 12);
            var tile = GetOrCreateAsset<RuleTile>($"{TilesFolder}/{GrassTileName}.asset");
            tile.m_DefaultSprite = plain;
            tile.m_DefaultColliderType = Tile.ColliderType.None;
            tile.m_TilingRules = new List<RuleTile.TilingRule>
            {
                new()
                {
                    m_Output = RuleTile.TilingRuleOutput.OutputSprite.Random,
                    m_ColliderType = Tile.ColliderType.None,
                    m_PerlinScale = 0.9f,
                    m_Sprites = new[]
                    {
                        plain, plain, plain, plain, plain, plain, plain, plain,
                        Piece(FloorSheet, 3, 11), Piece(FloorSheet, 2, 11), Piece(FloorSheet, 2, 12),
                    },
                },
            };
            EditorUtility.SetDirty(tile);
        }

        // Camino de tierra sobre el pasto: elige borde, esquina, tira o centro según los caminos vecinos.
        private static void BuildPathTile()
        {
            const int This = RuleTile.TilingRuleOutput.Neighbor.This;
            const int Not = RuleTile.TilingRuleOutput.Neighbor.NotThis;

            Sprite Floor(int column, int row) => Piece(FloorSheet, column, row);

            var tile = GetOrCreateAsset<RuleTile>($"{TilesFolder}/{PathTileName}.asset");
            tile.m_DefaultSprite = Floor(3, 10); // camino de un solo tile
            tile.m_DefaultColliderType = Tile.ColliderType.None;
            tile.m_TilingRules = new List<RuleTile.TilingRule>
            {
                // (arriba, abajo, izquierda, derecha). La primera regla que encaja gana.
                // Esquinas interiores: rodeado de camino salvo en una diagonal.
                PathRule(Floor(5, 9), This, This, This, This, (Vector3Int.up + Vector3Int.right, Not)),
                PathRule(Floor(6, 9), This, This, This, This, (Vector3Int.up + Vector3Int.left, Not)),
                PathRule(Floor(5, 8), This, This, This, This, (Vector3Int.down + Vector3Int.right, Not)),
                PathRule(Floor(6, 8), This, This, This, This, (Vector3Int.down + Vector3Int.left, Not)),
                PathRule(Floor(1, 8), This, This, This, This),
                // Bordes
                PathRule(Floor(1, 7), Not, This, This, This),
                PathRule(Floor(1, 9), This, Not, This, This),
                PathRule(Floor(0, 8), This, This, Not, This),
                PathRule(Floor(2, 8), This, This, This, Not),
                // Esquinas
                PathRule(Floor(0, 7), Not, This, Not, This),
                PathRule(Floor(2, 7), Not, This, This, Not),
                PathRule(Floor(0, 9), This, Not, Not, This),
                PathRule(Floor(2, 9), This, Not, This, Not),
                // Caminos de un tile de ancho
                PathRule(Floor(3, 8), This, This, Not, Not),
                PathRule(Floor(1, 10), Not, Not, This, This),
                PathRule(Floor(3, 7), Not, This, Not, Not),
                PathRule(Floor(3, 9), This, Not, Not, Not),
                PathRule(Floor(0, 10), Not, Not, Not, This),
                PathRule(Floor(2, 10), Not, Not, This, Not),
            };
            EditorUtility.SetDirty(tile);
        }

        private static RuleTile.TilingRule PathRule(Sprite sprite, int up, int down, int left, int right,
            (Vector3Int Position, int Neighbor)? diagonal = null)
        {
            var rule = new RuleTile.TilingRule { m_Sprites = new[] { sprite }, m_ColliderType = Tile.ColliderType.None };
            var neighbors = new Dictionary<Vector3Int, int>
            {
                [Vector3Int.up] = up,
                [Vector3Int.down] = down,
                [Vector3Int.left] = left,
                [Vector3Int.right] = right,
            };
            if (diagonal.HasValue) neighbors[diagonal.Value.Position] = diagonal.Value.Neighbor;
            rule.ApplyNeighbors(neighbors);
            return rule;
        }

        // Fila de arriba: suelo (Tilemap "Suelo"). Debajo: árboles, rocas y flores (Tilemap "Decorado").
        public static void BuildPalette()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) == null)
            {
                EnsureFolder(PaletteFolder);
                GridPaletteUtility.CreateNewPalette(PaletteFolder, PaletteName, GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic, Vector3.one, GridLayout.CellSwizzle.XYZ);
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(PalettePath);
            Tilemap tilemap = contents.GetComponentInChildren<Tilemap>();
            tilemap.ClearAllTiles();
            tilemap.SetTile(new Vector3Int(0, 0, 0), LoadTile(GrassTileName));
            tilemap.SetTile(new Vector3Int(1, 0, 0), LoadTile(PathTileName));
            tilemap.SetTile(new Vector3Int(2, 0, 0), LoadTile(SpikesTileName));

            int bigX = 0, smallX = 0, flatX = 0;
            foreach (var (tileName, _, _, size, solid) in DecorTiles)
            {
                if (size == 2)
                {
                    for (int dy = 0; dy < 2; dy++)
                    {
                        for (int dx = 0; dx < 2; dx++)
                        {
                            tilemap.SetTile(new Vector3Int(bigX + dx, -2 - dy, 0), LoadTile(DecorTileName(tileName, size, dx, dy)));
                        }
                    }
                    bigX += 3;
                }
                else if (solid) tilemap.SetTile(new Vector3Int(smallX++, -5, 0), LoadTile(tileName));
                else tilemap.SetTile(new Vector3Int(flatX++, -7, 0), LoadTile(tileName));
            }
            PrefabUtility.SaveAsPrefabAsset(contents, PalettePath);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        // ---------- Prefabs ----------

        public static void BuildPrefabs()
        {
            EnsureFolder(PrefabsFolder);

            // OnValidate avisa de referencias vacías al añadir cada componente, antes de que se asignen: se silencia.
            Debug.unityLogger.logEnabled = false;
            try
            {
                BuildPlayerPrefab();
                BuildCoinPrefab();
                BuildCheckpointPrefab();
                BuildGoalPrefab();
                BuildHudPrefab();
            }
            finally
            {
                Debug.unityLogger.logEnabled = true;
            }
            AssetDatabase.SaveAssets();
        }

        private static void BuildPlayerPrefab()
        {
            var material = GetOrCreateAsset<PhysicsMaterial2D>(PhysicsMaterialPath);
            material.friction = 0f;
            material.bounciness = 0f;
            EditorUtility.SetDirty(material);

            var root = new GameObject("Jugador") { tag = "Player", layer = LayerMask.NameToLayer("Player") };
            SpriteRenderer sprite = AddSprite(root, "Jugador", "IdleDown", "Player", 10);
            Animator animator = AddAnimator(root, "Jugador");

            var body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Solo los pies chocan: así el personaje puede pasar justo por delante de un árbol o una roca.
            // Bordes redondeados y sin tocar el borde del tile: si no, se engancha en las esquinas al pasar rozando.
            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.5f, 0.25f);
            box.edgeRadius = 0.0625f;
            box.offset = new Vector2(0f, -0.25f);
            box.sharedMaterial = material;

            InputActionReference[] actions = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath).OfType<InputActionReference>().ToArray();
            var controller = root.AddComponent<PlayerController>();
            SetFields(controller, ("_body", body), ("_animator", animator), ("_moveAction", FindAction(actions, "Move")));

            var health = root.AddComponent<PlayerHealth>();
            SetFields(health, ("_controller", controller), ("_collider", box), ("_sprite", sprite), ("_animator", animator),
                ("_hurtSound", LoadSound(HurtSound)), ("_deathSound", LoadSound(DeathSound)));

            SavePrefab(root, PlayerPrefabPath);
        }

        private static void BuildCoinPrefab()
        {
            var root = new GameObject("Moneda");
            AddSprite(root, "Moneda", "Spin", "Traps", 5);
            AddAnimator(root, "Moneda");

            var circle = root.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.35f;

            SetFields(root.AddComponent<Coin>(), ("_sound", LoadSound(CoinSound)));
            SavePrefab(root, CoinPrefabPath);
        }

        private static void BuildCheckpointPrefab()
        {
            var root = new GameObject("Bandera");
            AddSprite(root, "Bandera", "Off", "Traps", 0);
            Animator animator = AddAnimator(root, "Bandera");

            var box = root.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(0.8f, 0.8f);

            SetFields(root.AddComponent<Checkpoint>(), ("_animator", animator), ("_sound", LoadSound(CheckpointSound)));
            SavePrefab(root, CheckpointPrefabPath);
        }

        // Cofre sólido de 1 tile: al chocar con él se abre y termina el nivel.
        private static void BuildGoalPrefab()
        {
            var root = new GameObject("Cofre");
            var sprite = root.AddComponent<SpriteRenderer>();
            sprite.sprite = Piece(ChestSheet, 0, 0);
            sprite.sortingLayerName = "Traps";

            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 0.875f);

            SetFields(root.AddComponent<Goal>(),
                ("_sprite", sprite), ("_openSprite", Piece(ChestSheet, 1, 0)), ("_sound", LoadSound(GoalSound)));
            SavePrefab(root, GoalPrefabPath);
        }

        // Canvas con los corazones, el contador de monedas y el mensaje central. Medidas en píxeles del juego (384×216).
        private static void BuildHudPrefab()
        {
            var root = new GameObject("Interfaz");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(384f, 216f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform hearts = AddRect(root.transform, "Corazones", new Vector2(4f, -4f), new Vector2(200f, 16f));
            var layout = hearts.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 1f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            Sprite[] heartFrames = LoadSheet(HeartSheet);
            var heart = new GameObject("Corazon", typeof(RectTransform)).AddComponent<Image>();
            heart.rectTransform.SetParent(hearts, false);
            heart.rectTransform.sizeDelta = new Vector2(16f, 16f);
            heart.sprite = heartFrames[^1];
            heart.raycastTarget = false;

            RectTransform counter = AddRect(root.transform, "Monedas", new Vector2(5f, -22f), new Vector2(80f, 12f));
            var icon = new GameObject("Icono", typeof(RectTransform)).AddComponent<Image>();
            icon.rectTransform.SetParent(counter, false);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = icon.rectTransform.pivot = new Vector2(0f, 0.5f);
            icon.rectTransform.sizeDelta = new Vector2(10f, 10f);
            icon.sprite = Piece(CoinSheet, 0, 0);
            icon.raycastTarget = false;

            Text coinText = AddText(counter, "Contador", 10, TextAnchor.MiddleLeft);
            coinText.rectTransform.anchorMin = Vector2.zero;
            coinText.rectTransform.anchorMax = Vector2.one;
            coinText.rectTransform.offsetMin = new Vector2(14f, 0f);
            coinText.rectTransform.offsetMax = Vector2.zero;

            Text messageText = AddText(root.transform, "Mensaje", 20, TextAnchor.MiddleCenter);
            messageText.rectTransform.anchorMin = Vector2.zero;
            messageText.rectTransform.anchorMax = Vector2.one;
            messageText.rectTransform.offsetMin = messageText.rectTransform.offsetMax = Vector2.zero;

            SetFields(root.AddComponent<LevelHud>(),
                ("_heart", heart), ("_fullHeart", heartFrames[^1]), ("_emptyHeart", heartFrames[0]),
                ("_coinCounter", counter.gameObject), ("_coinText", coinText), ("_messageText", messageText));
            SavePrefab(root, HudPrefabPath);
        }

        // Rectángulo anclado arriba a la izquierda.
        private static RectTransform AddRect(Transform parent, string objectName, Vector2 position, Vector2 size)
        {
            var rect = (RectTransform)new GameObject(objectName, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Text AddText(Transform parent, string objectName, int fontSize, TextAnchor alignment)
        {
            var text = new GameObject(objectName, typeof(RectTransform)).AddComponent<Text>();
            text.rectTransform.SetParent(parent, false);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            text.raycastTarget = false;
            text.gameObject.AddComponent<Outline>().effectColor = Color.black;
            return text;
        }

        private static SpriteRenderer AddSprite(GameObject root, string character, string firstClip, string sortingLayer, int order)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsFolder}/{character}/{firstClip}.anim");
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var sprite = root.AddComponent<SpriteRenderer>();
            sprite.sprite = (Sprite)AnimationUtility.GetObjectReferenceCurve(clip, binding)[0].value;
            sprite.sortingLayerName = sortingLayer;
            sprite.sortingOrder = order;
            return sprite;
        }

        private static Animator AddAnimator(GameObject root, string character)
        {
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<AnimatorController>($"{AnimationsFolder}/{character}/{character}.controller");
            return animator;
        }

        private static InputActionReference FindAction(InputActionReference[] actions, string actionName)
        {
            InputActionReference reference = actions.FirstOrDefault(
                r => r.action != null && r.action.name == actionName && r.action.actionMap.name == "Player");
            if (reference == null) Debug.LogWarning($"[TopDown] No se encontró la acción Player/{actionName} en {InputActionsPath}.");
            return reference;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        // ---------- Niveles ----------

        public static void CreateLevel(string scenePath, bool exampleLayout)
        {
            EnsureFolder(Path.GetDirectoryName(scenePath).Replace('\\', '/'));
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            LightAllSortingLayers(light);

            var grid = new GameObject("Grid").AddComponent<Grid>();
            Tilemap floor = AddTilemap(grid, FloorTilemapName, "Default", 0);
            Tilemap decor = AddTilemap(grid, DecorTilemapName, "Ground", 1);

            // Se pinta antes de añadir los colliders: si no, la escena se guarda con el collider vacío.
            Vector2 start = exampleLayout ? PaintExampleLevel(floor, decor) : PaintStarterLevel(floor, decor);
            AddCollision(decor);

            GameObject player = Spawn(PlayerPrefabPath, start);
            if (exampleLayout) SpawnExampleObjects();
            else Spawn(GoalPrefabPath, new Vector2(28.5f, 8.5f));

            CameraFollow cameraFollow = CreateCamera(player.transform.position);
            if (exampleLayout)
            {
                var serializedCamera = new SerializedObject(cameraFollow);
                serializedCamera.FindProperty("_useBounds").boolValue = true;
                serializedCamera.FindProperty("_boundsMin").vector2Value = Vector2.zero;
                serializedCamera.FindProperty("_boundsMax").vector2Value = new Vector2(48f, 30f);
                serializedCamera.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath));

            EditorSceneManager.SaveScene(scene, scenePath);
            FocusPainting(floor);
            Debug.Log($"[TopDown] Nivel creado en {scenePath}. Pinta con la paleta '{PaletteName}' y dale a Play.");
        }

        private static Tilemap AddTilemap(Grid grid, string tilemapName, string layer, int order)
        {
            var tilemapObject = new GameObject(tilemapName) { layer = LayerMask.NameToLayer(layer) };
            tilemapObject.transform.SetParent(grid.transform, false);
            var tilemap = tilemapObject.AddComponent<Tilemap>();
            var tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingLayerName = "Terrain";
            tilemapRenderer.sortingOrder = order;
            return tilemap;
        }

        // Prado rodeado de árboles con un camino hasta el cofre. Devuelve dónde empieza el jugador.
        private static Vector2 PaintStarterLevel(Tilemap floor, Tilemap decor)
        {
            Fill(floor, LoadTile(GrassTileName), 0, 31, 0, 17);
            Fill(floor, LoadTile(PathTileName), 2, 29, 8, 9);
            TreeBorder(decor, 32, 18);
            return new Vector2(3.5f, 8.5f);
        }

        // Nivel de ejemplo (48×30 tiles): camino con monedas, un claro escondido en el bosque y un pasillo de pinchos en zigzag.
        private static Vector2 PaintExampleLevel(Tilemap floor, Tilemap decor)
        {
            TileBase path = LoadTile(PathTileName);
            TileBase spikes = LoadTile(SpikesTileName);

            Fill(floor, LoadTile(GrassTileName), 0, 47, 0, 29);
            Fill(floor, path, 2, 21, 4, 5);    // salida hacia la derecha
            Fill(floor, path, 20, 21, 4, 21);  // sube
            Fill(floor, path, 22, 25, 20, 21); // hasta el pasillo de pinchos
            Fill(floor, path, 36, 45, 20, 21); // del pasillo al cofre
            TreeBorder(decor, 48, 30);

            // Bosque con un claro escondido; se entra por un hueco del lado derecho.
            for (int x = 4; x <= 16; x += 2)
            {
                for (int y = 8; y <= 16; y += 2)
                {
                    bool clearing = x >= 8 && x <= 12 && y >= 10 && y <= 14;
                    bool entrance = x >= 14 && y == 12;
                    if (!clearing && !entrance) Paint2x2(decor, (x + y) % 4 == 0 ? "Pino" : "Arbol", x, y);
                }
            }

            // Pasillo entre dos filas de árboles con columnas de pinchos alternas: hay que ir en zigzag.
            // Los árboles cierran también el paso por arriba y por abajo: al cofre solo se llega cruzando el pasillo.
            for (int x = 24; x <= 44; x += 2)
            {
                Paint2x2(decor, "Arbol", x, 16);
            }
            for (int x = 24; x <= 38; x += 2)
            {
                Paint2x2(decor, "Arbol", x, 24);
            }
            Paint2x2(decor, "Arbol", 22, 24);
            Paint2x2(decor, "Arbol", 22, 26);
            Fill(floor, spikes, 26, 27, 18, 21);
            Fill(floor, spikes, 30, 31, 20, 23);
            Fill(floor, spikes, 34, 35, 18, 21);

            // Zona abierta de abajo a la derecha con rocas y árboles sueltos.
            Paint2x2(decor, "Roca", 9, 2);
            Paint2x2(decor, "Arbol", 28, 6);
            Paint2x2(decor, "Pino", 33, 10);
            Paint2x2(decor, "RocaGris", 37, 4);
            Paint2x2(decor, "Tocon", 41, 11);
            Paint2x2(decor, "Roca", 26, 12);
            foreach (var (x, y) in new[] { (39, 7), (40, 7), (41, 7), (41, 6), (41, 5) })
            {
                decor.SetTile(new Vector3Int(x, y, 0), LoadTile("RocaGrisPequena"));
            }
            decor.SetTile(new Vector3Int(15, 6, 0), LoadTile("Arbusto"));
            decor.SetTile(new Vector3Int(23, 9, 0), LoadTile("Arbusto"));
            decor.SetTile(new Vector3Int(18, 3, 0), LoadTile("RocaPequena"));
            decor.SetTile(new Vector3Int(43, 25, 0), LoadTile("ToconPequeno"));

            // Flores y hierba (no chocan).
            string[] flowers = { "Girasol", "FlorRoja", "FlorBlanca", "Hierba", "Hierba2" };
            var cells = new[] { (3, 7), (5, 2), (12, 7), (17, 2), (23, 6), (24, 14), (30, 3), (35, 14), (39, 13), (44, 3), (44, 8), (42, 26), (34, 26), (18, 18), (3, 20), (6, 25), (14, 22) };
            for (int i = 0; i < cells.Length; i++)
            {
                decor.SetTile(new Vector3Int(cells[i].Item1, cells[i].Item2, 0), LoadTile(flowers[i % flowers.Length]));
            }
            return new Vector2(3.5f, 4.5f);
        }

        private static void SpawnExampleObjects()
        {
            foreach (Vector2 position in new Vector2[]
                     {
                         new(8.5f, 4.5f), new(12.5f, 5.5f), new(16.5f, 4.5f),   // camino
                         new(10f, 13f), new(9f, 12f), new(11f, 12f),             // claro escondido
                         new(28.5f, 22.5f), new(32.5f, 18.5f), new(36.5f, 22.5f), // pasillo de pinchos
                         new(40.5f, 5.5f),                                       // tras las rocas
                     })
            {
                Spawn(CoinPrefabPath, position);
            }

            Spawn(CheckpointPrefabPath, new Vector2(22.5f, 19f)); // antes del pasillo de pinchos
            Spawn(GoalPrefabPath, new Vector2(44.5f, 22.5f));
        }

        // Fila de árboles de 2×2 alrededor de todo el nivel para que el jugador no se salga.
        private static void TreeBorder(Tilemap decor, int width, int height)
        {
            for (int x = 0; x < width; x += 2)
            {
                Paint2x2(decor, "Arbol", x, 0);
                Paint2x2(decor, "Arbol", x, height - 2);
            }
            for (int y = 2; y < height - 2; y += 2)
            {
                Paint2x2(decor, "Arbol", 0, y);
                Paint2x2(decor, "Arbol", width - 2, y);
            }
        }

        // (x, y) es la celda de abajo a la izquierda del bloque.
        private static void Paint2x2(Tilemap tilemap, string tileName, int x, int y)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                for (int dx = 0; dx < 2; dx++)
                {
                    tilemap.SetTile(new Vector3Int(x + dx, y + 1 - dy, 0), LoadTile(DecorTileName(tileName, 2, dx, dy)));
                }
            }
        }

        private static GameObject Spawn(string prefabPath, Vector2 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
            return instance;
        }

        private static void Fill(Tilemap tilemap, TileBase tile, int xMin, int xMax, int yMin, int yMax)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        private static void AddCollision(Tilemap tilemap)
        {
            GameObject tilemapObject = tilemap.gameObject;
            var body = tilemapObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var tilemapCollider = tilemapObject.AddComponent<TilemapCollider2D>();
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            var composite = tilemapObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

            tilemapCollider.ProcessTilemapChanges();
            composite.GenerateGeometry();
        }

        // Cámara pixel perfect que sigue al jugador, con la música del nivel.
        private static CameraFollow CreateCamera(Vector3 playerPosition)
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(playerPosition.x, playerPosition.y, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 216f / 2f / 16f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.GetUniversalAdditionalCameraData();
            cameraObject.AddComponent<AudioListener>();

            var pixelPerfect = cameraObject.AddComponent<PixelPerfectCamera>();
            pixelPerfect.assetsPPU = 16;
            pixelPerfect.refResolutionX = 384;
            pixelPerfect.refResolutionY = 216;
            // Sin Pixel Snapping la cámara avanza en píxeles de pantalla: con zoom ×5 el mundo no se mueve a saltos.
            pixelPerfect.gridSnapping = PixelPerfectCamera.GridSnapping.None;
            pixelPerfect.cropFrame = PixelPerfectCamera.CropFrame.None;

            var cameraFollow = cameraObject.AddComponent<CameraFollow>();

            var music = new GameObject("Musica").AddComponent<AudioSource>();
            music.transform.SetParent(cameraObject.transform, false);
            music.clip = LoadSound(MusicPath);
            music.loop = true;
            music.volume = 0.4f;
            music.playOnAwake = true;
            return cameraFollow;
        }

        // Una luz 2D solo ilumina las Sorting Layers de su lista: sin esto los sprites se ven negros.
        private static void LightAllSortingLayers(Light2D light)
        {
            SortingLayer[] layers = SortingLayer.layers;
            var serializedLight = new SerializedObject(light);
            SerializedProperty targets = serializedLight.FindProperty("m_ApplyToSortingLayers");
            targets.arraySize = layers.Length;
            for (int i = 0; i < layers.Length; i++)
            {
                targets.GetArrayElementAtIndex(i).intValue = layers[i].id;
            }
            serializedLight.ApplyModifiedPropertiesWithoutUndo();
        }

        // Deja la Tile Palette abierta, con la paleta del kit y el Tilemap "Suelo" como destino.
        private static void FocusPainting(Tilemap floor)
        {
            EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
            try
            {
                GridPaintingState.palette = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
            }
            catch (ArgumentException)
            {
                // La lista de paletas aún no incluye la recién creada: el alumno la elige en el desplegable.
            }
            GridPaintingState.scenePaintTarget = floor.gameObject;
            Selection.activeGameObject = floor.gameObject;
        }

        // ---------- Utilidades ----------

        // Todos los trozos de una hoja, ordenados por su número.
        private static Sprite[] LoadSheet(string sheetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().OrderBy(FrameIndex).ToArray();
        }

        // Trozos concretos de una hoja, por su número.
        private static Sprite[] Frames(string sheetPath, params int[] indices)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
            return indices.Select(index => sprites.First(s => FrameIndex(s) == index)).ToArray();
        }

        // Trozo de una hoja por su columna y fila (contando desde arriba a la izquierda).
        private static Sprite Piece(string sheetPath, int column, int row)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
            Sprite any = sprites[0];
            int columns = (int)(any.texture.width / any.rect.width);
            return sprites.First(s => FrameIndex(s) == row * columns + column);
        }

        // Los trozos se llaman "<archivo>_<n>".
        private static int FrameIndex(Sprite sprite)
        {
            int underscore = sprite.name.LastIndexOf('_');
            return underscore >= 0 && int.TryParse(sprite.name[(underscore + 1)..], out int index) ? index : 0;
        }

        private static AudioClip LoadSound(string path)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) Debug.LogWarning($"[TopDown] Falta el sonido {path}.");
            return clip;
        }

        private static TileBase LoadTile(string tileName)
        {
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TilesFolder}/{tileName}.asset");
            if (tile == null) Debug.LogWarning($"[TopDown] Falta el tile {TilesFolder}/{tileName}.asset.");
            return tile;
        }

        private static T GetOrCreateAsset<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = typeof(ScriptableObject).IsAssignableFrom(typeof(T))
                ? (T)(Object)ScriptableObject.CreateInstance(typeof(T))
                : (T)Activator.CreateInstance(typeof(T));
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetFields(Object target, params (string Name, Object Value)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach (var (fieldName, value) in fields)
            {
                serialized.FindProperty(fieldName).objectReferenceValue = value;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
