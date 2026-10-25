using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonLoader
{
    public static Dictionary<string, T> LoadDictionary<T>(
        string json,
        Func<T, string> getId,
        string tableName
    )
        where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException($"[JsonLoader] {tableName}: json is empty");

        List<T> rows;
        try
        {
            rows = JsonConvert.DeserializeObject<List<T>>(json);
        }
        catch (Exception e)
        {
            throw new Exception($"[JsonLoader] {tableName}: deserialize failed: {e.Message}", e);
        }

        if (rows == null)
            throw new Exception($"[JsonLoader] {tableName}: rows null");

        var dict = new Dictionary<string, T>(rows.Count);

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row == null) continue;

            var id = getId(row);
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"[JsonLoader] {tableName}: empty id at {i}");
                continue;
            }

            if (dict.ContainsKey(id))
            {
                 throw new Exception($"[JsonLoader] {tableName}: duplicated id '{id}'");
            }

            dict.Add(id, row);
        }

        return dict;
    }
    // Resources 폴더에서 로드 (확장자 제외 경로)
    public static List<T> LoadFromResources<T>(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
            throw new FileNotFoundException($"[JsonLoader] Resources not found: {path}");

        try
        {
            return JsonConvert.DeserializeObject<List<T>>(textAsset.text);
        }
        catch (Exception e)
        {
            throw new Exception($"[JsonLoader] {path}: deserialize failed: {e.Message}", e);
        }
    }
    public static string LoadText(string resourcePath)
    {
        var ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null)
        {
            Debug.LogError($"[JsonLoader] Asset not found: Resources/{resourcePath}.json");
            return null;
        }
        return ta.text;
    }
}
