#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;

public static class Lab03Bootstrap
{
    const string ScenePath = "Assets/Scenes/Level01.unity";
    const string CharSpritePath = "Assets/Art/Characters/player.png";
    const string PhysMatPath = "Assets/Settings/PlatformPhysics.physicsMaterial2D";
    const string AnimatorPath = "Assets/Art/Characters/Player.controller";
    const string MixerPath = "Assets/Audio/GameMixer.mixer";
    const string LegacyMixerPath = "Assets/NewAudioMixer.mixer";

    public static void Run()
    {
        Directory.CreateDirectory("Assets/Art/Characters");
        CopyCharacterSprite();
        ConfigurePlayerSprite();
        CreatePhysicsMaterial();
        var controller = CreateAnimator();
        SetupPlatformPrefabLayer();
        var sfx = SetupAudioMixer();
        SetupScenePlayer(controller, sfx);
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Lab03Bootstrap completed.");
    }

    static void CopyCharacterSprite()
    {
        var src = "/tmp/kenney_pixel/Tiles/Characters/tile_0000.png";
        if (File.Exists(src))
            File.Copy(src, CharSpritePath, true);
    }

    static void ConfigurePlayerSprite()
    {
        if (!File.Exists(CharSpritePath))
            return;
        var importer = AssetImporter.GetAtPath(CharSpritePath) as TextureImporter;
        if (importer == null)
            return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16;
        importer.spritePivot = new Vector2(0.5f, 0f);
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    static void CreatePhysicsMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysMatPath);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, PhysMatPath);
        }
        mat.friction = 0f;
        mat.bounciness = 0f;
        EditorUtility.SetDirty(mat);
    }

    static AnimatorController CreateAnimator()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimatorPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
        if (controller.layers.Length == 0)
            controller.AddLayer("Base");

        var sm = controller.layers[0].stateMachine;
        while (sm.states.Length > 0)
            sm.RemoveState(sm.states[0].state);

        var idleClip = CreateClip("Player_Idle", CharSpritePath);
        var runClip = CreateClip("Player_Run", CharSpritePath);
        var idle = sm.AddState("Idle");
        var run = sm.AddState("Run");
        idle.motion = idleClip;
        run.motion = runClip;
        sm.defaultState = idle;

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var t1 = idle.AddTransition(run);
        t1.hasExitTime = false;
        t1.duration = 0f;
        t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        var t2 = run.AddTransition(idle);
        t2.hasExitTime = false;
        t2.duration = 0f;
        t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        return controller;
    }

    static AnimationClip CreateClip(string name, string spritePath)
    {
        var path = $"Assets/Art/Characters/{name}.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip { frameRate = 10 };
            AssetDatabase.CreateAsset(clip, path);
        }
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null)
        {
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var key = new ObjectReferenceKeyframe { time = 0, value = sprite };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, new[] { key });
        }
        return clip;
    }

    static void SetupPlatformPrefabLayer()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Platform.prefab");
        if (prefab == null)
            return;
        var root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
        root.layer = LayerMask.NameToLayer("Ground");
        var col = root.GetComponent<BoxCollider2D>();
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysMatPath);
        if (col != null && mat != null)
            col.sharedMaterial = mat;
        PrefabUtility.SaveAsPrefabAsset(root, AssetDatabase.GetAssetPath(prefab));
        PrefabUtility.UnloadPrefabContents(root);
    }

    static AudioMixerGroup SetupAudioMixer()
    {
        Directory.CreateDirectory("Assets/Audio");
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        if (mixer == null && AssetDatabase.LoadAssetAtPath<AudioMixer>(LegacyMixerPath) != null)
        {
            AssetDatabase.MoveAsset(LegacyMixerPath, MixerPath);
            mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        }
        if (mixer == null)
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/Audio");
            EditorApplication.ExecuteMenuItem("Assets/Create/Audio/Audio Mixer");
            AssetDatabase.Refresh();
            foreach (var guid in AssetDatabase.FindAssets("t:AudioMixer"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == MixerPath)
                    continue;
                AssetDatabase.MoveAsset(path, MixerPath);
                break;
            }
            mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        }
        if (mixer == null)
            return null;
        return mixer.FindMatchingGroups("Master").Length > 0 ? mixer.FindMatchingGroups("Master")[0] : null;
    }

    static AudioClip[] LoadFootsteps()
    {
        var clips = new AudioClip[3];
        for (var i = 0; i < 3; i++)
            clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/footstep_{i + 1}.wav");
        return clips;
    }

    static void SetupScenePlayer(AnimatorController controller, AudioMixerGroup sfxGroup)
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var p in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include))
            Object.DestroyImmediate(p.gameObject);

        var existing = GameObject.Find("Player");
        if (existing != null)
            Object.DestroyImmediate(existing);

        var rig = GameObject.Find("CameraRig");
        if (rig == null)
            rig = new GameObject("CameraRig");
        rig.transform.position = Vector3.zero;
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.SetParent(rig.transform, false);
            cam.transform.localPosition = Vector3.zero;
            if (cam.GetComponent<CameraShake>() == null)
                cam.gameObject.AddComponent<CameraShake>();
        }

        var gameplay = GameObject.Find("GAMEPLAY");
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CharSpritePath);
        var player = new GameObject("Player");
        player.transform.SetParent(gameplay != null ? gameplay.transform : null, false);
        player.transform.position = new Vector3(-2f, 0f, 0f);
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Player";
        var body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        var cap = player.AddComponent<CapsuleCollider2D>();
        cap.size = new Vector2(0.7f, 1f);
        cap.offset = new Vector2(0f, 0.5f);
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysMatPath);
        cap.sharedMaterial = mat;

        var groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform, false);
        groundCheck.transform.localPosition = new Vector3(0f, -0.5f, 0f);

        var animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        var audio = player.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.loop = false;
        if (sfxGroup != null)
            audio.outputAudioMixerGroup = sfxGroup;

        var pc = player.AddComponent<PlayerController>();
        var so = new SerializedObject(pc);
        so.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        so.FindProperty("groundLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
        var footsteps = so.FindProperty("footstepClips");
        var clips = LoadFootsteps();
        footsteps.arraySize = clips.Length;
        for (var i = 0; i < clips.Length; i++)
            footsteps.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
#endif