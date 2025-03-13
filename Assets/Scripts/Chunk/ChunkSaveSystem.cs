using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ChunkSaveSystem
{
    public static void SaveChunks(Dictionary<Vector2Int, List<ObjectData>> chunkData)
    {
        string json = JsonUtility.ToJson(chunkData);
        File.WriteAllText(Application.persistentDataPath + "/chunks.json", json);
    }

    public static Dictionary<Vector2Int, List<ObjectData>> LoadChunks()
    {
        string path = Application.persistentDataPath + "/chunks.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<Dictionary<Vector2Int, List<ObjectData>>>(json);
        }
        return new Dictionary<Vector2Int, List<ObjectData>>();
    }
}