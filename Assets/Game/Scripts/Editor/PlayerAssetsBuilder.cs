using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

// Genera los assets del jugador: clips, Animator Controller base, overrides y skins por personaje, presets de movimiento y prefab.
// Idempotente: los clips y overrides se actualizan en su sitio; controller, presets y prefab solo se crean si faltan.
public static class PlayerAssetsBuilder
{
    private const string CharactersRoot = "Assets/ThirdParty/Pixel Adventure 1/Assets/Main Characters";
    private const string AnimationsRoot = "Assets/Game/Art/Animations/Player";
    private const string BaseControllerPath = AnimationsRoot + "/Player.controller";
    private const string DataRoot = "Assets/Game/Data/Player";
    private const string SkinsRoot = DataRoot + "/Skins";
    private const string PhysicsMaterialPath = DataRoot + "/PlayerNoFriction.physicsMaterial2D";
    private const string PrefabPath = "Assets/Game/Prefabs/Player/Player.prefab";
    private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";
    private const string EventsRoot = "Assets/Game/Events";
    private const string BaseCharacter = "Ninja Frog";
    private const int FrameRate = 20;

    private static readonly string[] Characters = { "Ninja Frog", "Mask Dude", "Pink Man", "Virtual Guy" };

    private static readonly (string Key, string Sheet, bool Loop)[] ClipDefinitions =
    {
        ("Idle", "Idle", true),
        ("Run", "Run", true),
        ("Jump", "Jump", true),
        ("Fall", "Fall", true),
        ("DoubleJump", "Double Jump", false),
        ("WallJump", "Wall Jump", true),
        ("Hit", "Hit", false),
    };

    // Presets de movimiento: nombre de campo serializado → valor.
    private static readonly (string Name, Dictionary<string, float> Values)[] Presets =
    {
        ("Preciso", new Dictionary<string, float>
        {
            ["_maxRunSpeed"] = 8f, ["_groundAccelerationTime"] = 0.05f, ["_groundDecelerationTime"] = 0.04f,
            ["_airAccelerationTime"] = 0.08f, ["_airDecelerationTime"] = 0.12f,
            ["_jumpHeight"] = 3.2f, ["_timeToApex"] = 0.36f, ["_fallGravityMultiplier"] = 1.8f,
            ["_jumpCutGravityMultiplier"] = 3f, ["_maxFallSpeed"] = 16f, ["_coyoteTime"] = 0.1f, ["_jumpBufferTime"] = 0.12f,
            ["_airJumps"] = 1, ["_airJumpHeight"] = 2.2f,
            ["_wallSlideMaxSpeed"] = 3f, ["_wallJumpHeight"] = 2.8f, ["_wallJumpPushSpeed"] = 8f,
            ["_wallJumpControlLockTime"] = 0.15f, ["_wallCoyoteTime"] = 0.1f,
        }),
        ("Floaty", new Dictionary<string, float>
        {
            ["_maxRunSpeed"] = 7f, ["_groundAccelerationTime"] = 0.18f, ["_groundDecelerationTime"] = 0.22f,
            ["_airAccelerationTime"] = 0.3f, ["_airDecelerationTime"] = 0.5f,
            ["_jumpHeight"] = 3.6f, ["_timeToApex"] = 0.55f, ["_fallGravityMultiplier"] = 1f,
            ["_jumpCutGravityMultiplier"] = 2f, ["_maxFallSpeed"] = 9f, ["_coyoteTime"] = 0.15f, ["_jumpBufferTime"] = 0.15f,
            ["_airJumps"] = 1, ["_airJumpHeight"] = 2.8f,
            ["_wallSlideMaxSpeed"] = 2f, ["_wallJumpHeight"] = 3f, ["_wallJumpPushSpeed"] = 7f,
            ["_wallJumpControlLockTime"] = 0.2f, ["_wallCoyoteTime"] = 0.12f,
        }),
        ("Pesado", new Dictionary<string, float>
        {
            ["_maxRunSpeed"] = 9.5f, ["_groundAccelerationTime"] = 0.12f, ["_groundDecelerationTime"] = 0.08f,
            ["_airAccelerationTime"] = 0.25f, ["_airDecelerationTime"] = 0.25f,
            ["_jumpHeight"] = 2.6f, ["_timeToApex"] = 0.3f, ["_fallGravityMultiplier"] = 2.6f,
            ["_jumpCutGravityMultiplier"] = 3f, ["_maxFallSpeed"] = 22f, ["_coyoteTime"] = 0.08f, ["_jumpBufferTime"] = 0.1f,
            ["_airJumps"] = 1, ["_airJumpHeight"] = 1.8f,
            ["_wallSlideMaxSpeed"] = 5f, ["_wallJumpHeight"] = 2.4f, ["_wallJumpPushSpeed"] = 9f,
            ["_wallJumpControlLockTime"] = 0.18f, ["_wallCoyoteTime"] = 0.08f,
        }),
    };

    [MenuItem("Designer Playground/Jugador/Generar assets del jugador", priority = 300)]
    public static void BuildAll()
    {
        BuildAnimationsAndSkins();
        BuildConfigPresets();
        BuildPhysicsMaterial();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) BuildPrefab();
        AssetDatabase.SaveAssets();
        Debug.Log("[Jugador] Assets del jugador generados.");
    }

    [MenuItem("Designer Playground/Jugador/Regenerar prefab Player (sobrescribe)", priority = 301)]
    public static void RebuildPrefab()
    {
        BuildAll();
        BuildPrefab();
        AssetDatabase.SaveAssets();
    }

    public static List<CharacterSkinSO> BuildAnimationsAndSkins()
    {
        var clipsByCharacter = Characters.ToDictionary(c => c, BuildClips);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(BaseControllerPath)
                                        ?? BuildBaseController(clipsByCharacter[BaseCharacter]);

        var skins = new List<CharacterSkinSO>();
        foreach (string character in Characters)
        {
            AnimatorOverrideController overrideController = BuildOverride(controller, character, clipsByCharacter[character]);
            skins.Add(BuildSkin(character, overrideController, clipsByCharacter[character]["Idle"]));
        }
        return skins;
    }

    private static Dictionary<string, AnimationClip> BuildClips(string character)
    {
        string folder = $"{AnimationsRoot}/{Compact(character)}";
        EnsureFolder(folder);

        var clips = new Dictionary<string, AnimationClip>();
        foreach (var definition in ClipDefinitions)
        {
            string sheetPath = $"{CharactersRoot}/{character}/{definition.Sheet} (32x32).png";
            Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().OrderBy(FrameIndex).ToArray();
            if (frames.Length == 0)
            {
                Debug.LogError($"[Jugador] No hay sprites en {sheetPath}.");
                continue;
            }

            string clipPath = $"{folder}/{Compact(character)}_{definition.Key}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            FillSpriteClip(clip, frames, definition.Loop);
            clips[definition.Key] = clip;
        }
        return clips;
    }

    private static void FillSpriteClip(AnimationClip clip, Sprite[] frames, bool loop)
    {
        clip.frameRate = FrameRate;
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

        // Unity ya añade la duración de un fotograma tras la última clave de sprite.
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = (float)i / FrameRate, value = frames[i] };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
    }

    private static AnimatorController BuildBaseController(Dictionary<string, AnimationClip> clips)
    {
        EnsureFolder(AnimationsRoot);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(BaseControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsWallSliding", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        controller.AddParameter("DoubleJump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = machine.AddState("Idle", new Vector3(300, 0));
        AnimatorState run = machine.AddState("Run", new Vector3(550, 0));
        AnimatorState jump = machine.AddState("Jump", new Vector3(300, 150));
        AnimatorState fall = machine.AddState("Fall", new Vector3(550, 150));
        AnimatorState wallJump = machine.AddState("WallJump", new Vector3(800, 150));
        AnimatorState doubleJump = machine.AddState("DoubleJump", new Vector3(425, 300));
        AnimatorState hit = machine.AddState("Hit", new Vector3(50, 300));
        idle.motion = clips["Idle"];
        run.motion = clips["Run"];
        jump.motion = clips["Jump"];
        fall.motion = clips["Fall"];
        wallJump.motion = clips["WallJump"];
        doubleJump.motion = clips["DoubleJump"];
        hit.motion = clips["Hit"];
        machine.defaultState = idle;

        const AnimatorConditionMode greater = AnimatorConditionMode.Greater;
        const AnimatorConditionMode less = AnimatorConditionMode.Less;
        const AnimatorConditionMode isTrue = AnimatorConditionMode.If;
        const AnimatorConditionMode isFalse = AnimatorConditionMode.IfNot;
        const float moving = 0.1f;

        // Suelo
        foreach (AnimatorState grounded in new[] { idle, run })
        {
            AddTransition(grounded, jump, false, (isFalse, 0, "IsGrounded"), (greater, moving, "VelocityY"));
            AddTransition(grounded, fall, false, (isFalse, 0, "IsGrounded"), (less, moving, "VelocityY"));
        }
        AddTransition(idle, run, false, (isTrue, 0, "IsGrounded"), (greater, moving, "Speed"));
        AddTransition(run, idle, false, (less, moving, "Speed"));

        // Aire (el orden importa: aterrizar y pared antes que subir/bajar)
        AddTransition(jump, idle, false, (isTrue, 0, "IsGrounded"));
        AddTransition(jump, wallJump, false, (isTrue, 0, "IsWallSliding"));
        AddTransition(jump, fall, false, (less, moving, "VelocityY"));
        AddTransition(fall, idle, false, (isTrue, 0, "IsGrounded"));
        AddTransition(fall, wallJump, false, (isTrue, 0, "IsWallSliding"));
        AddTransition(fall, jump, false, (greater, moving, "VelocityY"));
        AddTransition(wallJump, idle, false, (isTrue, 0, "IsGrounded"));
        AddTransition(wallJump, jump, false, (isFalse, 0, "IsWallSliding"), (greater, moving, "VelocityY"));
        AddTransition(wallJump, fall, false, (isFalse, 0, "IsWallSliding"), (less, moving, "VelocityY"));
        AddTransition(doubleJump, idle, false, (isTrue, 0, "IsGrounded"));
        AddTransition(doubleJump, wallJump, false, (isTrue, 0, "IsWallSliding"));
        AddTransition(doubleJump, jump, true, (greater, moving, "VelocityY"));
        AddTransition(doubleJump, fall, true, (less, moving, "VelocityY"));

        // Desde cualquier estado
        AnimatorStateTransition toDoubleJump = machine.AddAnyStateTransition(doubleJump);
        ConfigureTransition(toDoubleJump, false);
        toDoubleJump.canTransitionToSelf = true;
        toDoubleJump.AddCondition(isTrue, 0, "DoubleJump");

        AnimatorStateTransition toHit = machine.AddAnyStateTransition(hit);
        ConfigureTransition(toHit, false);
        toHit.canTransitionToSelf = false;
        toHit.AddCondition(isTrue, 0, "Hit");
        AddTransition(hit, idle, true, (isFalse, 0, "IsDead"));

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, bool exitTime,
        params (AnimatorConditionMode Mode, float Threshold, string Parameter)[] conditions)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        ConfigureTransition(transition, exitTime);
        foreach (var condition in conditions)
        {
            transition.AddCondition(condition.Mode, condition.Threshold, condition.Parameter);
        }
    }

    private static void ConfigureTransition(AnimatorStateTransition transition, bool exitTime)
    {
        transition.hasExitTime = exitTime;
        transition.exitTime = 1f;
        transition.hasFixedDuration = true;
        transition.duration = 0f;
    }

    private static AnimatorOverrideController BuildOverride(AnimatorController controller, string character, Dictionary<string, AnimationClip> clips)
    {
        string path = $"{AnimationsRoot}/{Compact(character)}/{Compact(character)}.overrideController";
        var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
        if (overrideController == null)
        {
            overrideController = new AnimatorOverrideController(controller);
            AssetDatabase.CreateAsset(overrideController, path);
        }
        overrideController.runtimeAnimatorController = controller;

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);
        for (int i = 0; i < overrides.Count; i++)
        {
            AnimationClip original = overrides[i].Key;
            string key = original.name.Substring(original.name.LastIndexOf('_') + 1);
            clips.TryGetValue(key, out AnimationClip replacement);
            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, replacement);
        }
        overrideController.ApplyOverrides(overrides);
        EditorUtility.SetDirty(overrideController);
        return overrideController;
    }

    private static CharacterSkinSO BuildSkin(string character, RuntimeAnimatorController controller, AnimationClip idleClip)
    {
        EnsureFolder(SkinsRoot);
        string path = $"{SkinsRoot}/Skin_{Compact(character)}.asset";
        var skin = LoadOrCreate<CharacterSkinSO>(path);

        Sprite preview = AnimationUtility.GetObjectReferenceCurve(idleClip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"))[0].value as Sprite;

        var serialized = new SerializedObject(skin);
        serialized.FindProperty("_displayName").stringValue = character;
        serialized.FindProperty("_animatorController").objectReferenceValue = controller;
        serialized.FindProperty("_previewSprite").objectReferenceValue = preview;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return skin;
    }

    public static List<PlayerMovementConfigSO> BuildConfigPresets()
    {
        EnsureFolder(DataRoot);
        var configs = new List<PlayerMovementConfigSO>();
        foreach (var preset in Presets)
        {
            string path = $"{DataRoot}/PlayerMovementConfig_{preset.Name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PlayerMovementConfigSO>(path);
            if (existing != null)
            {
                configs.Add(existing);
                continue;
            }

            var config = LoadOrCreate<PlayerMovementConfigSO>(path);
            var serialized = new SerializedObject(config);
            foreach (var pair in preset.Values)
            {
                SerializedProperty property = serialized.FindProperty(pair.Key);
                if (property.propertyType == SerializedPropertyType.Integer) property.intValue = Mathf.RoundToInt(pair.Value);
                else property.floatValue = pair.Value;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            configs.Add(config);
        }
        return configs;
    }

    private static PhysicsMaterial2D BuildPhysicsMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysicsMaterialPath);
        if (material != null) return material;

        // Sin fricción: el jugador no se queda pegado a las paredes y el script controla el frenado.
        material = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(material, PhysicsMaterialPath);
        return material;
    }

    public static GameObject BuildPrefab()
    {
        // Los OnValidate avisan de referencias vacías al añadir cada componente; aquí se asignan justo después.
        bool logEnabled = Debug.unityLogger.logEnabled;
        Debug.unityLogger.logEnabled = false;
        try
        {
            return CreatePrefab();
        }
        finally
        {
            Debug.unityLogger.logEnabled = logEnabled;
        }
    }

    private static GameObject CreatePrefab()
    {
        EnsureFolder(Path.GetDirectoryName(PrefabPath).Replace('\\', '/'));
        CharacterSkinSO skin = AssetDatabase.LoadAssetAtPath<CharacterSkinSO>($"{SkinsRoot}/Skin_{Compact(BaseCharacter)}.asset");
        var config = AssetDatabase.LoadAssetAtPath<PlayerMovementConfigSO>($"{DataRoot}/PlayerMovementConfig_Preciso.asset");

        // El origen del prefab está en los pies: se coloca directamente sobre el suelo.
        var root = new GameObject("Player") { layer = LayerMask.NameToLayer(GameLayers.Physics.Player) };

        var body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.sleepMode = RigidbodySleepMode2D.NeverSleep;

        // 16x26 px con esquinas redondeadas para no engancharse en los bordes de los tiles.
        var box = root.AddComponent<BoxCollider2D>();
        box.edgeRadius = 0.0625f;
        box.size = new Vector2(1f, 1.625f) - Vector2.one * (2f * box.edgeRadius);
        box.offset = new Vector2(0f, 1.625f / 2f);
        box.sharedMaterial = BuildPhysicsMaterial();

        var visual = new GameObject("Visual") { layer = root.layer };
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = new Vector3(0f, 1f, 0f);
        var spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = GameLayers.Sorting.Player;
        spriteRenderer.sprite = skin != null ? skin.PreviewSprite : null;
        var animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = skin != null ? skin.AnimatorController : null;

        var input = root.AddComponent<PlayerInputReader>();
        var movement = root.AddComponent<PlayerMovement>();
        var health = root.AddComponent<PlayerHealth>();
        var playerAnimator = root.AddComponent<PlayerAnimator>();

        InputActionReference[] actions = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath).OfType<InputActionReference>().ToArray();
        SetFields(input,
            ("_moveAction", FindAction(actions, "Player", "Move")),
            ("_jumpAction", FindAction(actions, "Player", "Jump")),
            ("_eventPlayerInputEnabled", LoadEvent("EventPlayerInputEnabled")),
            ("_eventGamePaused", LoadEvent("EventGamePaused")));

        SetFields(movement,
            ("_config", config),
            ("_input", input),
            ("_body", body),
            ("_collider", box));
        SetMask(movement, "_groundLayers", GameLayers.Physics.Ground, GameLayers.Physics.OneWayPlatform);
        SetMask(movement, "_wallLayers", GameLayers.Physics.Ground);

        SetFields(health,
            ("_movement", movement),
            ("_spriteRenderer", spriteRenderer),
            ("_eventPlayerDied", LoadEvent("EventPlayerDied")));

        SetFields(playerAnimator,
            ("_skin", skin),
            ("_animator", animator),
            ("_spriteRenderer", spriteRenderer),
            ("_movement", movement),
            ("_health", health));

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static InputActionReference FindAction(InputActionReference[] actions, string map, string action)
    {
        InputActionReference reference = actions.FirstOrDefault(r => r.action != null && r.action.name == action && r.action.actionMap.name == map);
        if (reference == null) Debug.LogError($"[Jugador] No se encontró la acción {map}/{action} en {InputActionsPath}.");
        return reference;
    }

    private static Object LoadEvent(string assetName)
    {
        Object channel = AssetDatabase.LoadMainAssetAtPath($"{EventsRoot}/{assetName}.asset");
        if (channel == null) Debug.LogError($"[Jugador] Falta el canal {assetName} en {EventsRoot}.");
        return channel;
    }

    private static void SetFields(Object target, params (string Field, Object Value)[] fields)
    {
        var serialized = new SerializedObject(target);
        foreach (var field in fields)
        {
            serialized.FindProperty(field.Field).objectReferenceValue = field.Value;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetMask(Object target, string field, params string[] layers)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).intValue = LayerMask.GetMask(layers);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static int FrameIndex(Sprite sprite)
    {
        int separator = sprite.name.LastIndexOf('_');
        return separator >= 0 && int.TryParse(sprite.name.Substring(separator + 1), out int index) ? index : 0;
    }

    private static string Compact(string character) => character.Replace(" ", "");

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }
}
