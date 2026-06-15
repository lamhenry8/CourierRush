using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    public WorldSpawner worldSpawner;
    public DeliveryManager deliveryManager;
    public Transform player;

    [Header("Prefabs")]
    public GameObject[] solidObstaclePrefabs;
    public GameObject oilSpillPrefab;

    [Header("Settings")]
    public int solidObstaclesPerChunk = 2;
    public int oilSpillsPerChunk = 1;
    public string roadTilemapName = "Road";
    public float minDistanceBetweenObstacles = 1.5f;
    public float minDistanceFromPlayer = 5f;

    private Dictionary<Vector2Int, List<GameObject>> chunkObstacles = new Dictionary<Vector2Int, List<GameObject>>();
    private float refreshTimer;
    private const float RefreshInterval = 0.5f;

    void Start()
    {
        if (deliveryManager != null)
            deliveryManager.OnRestart += HandleRestart;
    }

    void OnDestroy()
    {
        if (deliveryManager != null)
            deliveryManager.OnRestart -= HandleRestart;
    }

    void Update()
    {
        if (deliveryManager != null && deliveryManager.IsGameOver)
            return;

        refreshTimer += Time.deltaTime;
        if (refreshTimer >= RefreshInterval)
        {
            refreshTimer = 0f;
            RefreshObstacles();
        }
    }

    void RefreshObstacles()
    {
        var activeChunks = worldSpawner.GetActiveChunks();

        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in chunkObstacles)
        {
            if (!activeChunks.ContainsKey(kvp.Key))
            {
                foreach (var obj in kvp.Value)
                    if (obj != null) Destroy(obj);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var coord in toRemove)
            chunkObstacles.Remove(coord);

        foreach (var kvp in activeChunks)
        {
            if (!chunkObstacles.ContainsKey(kvp.Key))
                SpawnObstaclesInChunk(kvp.Key, kvp.Value);
        }
    }

    void SpawnObstaclesInChunk(Vector2Int coord, GameObject chunkObject)
    {
        List<GameObject> spawned = new List<GameObject>();
        chunkObstacles[coord] = spawned;

        List<Vector3> roadPositions = GetRoadPositions(chunkObject);
        if (roadPositions.Count == 0) return;

        Shuffle(roadPositions);

        List<Vector3> selected = new List<Vector3>();
        int totalNeeded = solidObstaclesPerChunk + oilSpillsPerChunk;

        foreach (Vector3 pos in roadPositions)
        {
            if (selected.Count >= totalNeeded) break;
            if (player != null && Vector3.Distance(pos, player.position) < minDistanceFromPlayer) continue;
            if (IsFarEnough(pos, selected))
                selected.Add(pos);
        }

        for (int i = 0; i < selected.Count; i++)
        {
            GameObject prefab = null;
            if (i < solidObstaclesPerChunk && solidObstaclePrefabs != null && solidObstaclePrefabs.Length > 0)
                prefab = solidObstaclePrefabs[Random.Range(0, solidObstaclePrefabs.Length)];
            else if (oilSpillPrefab != null)
                prefab = oilSpillPrefab;

            if (prefab != null)
                spawned.Add(Instantiate(prefab, selected[i], Quaternion.identity));
        }
    }

    List<Vector3> GetRoadPositions(GameObject chunkObject)
    {
        List<Vector3> positions = new List<Vector3>();
        Tilemap tilemap = chunkObject.transform.Find(roadTilemapName)?.GetComponent<Tilemap>();
        if (tilemap == null) return positions;

        foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
            if (tilemap.HasTile(cell))
                positions.Add(tilemap.GetCellCenterWorld(cell));

        return positions;
    }

    bool IsFarEnough(Vector3 pos, List<Vector3> existing)
    {
        foreach (var other in existing)
            if (Vector3.Distance(pos, other) < minDistanceBetweenObstacles)
                return false;
        return true;
    }

    void Shuffle(List<Vector3> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void HandleRestart()
    {
        foreach (var kvp in chunkObstacles)
            foreach (var obj in kvp.Value)
                if (obj != null) Destroy(obj);
        chunkObstacles.Clear();
    }
}
