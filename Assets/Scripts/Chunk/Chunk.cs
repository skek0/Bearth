using UnityEngine;
using System.Collections.Generic;

public class Chunk
{
    public Vector2Int position;
    public List<ObjectData> objects;

    public Chunk(Vector2Int _position)
    {
        position = _position;
        objects = new List<ObjectData>();
    }
}

[System.Serializable]
public class ObjectData
{
    public Vector2 position;
    public string prefabName;

    public ObjectData(GameObject obj)
    {
        position = obj.transform.position;
        prefabName = obj.name;
    }

}