using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    public WorldSpawner worldSpawner;

    [Header("Prefabs")]
    public GameObject pickupPrefab;
    public GameObject deliveryPrefab;

    [Header("Tilemap Names (must match child object names in each chunk)")]
    public string roadTilemapName = "Road";
    public string grassTilemapName = "Grass";

    private GameObject currentTargetObject;

    public GameObject CurrentTargetObject => currentTargetObject;

    public GameObject SpawnPickupTarget()
    {
        return SpawnTarget(roadTilemapName, pickupPrefab);
    }

    public GameObject SpawnDeliveryTarget()
    {
        return SpawnTarget(grassTilemapName, deliveryPrefab);
    }

    private GameObject SpawnTarget(string tilemapName, GameObject prefab)
    {
        DespawnCurrentTarget();

        List<Vector3> candidates = GetValidPositions(tilemapName);

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"TargetSpawner: no valid spawn positions found on tilemap '{tilemapName}'.");
            return null;
        }

        Vector3 spawnPos = candidates[Random.Range(0, candidates.Count)];
        currentTargetObject = Instantiate(prefab, spawnPos, Quaternion.identity);
        currentTargetObject.SetActive(true);

        return currentTargetObject;
    }

    private List<Vector3> GetValidPositions(string tilemapName)
    {
        List<Vector3> candidates = new List<Vector3>();

        foreach (var kvp in worldSpawner.GetActiveChunks())
        {
            GameObject chunkObject = kvp.Value;
            if (chunkObject == null) continue;

            Tilemap tilemap = chunkObject.transform.Find(tilemapName)?.GetComponent<Tilemap>();
            if (tilemap == null) continue;

            foreach (Vector3Int cellPosition in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(cellPosition))
                    candidates.Add(tilemap.GetCellCenterWorld(cellPosition));
            }
        }

        return candidates;
    }

    public void DespawnCurrentTarget()
    {
        if (currentTargetObject != null)
        {
            Destroy(currentTargetObject);
            currentTargetObject = null;
        }
    }

    public bool IsPositionInActiveChunk(Vector3 worldPosition)
    {
        Vector2Int coord = worldSpawner.WorldToGrid(worldPosition);
        return worldSpawner.IsChunkActive(coord);
    }
}
