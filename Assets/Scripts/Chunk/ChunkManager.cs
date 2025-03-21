using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int chunkSize = 10;
    public int viewDistance = 3; // 플레이어 주변 몇 개의 청크를 유지할지

    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    private Vector2Int lastPlayerChunk;
    Vector2Int currentChunk;

    void Start()
    {
        lastPlayerChunk = GetChunkPosition(transform.position);
        UpdateChunks();
    }

    void Update()
    {
        currentChunk = GetChunkPosition(transform.position);
        if (currentChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentChunk;
            UpdateChunks();
        }
    }

    Vector2Int GetChunkPosition(Vector2 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.y / chunkSize)
        );
    }

    void UpdateChunks()
    {
        HashSet<Vector2Int> neededChunks = new ();

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int chunkPos = lastPlayerChunk + new Vector2Int(x, y);
                neededChunks.Add(chunkPos);

                if (!chunks.ContainsKey(chunkPos))
                {
                    LoadChunk(chunkPos);
                }
            }
        }

        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var pos in chunks.Keys)
        {
            if (!neededChunks.Contains(pos))
            {
                toRemove.Add(pos);
            }
        }

        foreach (var pos in toRemove)
        {
            UnloadChunk(pos);
        }
    }

    void LoadChunk(Vector2Int position)
    {
        GameObject chunkObj = Instantiate(chunkPrefab, new Vector3(position.x * chunkSize, position.y * chunkSize, 0), Quaternion.identity);
        Chunk chunkData = new Chunk(position);

        // 저장된 데이터 불러오기
        if (SavedChunks.ContainsKey(position))
        {
            foreach (var objData in SavedChunks[position])
            {
                GameObject obj = Instantiate(Resources.Load<GameObject>(objData.prefabName));
                obj.transform.position = objData.position;
                chunkData.objects.Add(objData);
            }
        }

        chunks[position] = chunkData;
    }

    void UnloadChunk(Vector2Int position)
    {
        if (chunks.ContainsKey(position))
        {
            SavedChunks[position] = chunks[position].objects; // 청크 오브젝트 데이터 저장
            chunks.Remove(position);
        }
    }

    private Dictionary<Vector2Int, List<ObjectData>> SavedChunks = new Dictionary<Vector2Int, List<ObjectData>>();
}
