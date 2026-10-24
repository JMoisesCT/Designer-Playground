using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Platformer.Editor
{
    // Genera todo el kit de plataformas (animaciones, tiles, paleta, prefabs) y crea niveles listos para pintar.
    public static class PlatformerBuilder
    {
        private const string Root = "Assets/Platformer";
        private const string AnimationsFolder = Root + "/Art/Animations";
        private const string TilesFolder = Root + "/Art/Tiles";
        private const string PaletteFolder = Root + "/Art/Palettes";
        private const string PaletteName = "Platformer";
        private const string PalettePath = PaletteFolder + "/" + PaletteName + ".prefab";
        private const string PrefabsFolder = Root + "/Prefabs";
        private const string PhysicsMaterialPath = Root + "/Art/SinFriccion.physicsMaterial2D";
        private const string ScenesFolder = Root + "/Scenes";
        public const string ExampleScenePath = ScenesFolder + "/Nivel_01.unity";

        private const string Art = "Assets/ThirdParty/Pixel Adventure 1/Assets";
        private const string TerrainSheet = Art + "/Terrain/Terrain Sliced (16x16).png";
        private const string BackgroundSprite = Art + "/Background/Blue.png";
        private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";
        private const int FrameRate = 20;

        private const string PlayerPrefabPath = PrefabsFolder + "/Jugador.prefab";
        private const string EnemyPrefabPath = PrefabsFolder + "/Enemigo.prefab";
        private const string SawPrefabPath = PrefabsFolder + "/Sierra.prefab";
        private const string GoalPrefabPath = PrefabsFolder + "/Meta.prefab";
        private const string FruitPrefabPath = PrefabsFolder + "/Fruta.prefab";
        private const string PlatformPrefabPath = PrefabsFolder + "/PlataformaMovil.prefab";
        private const string TrampolinePrefabPath = PrefabsFolder + "/Trampolin.prefab";
        private const string HudPrefabPath = PrefabsFolder + "/Interfaz.prefab";
        private const string CheckpointPrefabPath = PrefabsFolder + "/Bandera.prefab";

        // Sonidos de Kenney (el alumno puede cambiarlos en el Inspector de cada prefab).
        private const string Sfx = "Assets/ThirdParty/Kenney/Audio_DigitalSFX/Audio";
        private const string Jingles = "Assets/ThirdParty/Kenney/Audio_MusicJingles/Audio/8-Bit jingles";
        private const string JumpSound = Sfx + "/phaseJump1.ogg";
        private const string DeathSound = Sfx + "/lowDown.ogg";
        private const string StompSound = Sfx + "/pepSound1.ogg";
        private const string FruitSound = Sfx + "/highUp.ogg";
        private const string TrampolineSound = Sfx + "/phaserUp2.ogg";
        private const string CheckpointSound = Sfx + "/threeTone1.ogg";
        private const string GoalSound = Jingles + "/jingles_NES00.ogg";

        private const string TerrainTilemapName = "Terreno";

        // Bloques 3×3 de la hoja de terreno: nombre del tile y esquina superior izquierda (en píxeles, origen abajo).
        private static readonly (string Name, int X, int Y)[] TerrainBlocks =
        {
            ("Pasto", 96, 160),
            ("Tierra", 96, 96),
            ("Caramelo", 96, 32),
        };

        private const string SpikesTileName = "Pinchos";

        [MenuItem("Platformer/Crear nivel nuevo", priority = 0)]
        private static void CreateLevelFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolder(ScenesFolder);
            string path = EditorUtility.SaveFilePanelInProject(
                "Crear nivel nuevo", "Nivel_02", "unity", "Elige el nombre del nivel.", ScenesFolder);
            if (string.IsNullOrEmpty(path)) return;

            CreateLevel(path, exampleLayout: false);
        }

        [MenuItem("Platformer/Avanzado/Regenerar assets del kit", priority = 100)]
        public static void BuildAllAssets()
        {
            BuildAnimations();
            BuildTiles();
            BuildPalette();
            BuildPrefabs();
            Debug.Log("[Platformer] Assets del kit generados.");
        }

        [MenuItem("Platformer/Avanzado/Regenerar nivel de ejemplo", priority = 101)]
        private static void BuildExampleLevelFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildAllAssets();
            CreateLevel(ExampleScenePath, exampleLayout: true);
        }

        // ---------- Animaciones ----------

        public static void BuildAnimations()
        {
            string frog = Art + "/Main Characters/Ninja Frog";
            BuildController("Jugador",
                BuildClip("Jugador", "Idle", $"{frog}/Idle (32x32).png", true),
                BuildClip("Jugador", "Run", $"{frog}/Run (32x32).png", true),
                BuildClip("Jugador", "Jump", $"{frog}/Jump (32x32).png", true),
                BuildClip("Jugador", "Fall", $"{frog}/Fall (32x32).png", true),
                BuildClip("Jugador", "Hit", $"{frog}/Hit (32x32).png", false),
                BuildClip("Jugador", "DoubleJump", $"{frog}/Double Jump (32x32).png", true));

            string mask = Art + "/Main Characters/Mask Dude";
            BuildController("Enemigo",
                BuildClip("Enemigo", "Run", $"{mask}/Run (32x32).png", true),
                BuildClip("Enemigo", "Hit", $"{mask}/Hit (32x32).png", false));

            BuildController("Sierra",
                BuildClip("Sierra", "On", $"{Art}/Traps/Saw/On (38x38).png", true));

            string end = Art + "/Items/Checkpoints/End";
            BuildController("Meta",
                BuildClip("Meta", "Idle", $"{end}/End (Idle).png", false),
                BuildClip("Meta", "Pressed", $"{end}/End (Pressed) (64x64).png", false));

            string flag = Art + "/Items/Checkpoints/Checkpoint";
            BuildController("Bandera",
                BuildClip("Bandera", "NoFlag", $"{flag}/Checkpoint (No Flag).png", false),
                BuildClip("Bandera", "FlagOut", $"{flag}/Checkpoint (Flag Out) (64x64).png", false),
                BuildClip("Bandera", "FlagIdle", $"{flag}/Checkpoint (Flag Idle)(64x64).png", true));
            AddExitTransition("Bandera", "FlagOut", "FlagIdle");

            // Un estado por fruta (mismo nombre que Fruit.Kind) y "Collected" al recogerla.
            var fruitClips = new List<AnimationClip>();
            foreach (Fruit.Kind kind in Enum.GetValues(typeof(Fruit.Kind)))
            {
                fruitClips.Add(BuildClip("Fruta", kind.ToString(), FruitSheet(kind), true));
            }
            fruitClips.Add(BuildClip("Fruta", "Collected", $"{Art}/Items/Fruits/Collected.png", false));
            BuildController("Fruta", fruitClips.ToArray());

            BuildController("PlataformaMovil",
                BuildClip("PlataformaMovil", "On", $"{Art}/Traps/Platforms/Brown On (32x8).png", true));

            string trampoline = Art + "/Traps/Trampoline";
            BuildController("Trampolin",
                BuildClip("Trampolin", "Idle", $"{trampoline}/Idle.png", false),
                BuildClip("Trampolin", "Jump", $"{trampoline}/Jump (28x28).png", false));

            AssetDatabase.SaveAssets();
        }

        private static AnimationClip BuildClip(string character, string clipName, string sheetPath, bool loop)
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

            Sprite[] frames = LoadFrames(sheetPath);
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = (float)i / FrameRate, value = frames[i] };
            }
            // Clave extra al final para que el último frame dure lo mismo que los demás.
            keys[frames.Length] = new ObjectReferenceKeyframe { time = (float)frames.Length / FrameRate, value = frames[^1] };

            clip.frameRate = FrameRate;
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

        // Al terminar el estado "from" pasa solo a "to" (la bandera se despliega y después ondea).
        private static void AddExitTransition(string character, string from, string to)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{AnimationsFolder}/{character}/{character}.controller");
            ChildAnimatorState[] states = controller.layers[0].stateMachine.states;
            AnimatorState fromState = states.First(s => s.state.name == from).state;
            AnimatorState toState = states.First(s => s.state.name == to).state;

            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = true;
            transition.exitTime = 1f;
            transition.duration = 0f;
            EditorUtility.SetDirty(controller);
        }

        // ---------- Tiles y paleta ----------

        public static void BuildTiles()
        {
            EnsureFolder(TilesFolder);
            Sprite[] terrain = AssetDatabase.LoadAllAssetsAtPath(TerrainSheet).OfType<Sprite>().ToArray();

            foreach (var (blockName, x, y) in TerrainBlocks)
            {
                BuildRuleTile(blockName, terrain, x, y);
            }

            var spikes = GetOrCreateAsset<HazardTile>($"{TilesFolder}/{SpikesTileName}.asset");
            spikes.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "/Traps/Spikes/Idle.png");
            spikes.colliderType = Tile.ColliderType.Sprite;
            EditorUtility.SetDirty(spikes);

            AssetDatabase.SaveAssets();
        }

        // Rule Tile que elige solo la esquina, borde o centro según los vecinos.
        private static void BuildRuleTile(string tileName, Sprite[] sheet, int left, int top)
        {
            Sprite Piece(int column, int row) =>
                sheet.First(s => (int)s.rect.x == left + column * 16 && (int)s.rect.y == top - row * 16);

            const int NotThis = RuleTile.TilingRuleOutput.Neighbor.NotThis;

            var tile = GetOrCreateAsset<RuleTile>($"{TilesFolder}/{tileName}.asset");
            tile.m_DefaultSprite = Piece(1, 0);
            tile.m_DefaultColliderType = Tile.ColliderType.Grid;
            tile.m_TilingRules = new List<RuleTile.TilingRule>
            {
                // (arriba, abajo, izquierda, derecha): 0 = da igual. La primera regla que encaja gana.
                Rule(Piece(0, 0), NotThis, 0, NotThis, 0),
                Rule(Piece(2, 0), NotThis, 0, 0, NotThis),
                Rule(Piece(1, 0), NotThis, 0, 0, 0),
                Rule(Piece(0, 2), 0, NotThis, NotThis, 0),
                Rule(Piece(2, 2), 0, NotThis, 0, NotThis),
                Rule(Piece(1, 2), 0, NotThis, 0, 0),
                Rule(Piece(0, 1), 0, 0, NotThis, 0),
                Rule(Piece(2, 1), 0, 0, 0, NotThis),
                Rule(Piece(1, 1), 0, 0, 0, 0),
            };
            EditorUtility.SetDirty(tile);
        }

        private static RuleTile.TilingRule Rule(Sprite sprite, int up, int down, int left, int right)
        {
            var rule = new RuleTile.TilingRule
            {
                m_Sprites = new[] { sprite },
                m_ColliderType = Tile.ColliderType.Grid,
            };
            var neighbors = new Dictionary<Vector3Int, int>();
            if (up != 0) neighbors[Vector3Int.up] = up;
            if (down != 0) neighbors[Vector3Int.down] = down;
            if (left != 0) neighbors[Vector3Int.left] = left;
            if (right != 0) neighbors[Vector3Int.right] = right;
            rule.ApplyNeighbors(neighbors);
            return rule;
        }

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
            for (int i = 0; i < TerrainBlocks.Length; i++)
            {
                tilemap.SetTile(new Vector3Int(i * 2, 0, 0), LoadTile(TerrainBlocks[i].Name));
            }
            tilemap.SetTile(new Vector3Int(TerrainBlocks.Length * 2, 0, 0), LoadTile(SpikesTileName));
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
                BuildEnemyPrefab();
                BuildSawPrefab();
                BuildGoalPrefab();
                BuildFruitPrefab();
                BuildPlatformPrefab();
                BuildTrampolinePrefab();
                BuildCheckpointPrefab();
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
            SpriteRenderer sprite = AddSprite(root, "Jugador", "Idle", "Player", 10);
            Animator animator = AddAnimator(root, "Jugador");

            var body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 4f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Los pies quedan 1 tile por debajo del centro del sprite (32×32 px = 2×2 tiles).
            var box = root.AddComponent<BoxCollider2D>();
            box.edgeRadius = 0.05f;
            box.size = new Vector2(0.8f, 1.5f);
            box.offset = new Vector2(0f, -0.2f);
            box.sharedMaterial = material;

            InputActionReference[] actions = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath).OfType<InputActionReference>().ToArray();
            var controller = root.AddComponent<PlayerController>();
            SetFields(controller,
                ("_body", body), ("_sprite", sprite), ("_animator", animator),
                ("_moveAction", FindAction(actions, "Move")), ("_jumpAction", FindAction(actions, "Jump")),
                ("_jumpSound", LoadSound(JumpSound)));
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("_groundLayers").intValue = LayerMask.GetMask("Ground");
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            var health = root.AddComponent<PlayerHealth>();
            SetFields(health, ("_controller", controller), ("_body", body), ("_collider", box), ("_animator", animator),
                ("_deathSound", LoadSound(DeathSound)));

            SavePrefab(root, PlayerPrefabPath);
        }

        private static void BuildEnemyPrefab()
        {
            var root = new GameObject("Enemigo") { layer = LayerMask.NameToLayer("Hazard") };
            SpriteRenderer sprite = AddSprite(root, "Enemigo", "Run", "Player", 5);
            Animator animator = AddAnimator(root, "Enemigo");
            Rigidbody2D body = AddKinematicBody(root);

            var box = root.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(0.9f, 1.5f);
            box.offset = new Vector2(0f, -0.25f);

            var mover = root.AddComponent<PatrolMover>();
            SetFields(mover, ("_body", body), ("_sprite", sprite));

            var enemy = root.AddComponent<StompableEnemy>();
            SetFields(enemy, ("_collider", box), ("_animator", animator), ("_mover", mover), ("_stompSound", LoadSound(StompSound)));

            SavePrefab(root, EnemyPrefabPath);
        }

        private static void BuildSawPrefab()
        {
            var root = new GameObject("Sierra") { layer = LayerMask.NameToLayer("Hazard") };
            AddSprite(root, "Sierra", "On", "Traps", 0);
            AddAnimator(root, "Sierra");
            Rigidbody2D body = AddKinematicBody(root);

            var circle = root.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 1f;

            var mover = root.AddComponent<PatrolMover>();
            SetFields(mover, ("_body", body));
            var serializedMover = new SerializedObject(mover);
            serializedMover.FindProperty("_speed").floatValue = 3f;
            serializedMover.FindProperty("_waitTime").floatValue = 0.3f;
            serializedMover.FindProperty("_flipSprite").boolValue = false;
            serializedMover.ApplyModifiedPropertiesWithoutUndo();

            root.AddComponent<Hazard>();
            SavePrefab(root, SawPrefabPath);
        }

        // Trofeo de 4×4 tiles: los pies del trofeo quedan 2 tiles por debajo del centro.
        private static void BuildGoalPrefab()
        {
            var root = new GameObject("Meta");
            AddSprite(root, "Meta", "Idle", "Traps", 0);
            Animator animator = AddAnimator(root, "Meta");

            var box = root.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(2.4f, 2.6f);
            box.offset = new Vector2(0f, -0.7f);

            var goal = root.AddComponent<Goal>();
            SetFields(goal, ("_animator", animator), ("_sound", LoadSound(GoalSound)));
            SavePrefab(root, GoalPrefabPath);
        }

        // Bandera de 4×4 tiles como la meta: los pies quedan 2 tiles por debajo del centro y el mástil, a la izquierda.
        private static void BuildCheckpointPrefab()
        {
            var root = new GameObject("Bandera");
            AddSprite(root, "Bandera", "NoFlag", "Traps", 0);
            Animator animator = AddAnimator(root, "Bandera");

            var box = root.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 2.8f);
            box.offset = new Vector2(-0.5f, -0.6f);

            var checkpoint = root.AddComponent<Checkpoint>();
            SetFields(checkpoint, ("_animator", animator), ("_sound", LoadSound(CheckpointSound)));
            SavePrefab(root, CheckpointPrefabPath);
        }

        private static void BuildFruitPrefab()
        {
            var root = new GameObject("Fruta");
            SpriteRenderer sprite = AddSprite(root, "Fruta", nameof(Fruit.Kind.Apple), "Traps", 5);
            Animator animator = AddAnimator(root, "Fruta");

            var circle = root.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.45f;
            circle.offset = new Vector2(0f, 0.1f);

            var fruit = root.AddComponent<Fruit>();
            SetFields(fruit, ("_collider", circle), ("_animator", animator), ("_sprite", sprite), ("_sound", LoadSound(FruitSound)));

            var serializedFruit = new SerializedObject(fruit);
            SerializedProperty sprites = serializedFruit.FindProperty("_sprites");
            var kinds = (Fruit.Kind[])Enum.GetValues(typeof(Fruit.Kind));
            sprites.arraySize = kinds.Length;
            for (int i = 0; i < kinds.Length; i++)
            {
                sprites.GetArrayElementAtIndex(i).objectReferenceValue = LoadFrames(FruitSheet(kinds[i]))[0];
            }
            serializedFruit.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, FruitPrefabPath);
        }

        // Plataforma de 2 tiles de ancho que se atraviesa desde abajo y lleva al jugador encima.
        private static void BuildPlatformPrefab()
        {
            var root = new GameObject("PlataformaMovil") { layer = LayerMask.NameToLayer("Ground") };
            AddSprite(root, "PlataformaMovil", "On", "Traps", 0);
            AddAnimator(root, "PlataformaMovil");
            Rigidbody2D body = AddKinematicBody(root);

            // El dibujo mide 32×7 px visibles: la superficie queda 3 px por encima del centro.
            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2f, 0.4375f);
            box.offset = new Vector2(0f, -0.03125f);
            box.usedByEffector = true;
            root.AddComponent<PlatformEffector2D>().useOneWay = true;

            var mover = root.AddComponent<PatrolMover>();
            SetFields(mover, ("_body", body));
            var serializedMover = new SerializedObject(mover);
            serializedMover.FindProperty("_waitTime").floatValue = 0.5f;
            serializedMover.FindProperty("_flipSprite").boolValue = false;
            serializedMover.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, PlatformPrefabPath);
        }

        // Trampolín sólido (se puede chocar con él de lado); el dibujo visible ocupa los 11 px de abajo.
        private static void BuildTrampolinePrefab()
        {
            var root = new GameObject("Trampolin") { layer = LayerMask.NameToLayer("Ground") };
            AddSprite(root, "Trampolin", "Idle", "Traps", 0);
            Animator animator = AddAnimator(root, "Trampolin");

            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.375f, 0.625f);
            box.offset = new Vector2(0f, -0.55f);

            var trampoline = root.AddComponent<Trampoline>();
            SetFields(trampoline, ("_collider", box), ("_animator", animator), ("_sound", LoadSound(TrampolineSound)));
            SavePrefab(root, TrampolinePrefabPath);
        }

        // Canvas con el contador de frutas y el mensaje central. Medidas en píxeles del juego (384×216).
        private static void BuildHudPrefab()
        {
            var root = new GameObject("Interfaz");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(384f, 216f);
            scaler.matchWidthOrHeight = 0.5f;

            var counter = new GameObject("Frutas", typeof(RectTransform));
            var counterRect = (RectTransform)counter.transform;
            counterRect.SetParent(root.transform, false);
            counterRect.anchorMin = counterRect.anchorMax = counterRect.pivot = new Vector2(0f, 1f);
            counterRect.anchoredPosition = new Vector2(0f, 0f);
            counterRect.sizeDelta = new Vector2(80f, 32f);

            // La fruta solo ocupa el centro de su dibujo de 32×32 px.
            var icon = new GameObject("Icono", typeof(RectTransform)).AddComponent<Image>();
            icon.rectTransform.SetParent(counterRect, false);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = icon.rectTransform.pivot = new Vector2(0f, 0.5f);
            icon.rectTransform.sizeDelta = new Vector2(32f, 32f);
            icon.sprite = LoadFrames(FruitSheet(Fruit.Kind.Apple))[0];

            Text fruitText = AddText(counterRect, "Contador", 12, TextAnchor.MiddleLeft);
            fruitText.rectTransform.anchorMin = new Vector2(0f, 0f);
            fruitText.rectTransform.anchorMax = new Vector2(1f, 1f);
            fruitText.rectTransform.offsetMin = new Vector2(25f, 0f);
            fruitText.rectTransform.offsetMax = Vector2.zero;

            Text messageText = AddText(root.transform, "Mensaje", 24, TextAnchor.MiddleCenter);
            messageText.rectTransform.anchorMin = Vector2.zero;
            messageText.rectTransform.anchorMax = Vector2.one;
            messageText.rectTransform.offsetMin = messageText.rectTransform.offsetMax = Vector2.zero;

            var hud = root.AddComponent<LevelHud>();
            SetFields(hud, ("_fruitCounter", counter), ("_fruitText", fruitText), ("_messageText", messageText));
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

        private static string FruitSheet(Fruit.Kind kind) => $"{Art}/Items/Fruits/{kind}.png";

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

        private static Rigidbody2D AddKinematicBody(GameObject root)
        {
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            return body;
        }

        private static InputActionReference FindAction(InputActionReference[] actions, string actionName)
        {
            InputActionReference reference = actions.FirstOrDefault(
                r => r.action != null && r.action.name == actionName && r.action.actionMap.name == "Player");
            if (reference == null) Debug.LogWarning($"[Platformer] No se encontró la acción Player/{actionName} en {InputActionsPath}.");
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
            var terrainObject = new GameObject(TerrainTilemapName) { layer = LayerMask.NameToLayer("Ground") };
            terrainObject.transform.SetParent(grid.transform, false);
            var terrain = terrainObject.AddComponent<Tilemap>();
            terrainObject.AddComponent<TilemapRenderer>().sortingLayerName = "Terrain";

            // Se pinta antes de añadir los colliders: si no, la escena se guarda con el collider vacío.
            if (exampleLayout) PaintExampleLevel(terrain);
            else PaintStarterLevel(terrain);
            AddCollision(terrain);

            GameObject player = Spawn(PlayerPrefabPath, new Vector2(exampleLayout ? -6f : 0f, 1f));
            if (exampleLayout) SpawnExampleObjects();
            else Spawn(GoalPrefabPath, new Vector2(18f, 2f));

            CameraFollow cameraFollow = CreateCamera(player.transform.position);
            if (exampleLayout)
            {
                var serializedCamera = new SerializedObject(cameraFollow);
                serializedCamera.FindProperty("_useBounds").boolValue = true;
                serializedCamera.FindProperty("_boundsMin").vector2Value = new Vector2(-14f, -4f);
                serializedCamera.FindProperty("_boundsMax").vector2Value = new Vector2(109f, 16f);
                serializedCamera.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath));

            EditorSceneManager.SaveScene(scene, scenePath);
            FocusPainting(terrain);
            Debug.Log($"[Platformer] Nivel creado en {scenePath}. Pinta con la paleta '{PaletteName}' y dale a Play.");
        }

        private static void PaintStarterLevel(Tilemap terrain)
        {
            TileBase grass = LoadTile("Pasto");
            Fill(terrain, grass, -12, 20, -4, -1);
            Fill(terrain, LoadTile("Tierra"), 6, 9, 2, 2);
        }

        // Nivel de ejemplo: un recorrido corto que usa todo lo que trae el kit.
        private static void PaintExampleLevel(Tilemap terrain)
        {
            TileBase grass = LoadTile("Pasto");
            TileBase spikes = LoadTile(SpikesTileName);

            Fill(terrain, grass, -14, -11, -4, 9);          // pared izquierda
            Fill(terrain, grass, -10, 20, -4, -1);          // primer suelo
            Fill(terrain, spikes, 8, 9, 0, 0);              // pinchos para saltar
            Fill(terrain, LoadTile("Tierra"), 13, 16, 2, 2);
            // hueco de 4 tiles (21..24)
            Fill(terrain, grass, 25, 72, -4, -1);           // segundo suelo
            Fill(terrain, grass, 38, 44, 0, 1);             // escalón
            Fill(terrain, spikes, 45, 53, 0, 0);            // foso de pinchos
            Fill(terrain, LoadTile("Caramelo"), 48, 50, 2, 2);
            Fill(terrain, LoadTile("Tierra"), 62, 67, 2, 2);
            // foso de 12 tiles (73..84): se cruza en la plataforma móvil
            Fill(terrain, grass, 85, 104, -4, -1);          // tercer suelo
            Fill(terrain, grass, 93, 104, 0, 6);            // acantilado: solo se sube con el trampolín
            Fill(terrain, grass, 105, 108, -4, 13);         // pared final
        }

        private static void SpawnExampleObjects()
        {
            Spawn(EnemyPrefabPath, new Vector2(28.5f, 1f), new Vector2(7f, 0f));
            Spawn(SawPrefabPath, new Vector2(58f, 1f), new Vector2(0f, 4f));
            Spawn(EnemyPrefabPath, new Vector2(62.5f, 4f), new Vector2(5f, 0f));
            Spawn(EnemyPrefabPath, new Vector2(64.5f, 1f), new Vector2(-3f, 0f));

            Spawn(CheckpointPrefabPath, new Vector2(70f, 2f));              // antes del foso de la plataforma móvil
            Spawn(PlatformPrefabPath, new Vector2(75f, -0.25f), new Vector2(8f, 0f));
            Spawn(TrampolinePrefabPath, new Vector2(89.5f, 0.875f));
            Spawn(GoalPrefabPath, new Vector2(99f, 9f));

            SpawnFruit(Fruit.Kind.Apple, new Vector2(8.5f, 3f));
            SpawnFruit(Fruit.Kind.Apple, new Vector2(14.5f, 4f));
            SpawnFruit(Fruit.Kind.Bananas, new Vector2(22.5f, 3f));
            SpawnFruit(Fruit.Kind.Cherries, new Vector2(41f, 3.5f));
            SpawnFruit(Fruit.Kind.Strawberry, new Vector2(49f, 4.5f));
            SpawnFruit(Fruit.Kind.Kiwi, new Vector2(79f, 2f));
            SpawnFruit(Fruit.Kind.Melon, new Vector2(89.5f, 5f));
            SpawnFruit(Fruit.Kind.Pineapple, new Vector2(96f, 8f));
        }

        private static void SpawnFruit(Fruit.Kind kind, Vector2 position)
        {
            var fruit = new SerializedObject(Spawn(FruitPrefabPath, position).GetComponent<Fruit>());
            fruit.FindProperty("_kind").enumValueIndex = (int)kind;
            fruit.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Spawn(string prefabPath, Vector2 position, Vector2? patrolOffset = null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
            if (patrolOffset.HasValue)
            {
                var mover = new SerializedObject(instance.GetComponent<PatrolMover>());
                mover.FindProperty("_endOffset").vector2Value = patrolOffset.Value;
                mover.ApplyModifiedPropertiesWithoutUndo();
            }
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

        // Cámara pixel perfect que sigue al jugador, con el fondo como hijo para que siempre lo cubra.
        private static CameraFollow CreateCamera(Vector3 playerPosition)
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(playerPosition.x, playerPosition.y + 1.5f, -10f);

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
            pixelPerfect.gridSnapping = PixelPerfectCamera.GridSnapping.PixelSnapping;
            pixelPerfect.cropFrame = PixelPerfectCamera.CropFrame.None;

            var cameraFollow = cameraObject.AddComponent<CameraFollow>();

            var background = new GameObject("Fondo");
            background.transform.SetParent(cameraObject.transform, false);
            background.transform.localPosition = new Vector3(0f, 0f, 20f);
            var spriteRenderer = background.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSprite);
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.size = new Vector2(48f, 28f);
            spriteRenderer.sortingLayerName = "Background";
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

        // Deja la Tile Palette abierta, con la paleta del kit y el Tilemap "Terreno" como destino.
        private static void FocusPainting(Tilemap terrain)
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
            GridPaintingState.scenePaintTarget = terrain.gameObject;
            Selection.activeGameObject = terrain.gameObject;
        }

        // ---------- Utilidades ----------

        private static Sprite[] LoadFrames(string sheetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().OrderBy(FrameIndex).ToArray();
        }

        // Los sub-sprites se llaman "<archivo>_<n>".
        private static int FrameIndex(Sprite sprite)
        {
            int underscore = sprite.name.LastIndexOf('_');
            return underscore >= 0 && int.TryParse(sprite.name[(underscore + 1)..], out int index) ? index : 0;
        }

        private static AudioClip LoadSound(string path)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) Debug.LogWarning($"[Platformer] Falta el sonido {path}.");
            return clip;
        }

        private static TileBase LoadTile(string tileName)
        {
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TilesFolder}/{tileName}.asset");
            if (tile == null) Debug.LogWarning($"[Platformer] Falta el tile {TilesFolder}/{tileName}.asset.");
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
