using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static string TempPath => Path.Combine(Application.persistentDataPath, "save.tmp");

    public static bool HasSave => File.Exists(SavePath);

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(TempPath, json);
        if (File.Exists(SavePath))
            File.Replace(TempPath, SavePath, null);
        else
            File.Move(TempPath, SavePath);
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
            return new SaveData();
        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            return Migrate(data ?? new SaveData());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Сейв не прочитано: " + e.Message);
            return new SaveData();
        }
    }

    public static void Delete()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    private static SaveData Migrate(SaveData data)
    {
        if (data.version < 2)
        {
            data.levelIndex = 0;
            data.version = 2;
        }
        return data;
    }
}