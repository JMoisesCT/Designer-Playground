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

namespace Shmup.Editor
{
    // Genera todo el kit de naves (importación del arte, animación, tiles, paleta, prefabs) y crea niveles listos para pintar.
    public static class ShmupBuilder
    {
        private const string Root = "Assets/Shmup";
        private const string AnimationsFolder = Root + "/Art/Animations";
        private const string TilesFolder = Root + "/Art/Tiles";
        private const string PaletteFolder = Root + "/Art/Palettes";
        private const string PaletteName = "Shmup";
        private const string PalettePath = PaletteFolder + "/" + PaletteName + ".prefab";
        private const string PrefabsFolder = Root + "/Prefabs";
        private const string ScenesFolder = Root + "/Scenes";
        public const string ExampleScenePath = ScenesFolder + "/Nivel_01.unity";

        private const string Art = "Assets/ThirdParty/Kenney/PixelShmup/Tilemap";
        private const string ShipSheet = Art + "/ships_packed.png";
        private const string TileSheet = Art + "/tiles_packed.png";
        private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";

        private const string PlayerPrefabPath = PrefabsFolder + "/Jugador.prefab";
        private const string PlayerBulletPrefabPath = PrefabsFolder + "/BalaJugador.prefab";
        private const string EnemyBulletPrefabPath = PrefabsFolder + "/BalaEnemiga.prefab";
        private const string ExplosionPrefabPath = PrefabsFolder + "/Explosion.prefab";
        private const string StraightEnemyPrefabPath = PrefabsFolder + "/EnemigoRecto.prefab";
        private const string WaveEnemyPrefabPath = PrefabsFolder + "/EnemigoOndulante.prefab";
        private const string ShooterEnemyPrefabPath = PrefabsFolder + "/EnemigoTirador.prefab";
        private const string TurretPrefabPath = PrefabsFolder + "/Torreta.prefab";
        private const string BossPrefabPath = PrefabsFolder + "/Jefe.prefab";
        private const string WeaponPickupPrefabPath = PrefabsFolder + "/Mejora.prefab";
        private const string HealthPickupPrefabPath = PrefabsFolder + "/Botiquin.prefab";
        private const string GoalPrefabPath = PrefabsFolder + "/Meta.prefab";
        private const string HudPrefabPath = PrefabsFolder + "/Interfaz.prefab";
        private const string GrenadePrefabPath = PrefabsFolder + "/Granada.prefab";
        private const string GrenadierPrefabPath = PrefabsFolder + "/EnemigoGranadero.prefab";
        private const string BoosterPrefabPath = PrefabsFolder + "/Potenciador.prefab";

        // Sonidos: Kenney Digital SFX y, para explosiones y música, Ninja Adventure (el alumno puede cambiarlos en el Inspector).
        private const string Sfx = "Assets/ThirdParty/Kenney/Audio_DigitalSFX/Audio";
        private const string NinjaAudio = "Assets/ThirdParty/NinjaAdventure/Audio";
        private const string PlayerShotSound = Sfx + "/laser4.ogg";
        private const string EnemyShotSound = Sfx + "/laser9.ogg";
        private const string HitSound = Sfx + "/zap1.ogg";
        private const string HurtSound = Sfx + "/lowThreeTone.ogg";
        private const string PickupSound = Sfx + "/powerUp3.ogg";
        private const string ExplosionSound = NinjaAudio + "/Sounds/Elemental/Explosion.wav";
        private const string BigExplosionSound = NinjaAudio + "/Sounds/Elemental/Explosion2.wav";
        private const string GoalSound = "Assets/ThirdParty/Kenney/Audio_MusicJingles/Audio/8-Bit jingles/jingles_NES00.ogg";
        private const string MusicPath = NinjaAudio + "/Musics/17 - Fight.ogg";

        private const string IslandTilemapName = "Islas";
        private const string DecorTilemapName = "Decorado";
        private const string GrassTileName = "Isla";
        private const string DesertTileName = "Desierto";

        // El agua es el color de fondo de la cámara: no hace falta pintarla.
        private static readonly Color32 WaterColor = new(223, 246, 245, 255);

        private const float ViewCenterY = 6.75f; // la pantalla enseña las filas 0 a 13 (216 píxeles = 13,5 tiles)
        private const int EnemyAngle = 90;       // los dibujos miran hacia arriba: girados, vuelan hacia la izquierda
        private const int PlayerAngle = -90;

        // Hojas que se cortan en celdas iguales. Cada trozo se llama "<archivo>_<n>", contando desde arriba a la izquierda.
        private static readonly (string Path, int Width, int Height)[] SpriteSheets =
        {
            (ShipSheet, 32, 32),
            (TileSheet, 16, 16),
        };

        // Piezas de tiles_packed (columna, fila) que van en el Tilemap "Decorado".
        private static readonly (string Name, int Column, int Row)[] GrassDecor =
        {
            ("Arbusto", 0, 3), ("Arbol", 0, 4), ("Bosque", 0, 5), ("Cabana", 0, 6), ("Casa", 0, 7), ("Bandera", 0, 8),
            ("Huerto", 1, 9), ("Claro", 3, 9),
        };

        private static readonly (string Name, int Column, int Row)[] DesertDecor =
        {
            ("ArbolDesierto", 6, 4), ("BosqueDesierto", 6, 5), ("CabanaAzul", 6, 6), ("CasaAzul", 6, 7), ("BanderaAzul", 6, 8),
            ("HuertoDesierto", 7, 9), ("ClaroDesierto", 9, 9),
        };

        private static readonly (string Name, int Column, int Row)[] WaterDecor =
        {
            ("Islote", 4, 5), ("Islotes", 5, 5), ("IsloteDesierto", 10, 5), ("IslotesDesierto", 11, 5),
        };

        [MenuItem("Shmup/Crear nivel nuevo", priority = 0)]
        private static void CreateLevelFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolder(ScenesFolder);
            string path = EditorUtility.SaveFilePanelInProject(
                "Crear nivel nuevo", "Nivel_02", "unity", "Elige el nombre del nivel.", ScenesFolder);
            if (string.IsNullOrEmpty(path)) return;

            CreateLevel(path, exampleLayout: false);
        }

        [MenuItem("Shmup/Avanzado/Regenerar assets del kit", priority = 100)]
        public static void BuildAllAssets()
        {
            ImportArt();
            BuildAnimations();
            BuildTiles();
            BuildPalette();
            BuildPrefabs();
            Debug.Log("[Shmup] Assets del kit generados.");
        }

        [MenuItem("Shmup/Avanzado/Regenerar nivel de ejemplo", priority = 101)]
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
            var audio = (AudioImporter)AssetImporter.GetAtPath(MusicPath);
            AudioImporterSampleSettings settings = audio.defaultSampleSettings;
            if (settings.loadType == AudioClipLoadType.Streaming) return;
            settings.loadType = AudioClipLoadType.Streaming;
            audio.defaultSampleSettings = settings;
            audio.SaveAndReimport();
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

        // La explosión es la única animación del kit: fogonazo, estallido y humo.
        public static void BuildAnimations()
        {
            string folder = $"{AnimationsFolder}/Explosion";
            EnsureFolder(folder);
            string clipPath = $"{folder}/Explosion.anim";

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            const int frameRate = 12;
            Sprite[] frames = { Piece(TileSheet, 4, 0), Piece(TileSheet, 5, 0), Piece(TileSheet, 6, 0), Piece(TileSheet, 7, 0), Piece(TileSheet, 8, 0) };
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = (float)i / frameRate, value = frames[i] };
            }
            // Clave extra al final para que el último frame dure lo mismo que los demás.
            keys[frames.Length] = new ObjectReferenceKeyframe { time = (float)frames.Length / frameRate, value = frames[^1] };

            clip.frameRate = frameRate;
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);

            string controllerPath = $"{folder}/Explosion.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)
                             ?? AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState child in stateMachine.states)
            {
                stateMachine.RemoveState(child.state);
            }
            AnimatorState state = stateMachine.AddState(clip.name);
            state.motion = clip;
            state.writeDefaultValues = false;
            stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);

            AssetDatabase.SaveAssets();
        }

        // ---------- Tiles y paleta ----------

        public static void BuildTiles()
        {
            EnsureFolder(TilesFolder);
            BuildIslandTile(GrassTileName, 1, 4);
            BuildIslandTile(DesertTileName, 7, 10);

            foreach (var (tileName, column, row) in GrassDecor.Concat(DesertDecor).Concat(WaterDecor))
            {
                var tile = GetOrCreateAsset<Tile>($"{TilesFolder}/{tileName}.asset");
                tile.sprite = Piece(TileSheet, column, row);
                tile.colliderType = Tile.ColliderType.None;
                EditorUtility.SetDirty(tile);
            }

            AssetDatabase.SaveAssets();
        }

        // Isla con orilla automática. blockColumn: bloque 3×3 de la hoja; cornerColumn: bloque 2×2 de esquinas interiores.
        private static void BuildIslandTile(string tileName, int blockColumn, int cornerColumn)
        {
            const int This = RuleTile.TilingRuleOutput.Neighbor.This;
            const int Not = RuleTile.TilingRuleOutput.Neighbor.NotThis;

            Sprite Block(int column, int row) => Piece(TileSheet, blockColumn + column, 3 + row);
            Sprite Corner(int column, int row) => Piece(TileSheet, cornerColumn + column, 3 + row);

            var tile = GetOrCreateAsset<RuleTile>($"{TilesFolder}/{tileName}.asset");
            tile.m_DefaultSprite = Piece(TileSheet, cornerColumn, 5); // isla de un solo tile
            tile.m_DefaultColliderType = Tile.ColliderType.None;
            tile.m_TilingRules = new List<RuleTile.TilingRule>
            {
                // (arriba, abajo, izquierda, derecha). La primera regla que encaja gana.
                // Esquinas interiores: rodeado de isla salvo en una diagonal.
                IslandRule(Corner(0, 0), This, This, This, This, (Vector3Int.down + Vector3Int.right, Not)),
                IslandRule(Corner(1, 0), This, This, This, This, (Vector3Int.down + Vector3Int.left, Not)),
                IslandRule(Corner(0, 1), This, This, This, This, (Vector3Int.up + Vector3Int.right, Not)),
                IslandRule(Corner(1, 1), This, This, This, This, (Vector3Int.up + Vector3Int.left, Not)),
                IslandRule(Block(1, 1), This, This, This, This),
                // Orillas
                IslandRule(Block(1, 0), Not, This, This, This),
                IslandRule(Block(1, 2), This, Not, This, This),
                IslandRule(Block(0, 1), This, This, Not, This),
                IslandRule(Block(2, 1), This, This, This, Not),
                // Esquinas
                IslandRule(Block(0, 0), Not, This, Not, This),
                IslandRule(Block(2, 0), Not, This, This, Not),
                IslandRule(Block(0, 2), This, Not, Not, This),
                IslandRule(Block(2, 2), This, Not, This, Not),
            };
            EditorUtility.SetDirty(tile);
        }

        private static RuleTile.TilingRule IslandRule(Sprite sprite, int up, int down, int left, int right,
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

        // Fila de arriba: islas (Tilemap "Islas"). Debajo: adornos de cada tipo de isla y del agua (Tilemap "Decorado").
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
            tilemap.SetTile(new Vector3Int(1, 0, 0), LoadTile(DesertTileName));

            PaletteRow(tilemap, GrassDecor, -2);
            PaletteRow(tilemap, DesertDecor, -3);
            PaletteRow(tilemap, WaterDecor, -4);

            PrefabUtility.SaveAsPrefabAsset(contents, PalettePath);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void PaletteRow(Tilemap tilemap, (string Name, int Column, int Row)[] tiles, int y)
        {
            for (int x = 0; x < tiles.Length; x++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), LoadTile(tiles[x].Name));
            }
        }

        // ---------- Prefabs ----------

        public static void BuildPrefabs()
        {
            EnsureFolder(PrefabsFolder);

            // OnValidate avisa de referencias vacías al añadir cada componente, antes de que se asignen: se silencia.
            Debug.unityLogger.logEnabled = false;
            try
            {
                // Primero lo que los demás prefabs necesitan como referencia.
                BuildBulletPrefab(PlayerBulletPrefabPath, Piece(TileSheet, 0, 0), "Player", true, 18f);
                BuildBulletPrefab(EnemyBulletPrefabPath, Piece(TileSheet, 6, 1), "Hazard", false, 7f);
                BuildExplosionPrefab();
                BuildGrenadePrefab();
                BuildPlayerPrefab();
                BuildEnemyPrefabs();
                BuildPickupPrefab(WeaponPickupPrefabPath, Piece(TileSheet, 1, 2), Pickup.PickupKind.Weapon);
                BuildPickupPrefab(HealthPickupPrefabPath, Piece(TileSheet, 0, 2), Pickup.PickupKind.Health);
                BuildGoalPrefab();
                BuildHudPrefab();
            }
            finally
            {
                Debug.unityLogger.logEnabled = true;
            }
            AssetDatabase.SaveAssets();
        }

        // Todo lo que choca usa triggers con cuerpo cinemático: los scripts mueven los objetos y deciden qué pasa al tocarse.
        private static void AddTriggerBody(GameObject root, float radius, out CircleCollider2D circle)
        {
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            // Dormido no recibe OnTriggerStay2D.
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;

            circle = root.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = radius;
        }

        private static SpriteRenderer AddSprite(GameObject root, Sprite art, string sortingLayer, int order)
        {
            var sprite = root.AddComponent<SpriteRenderer>();
            sprite.sprite = art;
            sprite.sortingLayerName = sortingLayer;
            sprite.sortingOrder = order;
            return sprite;
        }

        private static void BuildBulletPrefab(string path, Sprite art, string layer, bool fromPlayer, float speed)
        {
            var root = new GameObject(Path.GetFileNameWithoutExtension(path)) { layer = LayerMask.NameToLayer(layer) };
            AddSprite(root, art, "Player", 5);
            AddTriggerBody(root, 0.25f, out _);
            SetValues(root.AddComponent<Bullet>(), ("_fromPlayer", fromPlayer), ("_speed", speed));
            SavePrefab(root, path);
        }

        // Bomba naranja (3,0) lenta que estalla en 8 balas enemigas.
        private static void BuildGrenadePrefab()
        {
            var root = new GameObject("Granada") { layer = LayerMask.NameToLayer("Hazard") };
            AddSprite(root, Piece(TileSheet, 3, 0), "Player", 6);
            AddTriggerBody(root, 0.3f, out _);
            SetValues(root.AddComponent<Bullet>(), ("_fromPlayer", false), ("_speed", 4f));
            SetFields(root.AddComponent<Grenade>(), ("_fragmentPrefab", LoadPrefab<Bullet>(EnemyBulletPrefabPath)),
                ("_explosion", LoadPrefab<Transform>(ExplosionPrefabPath).gameObject), ("_sound", LoadSound(ExplosionSound)));
            SavePrefab(root, GrenadePrefabPath);
        }

        private static void BuildExplosionPrefab()
        {
            var root = new GameObject("Explosion");
            AddSprite(root, Piece(TileSheet, 4, 0), "Player", 20);
            root.AddComponent<Animator>().runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<AnimatorController>($"{AnimationsFolder}/Explosion/Explosion.controller");
            SetValues(root.AddComponent<AutoDestroy>(), ("_lifetime", 0.42f));
            SavePrefab(root, ExplosionPrefabPath);
        }

        // Nave azul. La zona de golpe es más pequeña que el dibujo, para que rozar una bala no duela.
        private static void BuildPlayerPrefab()
        {
            var root = new GameObject("Jugador") { tag = "Player", layer = LayerMask.NameToLayer("Player") };
            root.transform.rotation = Quaternion.Euler(0f, 0f, PlayerAngle);
            SpriteRenderer sprite = AddSprite(root, Piece(ShipSheet, 0, 0), "Player", 10);
            AddTriggerBody(root, 0.4f, out CircleCollider2D circle);

            InputActionReference[] actions = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath).OfType<InputActionReference>().ToArray();
            var controller = root.AddComponent<PlayerController>();
            SetFields(controller, ("_bulletPrefab", LoadPrefab<Bullet>(PlayerBulletPrefabPath)),
                ("_moveAction", FindAction(actions, "Move")), ("_fireAction", FindAction(actions, "Jump")),
                ("_shotSound", LoadSound(PlayerShotSound)));

            SetFields(root.AddComponent<PlayerHealth>(), ("_controller", controller), ("_collider", circle), ("_sprite", sprite),
                ("_explosion", LoadPrefab<Transform>(ExplosionPrefabPath).gameObject),
                ("_hurtSound", LoadSound(HurtSound)), ("_deathSound", LoadSound(BigExplosionSound)));

            SavePrefab(root, PlayerPrefabPath);
        }

        private static void BuildEnemyPrefabs()
        {
            // Avioneta verde: cruza en línea recta. Cae de un balazo.
            GameObject straight = NewEnemy("EnemigoRecto", Piece(ShipSheet, 2, 1), 0.6f, 1, 100);
            SetValues(straight.AddComponent<EnemyMover>(), ("_speed", 6f));
            SavePrefab(straight, StraightEnemyPrefabPath);

            // Avioneta roja: avanza ondulando.
            GameObject wave = NewEnemy("EnemigoOndulante", Piece(ShipSheet, 1, 1), 0.6f, 2, 200);
            SetValues(wave.AddComponent<EnemyMover>(), ("_speed", 4f), ("_waveHeight", 3f), ("_waveTime", 2.5f));
            SavePrefab(wave, WaveEnemyPrefabPath);

            // Nave amarilla: lenta, aguanta varios balazos y dispara al jugador.
            GameObject shooter = NewEnemy("EnemigoTirador", Piece(ShipSheet, 3, 0), 0.8f, 5, 500);
            SetValues(shooter.AddComponent<EnemyMover>(), ("_speed", 2.5f));
            AddShooter(shooter, 1.6f, 1, 0f);
            SavePrefab(shooter, ShooterEnemyPrefabPath);

            // Cañón fijo en el suelo: no se mueve, la cámara pasa por encima.
            GameObject turret = NewEnemy("Torreta", Piece(TileSheet, 4, 1), 0.5f, 4, 300);
            AddShooter(turret, 2f, 1, 0f);
            SavePrefab(turret, TurretPrefabPath);

            // Jefe: nave grande que se queda en pantalla hasta que la destruyan, con disparos en abanico.
            GameObject boss = NewEnemy("Jefe", Piece(ShipSheet, 1, 0), 0.85f, 100, 5000);
            boss.transform.localScale = new Vector3(2f, 2f, 1f);
            SetValues(boss.AddComponent<EnemyMover>(), ("_speed", 3f), ("_waveHeight", 4f), ("_waveTime", 5f),
                ("_stayOnScreen", true), ("_screenDistance", 4f));
            AddShooter(boss, 1.3f, 5, 60f);
            SetFields(boss.GetComponent<Enemy>(), ("_deathSound", LoadSound(BigExplosionSound)));
            SavePrefab(boss, BossPrefabPath);

            // Nave grande verde: lenta y resistente, lanza granadas que estallan cerca del jugador.
            GameObject grenadier = NewEnemy("EnemigoGranadero", Piece(ShipSheet, 2, 0), 0.8f, 6, 600);
            SetValues(grenadier.AddComponent<EnemyMover>(), ("_speed", 2f));
            AddShooter(grenadier, 2.5f, 1, 0f, GrenadePrefabPath);
            SavePrefab(grenadier, GrenadierPrefabPath);

            // Avioneta amarilla de apoyo: se queda en pantalla y potencia (en morado) a los enemigos de su alrededor hasta que la destruyan.
            GameObject booster = NewEnemy("Potenciador", Piece(ShipSheet, 3, 1), 0.6f, 6, 800);
            SetValues(booster.AddComponent<EnemyMover>(), ("_speed", 3f), ("_waveHeight", 2f), ("_waveTime", 3f),
                ("_stayOnScreen", true), ("_screenDistance", 2f));
            booster.AddComponent<EnemyBooster>();
            SavePrefab(booster, BoosterPrefabPath);
        }

        // Lo común a los enemigos: dibujo, zona de golpe y vida.
        private static GameObject NewEnemy(string enemyName, Sprite art, float radius, int health, int points)
        {
            var root = new GameObject(enemyName) { layer = LayerMask.NameToLayer("Hazard") };
            root.transform.rotation = Quaternion.Euler(0f, 0f, EnemyAngle);
            SpriteRenderer sprite = AddSprite(root, art, "Traps", 5);
            AddTriggerBody(root, radius, out _);

            var enemy = root.AddComponent<Enemy>();
            SetFields(enemy, ("_sprite", sprite), ("_explosion", LoadPrefab<Transform>(ExplosionPrefabPath).gameObject),
                ("_hitSound", LoadSound(HitSound)), ("_deathSound", LoadSound(ExplosionSound)));
            SetValues(enemy, ("_health", health), ("_points", points));
            return root;
        }

        private static void AddShooter(GameObject root, float interval, int bullets, float spread, string bulletPath = EnemyBulletPrefabPath)
        {
            var shooter = root.AddComponent<EnemyShooter>();
            SetFields(shooter, ("_bulletPrefab", LoadPrefab<Bullet>(bulletPath)), ("_shotSound", LoadSound(EnemyShotSound)));
            SetValues(shooter, ("_fireInterval", interval), ("_bulletCount", bullets), ("_spreadAngle", spread));
        }

        private static void BuildPickupPrefab(string path, Sprite art, Pickup.PickupKind kind)
        {
            var root = new GameObject(Path.GetFileNameWithoutExtension(path));
            AddSprite(root, art, "Traps", 10);

            var circle = root.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.6f;

            var pickup = root.AddComponent<Pickup>();
            SetFields(pickup, ("_sound", LoadSound(PickupSound)));
            SetValues(pickup, ("_kind", (int)kind));
            SavePrefab(root, path);
        }

        // Punto de mira que marca el final del nivel en la escena. La cámara se para antes: en el juego no se ve.
        private static void BuildGoalPrefab()
        {
            var root = new GameObject("Meta");
            AddSprite(root, Piece(TileSheet, 3, 2), "Traps", 0);
            SetFields(root.AddComponent<Goal>(), ("_sound", LoadSound(GoalSound)));
            SavePrefab(root, GoalPrefabPath);
        }

        // Canvas con las vidas, los puntos y el mensaje central. Medidas en píxeles del juego (384×216).
        private static void BuildHudPrefab()
        {
            var root = new GameObject("Interfaz");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(384f, 216f);
            scaler.matchWidthOrHeight = 0.5f;

            var lives = (RectTransform)new GameObject("Vidas", typeof(RectTransform)).transform;
            lives.SetParent(root.transform, false);
            lives.anchorMin = lives.anchorMax = lives.pivot = new Vector2(0f, 1f);
            lives.anchoredPosition = new Vector2(4f, -4f);
            lives.sizeDelta = new Vector2(200f, 16f);
            var layout = lives.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 1f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            var life = new GameObject("Vida", typeof(RectTransform)).AddComponent<Image>();
            life.rectTransform.SetParent(lives, false);
            life.rectTransform.sizeDelta = new Vector2(16f, 16f);
            life.sprite = Piece(TileSheet, 0, 2);
            life.raycastTarget = false;

            Text scoreText = AddText(root.transform, "Puntos", 14, TextAnchor.UpperRight);
            scoreText.rectTransform.anchorMin = scoreText.rectTransform.anchorMax = scoreText.rectTransform.pivot = Vector2.one;
            scoreText.rectTransform.anchoredPosition = new Vector2(-6f, -4f);
            scoreText.rectTransform.sizeDelta = new Vector2(100f, 16f);

            Text messageText = AddText(root.transform, "Mensaje", 20, TextAnchor.MiddleCenter);
            messageText.rectTransform.anchorMin = Vector2.zero;
            messageText.rectTransform.anchorMax = Vector2.one;
            messageText.rectTransform.offsetMin = messageText.rectTransform.offsetMax = Vector2.zero;

            SetFields(root.AddComponent<LevelHud>(), ("_life", life), ("_scoreText", scoreText), ("_messageText", messageText));
            SavePrefab(root, HudPrefabPath);
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

        private static InputActionReference FindAction(InputActionReference[] actions, string actionName)
        {
            InputActionReference reference = actions.FirstOrDefault(
                r => r.action != null && r.action.name == actionName && r.action.actionMap.name == "Player");
            if (reference == null) Debug.LogWarning($"[Shmup] No se encontró la acción Player/{actionName} en {InputActionsPath}.");
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
            Tilemap islands = AddTilemap(grid, IslandTilemapName, 0);
            Tilemap decor = AddTilemap(grid, DecorTilemapName, 1);

            GameObject player = Spawn(PlayerPrefabPath, new Vector2(4f, ViewCenterY));
            if (exampleLayout)
            {
                PaintExampleLevel(islands, decor);
                SpawnExampleObjects();
            }
            else
            {
                PaintStarterLevel(islands, decor);
                Spawn(GoalPrefabPath, new Vector2(60.5f, ViewCenterY));
            }

            CreateCamera(player.transform.position);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath));

            EditorSceneManager.SaveScene(scene, scenePath);
            FocusPainting(islands);
            Debug.Log($"[Shmup] Nivel creado en {scenePath}. Pinta con la paleta '{PaletteName}' y dale a Play.");
        }

        private static Tilemap AddTilemap(Grid grid, string tilemapName, int order)
        {
            var tilemapObject = new GameObject(tilemapName);
            tilemapObject.transform.SetParent(grid.transform, false);
            var tilemap = tilemapObject.AddComponent<Tilemap>();
            var tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingLayerName = "Terrain";
            tilemapRenderer.sortingOrder = order;
            return tilemap;
        }

        // Dos islas de muestra: el resto del nivel lo pinta el alumno.
        private static void PaintStarterLevel(Tilemap islands, Tilemap decor)
        {
            TileBase grass = LoadTile(GrassTileName);
            Fill(islands, grass, 8, 16, 1, 4);
            Fill(islands, LoadTile(DesertTileName), 24, 32, 8, 11);
            Decorate(decor, "Arbol", (10, 3), (13, 2));
            Decorate(decor, "CasaAzul", (27, 10));
        }

        // Nivel de ejemplo (152 tiles de largo): islas cada vez más grandes hasta la del jefe.
        private static void PaintExampleLevel(Tilemap islands, Tilemap decor)
        {
            TileBase grass = LoadTile(GrassTileName);
            TileBase desert = LoadTile(DesertTileName);

            Fill(islands, grass, 6, 16, 1, 4);
            Fill(islands, grass, 9, 14, 5, 6);
            Fill(islands, desert, 20, 30, 9, 12);
            Fill(islands, desert, 24, 28, 7, 8);
            Fill(islands, grass, 36, 52, 0, 3);
            Fill(islands, grass, 40, 48, 4, 6);
            Fill(islands, grass, 58, 66, 8, 12);
            Fill(islands, desert, 72, 90, 1, 5);
            Fill(islands, desert, 78, 84, 6, 7);
            Fill(islands, grass, 96, 108, 8, 12);
            Fill(islands, grass, 100, 104, 6, 7);
            Fill(islands, desert, 114, 126, 1, 4);
            Fill(islands, grass, 132, 156, 2, 11);
            Fill(islands, grass, 138, 150, 0, 1);
            Fill(islands, grass, 138, 150, 12, 13);

            Decorate(decor, "Arbol", (8, 3), (11, 2), (14, 3), (42, 2), (47, 1), (60, 10), (64, 9), (98, 10), (106, 9));
            Decorate(decor, "Bosque", (10, 5), (13, 5), (38, 2), (50, 1), (62, 11), (102, 7), (136, 9), (140, 4), (150, 9));
            Decorate(decor, "Arbusto", (7, 2), (12, 4), (44, 5), (46, 3), (65, 11), (104, 10), (134, 4), (146, 12));
            Decorate(decor, "Casa", (41, 5), (43, 4), (103, 11), (144, 7), (148, 7));
            Decorate(decor, "Cabana", (45, 5), (100, 11), (142, 8), (146, 8));
            Decorate(decor, "Huerto", (46, 4), (47, 4), (99, 9), (100, 9), (143, 5), (144, 5), (145, 5));
            Decorate(decor, "Bandera", (46, 5), (144, 9));
            Decorate(decor, "ArbolDesierto", (22, 11), (29, 10), (75, 4), (82, 2), (88, 3), (116, 3), (124, 2));
            Decorate(decor, "BosqueDesierto", (25, 8), (74, 2), (86, 2), (118, 2), (122, 3));
            Decorate(decor, "CasaAzul", (23, 10), (26, 11), (79, 4), (81, 6), (120, 3));
            Decorate(decor, "CabanaAzul", (27, 10), (83, 6), (84, 3));
            Decorate(decor, "HuertoDesierto", (78, 3), (79, 3), (80, 3));
            Decorate(decor, "Islote", (19, 3), (33, 11), (55, 7), (69, 3), (93, 9), (111, 5), (129, 10));
            Decorate(decor, "Islotes", (3, 10), (18, 8), (34, 3), (56, 12), (68, 11), (92, 2), (110, 11), (128, 6));
            Decorate(decor, "IslotesDesierto", (33, 7), (54, 4), (70, 7), (94, 5), (112, 8), (130, 2));
        }

        private static void SpawnExampleObjects()
        {
            // Oleada 1: una fila que entra de frente y otra en diagonal.
            SpawnAll(StraightEnemyPrefabPath, (30, 10), (30, 7), (30, 4));
            SpawnAll(StraightEnemyPrefabPath, (40, 11), (42, 10), (44, 9), (46, 8));

            // Oleada 2: cuatro enemigos ondulantes en fila india y la primera Mejora.
            SpawnAll(WaveEnemyPrefabPath, (54, 7), (56, 7), (58, 7), (60, 7));
            Spawn(WeaponPickupPrefabPath, new Vector2(66f, 7f));

            // Oleada 3: un tirador protegido por torretas en la isla de abajo.
            GameObject shooter = Spawn(ShooterEnemyPrefabPath, new Vector2(78f, 10f));
            SetFields(shooter.GetComponent<Enemy>(), ("_drop", AssetDatabase.LoadAssetAtPath<GameObject>(WeaponPickupPrefabPath)));
            SpawnAll(TurretPrefabPath, (76, 3), (86, 4));

            // Oleada 4: ondulantes por arriba y dos tiradores que cierran el paso.
            // Un Potenciador entra con ellos: hasta destruirlo, los de su alrededor van más rápido y disparan el doble.
            SpawnAll(WaveEnemyPrefabPath, (92, 10), (94, 10), (96, 10));
            Spawn(BoosterPrefabPath, new Vector2(98f, 7f));
            SpawnAll(ShooterEnemyPrefabPath, (106, 3), (106, 11));
            Spawn(HealthPickupPrefabPath, new Vector2(112f, 7f));
            Spawn(GrenadierPrefabPath, new Vector2(114f, 10f));          // lanza granadas que se fragmentan

            // Oleada 5: muro de enemigos con un hueco y torretas antes del jefe.
            SpawnAll(StraightEnemyPrefabPath, (120, 2), (120, 4), (120, 6), (120, 10), (120, 12));
            SpawnAll(TurretPrefabPath, (118, 3), (124, 3));
            Spawn(HealthPickupPrefabPath, new Vector2(132f, 7f));

            // Jefe final, justo antes de la Meta.
            Spawn(BossPrefabPath, new Vector2(146f, ViewCenterY));
            Spawn(GoalPrefabPath, new Vector2(152.5f, ViewCenterY));
        }

        private static void SpawnAll(string prefabPath, params (float X, float Y)[] positions)
        {
            foreach (var (x, y) in positions)
            {
                Spawn(prefabPath, new Vector2(x, y));
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

        private static void Decorate(Tilemap decor, string tileName, params (int X, int Y)[] cells)
        {
            foreach (var (x, y) in cells)
            {
                decor.SetTile(new Vector3Int(x, y, 0), LoadTile(tileName));
            }
        }

        // Cámara pixel perfect que avanza sola, con la música del nivel. El agua es su color de fondo.
        private static void CreateCamera(Vector3 playerPosition)
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(playerPosition.x + 8f, ViewCenterY, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 216f / 2f / 16f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = WaterColor;
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

            cameraObject.AddComponent<ScrollCamera>();

            var music = new GameObject("Musica").AddComponent<AudioSource>();
            music.transform.SetParent(cameraObject.transform, false);
            music.clip = LoadSound(MusicPath);
            music.loop = true;
            music.volume = 0.4f;
            music.playOnAwake = true;
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

        // Deja la Tile Palette abierta, con la paleta del kit y el Tilemap "Islas" como destino.
        private static void FocusPainting(Tilemap islands)
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
            GridPaintingState.scenePaintTarget = islands.gameObject;
            Selection.activeGameObject = islands.gameObject;
        }

        // ---------- Utilidades ----------

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

        private static T LoadPrefab<T>(string path) where T : Component
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<T>();
        }

        private static AudioClip LoadSound(string path)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) Debug.LogWarning($"[Shmup] Falta el sonido {path}.");
            return clip;
        }

        private static TileBase LoadTile(string tileName)
        {
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TilesFolder}/{tileName}.asset");
            if (tile == null) Debug.LogWarning($"[Shmup] Falta el tile {TilesFolder}/{tileName}.asset.");
            return tile;
        }

        private static T GetOrCreateAsset<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = (T)(Object)ScriptableObject.CreateInstance(typeof(T));
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // Cambia referencias de un componente.
        private static void SetFields(Object target, params (string Name, Object Value)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach (var (fieldName, value) in fields)
            {
                serialized.FindProperty(fieldName).objectReferenceValue = value;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Cambia valores (no referencias) de un componente.
        private static void SetValues(Object target, params (string Name, object Value)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach (var (fieldName, value) in fields)
            {
                SerializedProperty property = serialized.FindProperty(fieldName);
                switch (value)
                {
                    case float number: property.floatValue = number; break;
                    case int number: property.intValue = number; break;
                    case bool flag: property.boolValue = flag; break;
                }
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
