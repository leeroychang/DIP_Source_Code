using UnityEngine;
using UnityEngine.Tilemaps;

public class TreasureSpawnScript : MonoBehaviour
{
    [SerializeField] Tilemap treasuremap;
    [SerializeField] Transform treasureHolder;
    [SerializeField] GameObject treasurePrefab;

    void Start()
    {
        if (treasuremap==null) return;

        treasuremap.CompressBounds();
        BoundsInt bounds = treasuremap.cellBounds;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = treasuremap.GetTile(cellPos);
            if (tile==null) continue;

            Vector3 worldPos = new Vector3(
                cellPos.x * treasuremap.cellSize.x,
                cellPos.y * treasuremap.cellSize.y,
                cellPos.z * treasuremap.cellSize.z);

            Instantiate(treasurePrefab,treasureHolder);
            treasurePrefab.transform.position = worldPos;
        }
    }
}
