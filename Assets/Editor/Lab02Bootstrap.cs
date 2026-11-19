#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class Lab02Bootstrap
{
    const string ScenePath = "Assets/Scenes/Level01.unity";
    const string PlatformSpritePath = "Assets/Art/Sprites/platform_tile.png";
    const string BgSpritePath = "Assets/Art/Sprites/bg_tile.png";
    const string PrefabPath = "Assets/Prefabs/Platform.prefab";

    public static void Run()
    {
        ConfigureSprite(PlatformSpritePath);
        ConfigureSprite(BgSpritePath);
        ConfigureTilemapSheet("Assets/Art/Tilesets/tilemap.png");
        AssetDatabase.Refresh();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        SetupCamera();
        EnsureRoots();
        CreatePlatforms(GameObject.Find("GAMEPLAY").transform);
        CreateBackground(null);
        CreateLoggers(GameObject.Find("SYSTEMS").transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Lab02Bootstrap completed.");
    }

    static void ConfigureSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    static void ConfigureTilemapSheet(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = HexToColor("2E3440");
        cam.clearFlags = CameraClearFlags.SolidColor;

        var urp = cam.GetComponent<UniversalAdditionalCameraData>();
        if (urp == null)
            urp = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

    }

    static Transform EnsureRoots()
    {
        var env = GameObject.Find("ENV") ?? new GameObject("ENV");
        var gameplay = GameObject.Find("GAMEPLAY") ?? new GameObject("GAMEPLAY");
        var systems = GameObject.Find("SYSTEMS") ?? new GameObject("SYSTEMS");
        env.transform.position = Vector3.zero;
        gameplay.transform.position = Vector3.zero;
        systems.transform.position = Vector3.zero;
        return gameplay.transform;
    }

    static void CreatePlatforms(Transform gameplay)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlatformSpritePath);
        GameObject prefab = null;
        if (File.Exists(PrefabPath))
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        if (prefab == null)
        {
            var platform = new GameObject("Platform");
            var sr = platform.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "Environment";
            sr.sortingOrder = 0;
            var col = platform.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            PrefabUtility.SaveAsPrefabAsset(platform, PrefabPath);
            Object.DestroyImmediate(platform);
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        foreach (Transform child in gameplay)
        {
            if (child.name.StartsWith("Platform"))
                Object.DestroyImmediate(child.gameObject);
        }

        Vector2[] positions =
        {
            new(-4, -2), new(-3, -2), new(-2, -2), new(-1, -2), new(0, -2), new(1, -2), new(2, -2), new(3, -2),
            new(-1, 0), new(0, 0), new(1, 0),
            new(2, 2), new(3, 2),
            new(-3, 1),
        };

        foreach (var pos in positions)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, gameplay);
            instance.name = "Platform";
            instance.transform.position = new Vector3(pos.x, pos.y, 0);
        }
    }

    static void CreateBackground(Transform _)
    {
        var env = GameObject.Find("ENV").transform;
        foreach (Transform child in env)
        {
            if (child.name == "Background")
                Object.DestroyImmediate(child.gameObject);
        }
        var bg = new GameObject("Background");
        bg.transform.SetParent(env, false);
        bg.transform.position = new Vector3(0, 1, 0);
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgSpritePath);
        sr.sortingLayerName = "Background";
        sr.sortingOrder = 0;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(20, 12);
    }

    static void CreateLoggers(Transform gameplay)
    {
        var systems = GameObject.Find("SYSTEMS").transform;
        CreateLogger(systems, "LoggerA", "A");
        CreateLogger(systems, "LoggerB", "B");
    }

    static void CreateLogger(Transform parent, string name, string label)
    {
        var existing = parent.Find(name);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var logger = go.AddComponent<LifecycleLogger>();
        var so = new SerializedObject(logger);
        so.FindProperty("label").stringValue = label;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out var c))
            return c;
        return Color.black;
    }
}
#endif