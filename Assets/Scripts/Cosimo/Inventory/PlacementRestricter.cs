using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementRestricter : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Collider2D _collder;

    public Tilemap Tilemap => _tilemap;
    public Collider2D Collider2D => _collder;

    private List<Vector3Int> _validCoveredCells = new();

    void Start()
    {
        if (_tilemap == null)
        {
            Debug.LogWarning("Unassigned Tilemap: " + gameObject.name);
            return;
        }
        var cells = GetCoveredCells(_collder, _tilemap);
        _validCoveredCells.Clear();

        for (int i = 0; i < cells.Count; i++)
        {
            TileBase tile = _tilemap.GetTile(cells[i]);
            if (tile == null)
            {
                continue;
            }
            _validCoveredCells.Add(cells[i]);
        }

        for (int i = 0; i < _validCoveredCells.Count; i++)
        {
            PlacementManager.Instance.SetCellRestriction(_tilemap, _validCoveredCells[i], true);
        }
    }

    void OnEnable()
    {
        if (_tilemap == null)
        {
            Debug.LogWarning("Unassigned Tilemap: " + gameObject.name + " using PlacementManager's");
            _tilemap = PlacementManager.Instance.TargetTilemap;
        }
        var cells = GetCoveredCells(_collder, _tilemap);
        _validCoveredCells.Clear();

        for (int i = 0; i < cells.Count; i++)
        {
            TileBase tile = _tilemap.GetTile(cells[i]);
            if (tile == null)
            {
                continue;
            }
            _validCoveredCells.Add(cells[i]);
        }

        for (int i = 0; i < _validCoveredCells.Count; i++)
        {
            PlacementManager.Instance.SetCellRestriction(_tilemap, _validCoveredCells[i], true);
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < _validCoveredCells.Count; i++)
        {
            PlacementManager.Instance.SetCellRestriction(_tilemap, _validCoveredCells[i], false);
        }
    }

    public static List<Vector3Int> GetCoveredCells(Collider2D collider, Tilemap tilemap)
    {
        if (collider == null || tilemap == null)
            return new List<Vector3Int>();

        HashSet<Vector3Int> coveredCells = new HashSet<Vector3Int>();
        Bounds bounds = collider.bounds;

        Vector3Int minCell = tilemap.WorldToCell(bounds.min);
        Vector3Int maxCell = tilemap.WorldToCell(bounds.max);

        // Pre-calculate half the cell size
        Vector3 halfSize = tilemap.cellSize * 0.5f;

        // The 9 sample offsets relative to the cell center: 
        // (0,0) = center, 4 corners, and 4 edge midpoints
        Vector3[] sampleOffsets = new Vector3[]
        {
        new Vector3( 0,  0, 0), // Center
        new Vector3(-halfSize.x, -halfSize.y, 0), // Bottom-left
        new Vector3( halfSize.x, -halfSize.y, 0), // Bottom-right
        new Vector3(-halfSize.x,  halfSize.y, 0), // Top-left
        new Vector3( halfSize.x,  halfSize.y, 0), // Top-right
        new Vector3(-halfSize.x,  0, 0), // Left edge
        new Vector3( halfSize.x,  0, 0), // Right edge
        new Vector3( 0, -halfSize.y, 0), // Bottom edge
        new Vector3( 0,  halfSize.y, 0)  // Top edge
        };

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPos);

                bool isCovered = false;
                foreach (Vector3 offset in sampleOffsets)
                {
                    Vector3 samplePoint = cellCenter + offset;
                    if (collider.OverlapPoint(samplePoint))
                    {
                        isCovered = true;
                        break;
                    }
                }

                if (isCovered)
                    coveredCells.Add(cellPos);
            }
        }

        return new List<Vector3Int>(coveredCells);
    }

    void OnDrawGizmos()
    {
        // if in editor mode recalculate grid
        if (!Application.isPlaying)
        {
            var cells = GetCoveredCells(_collder, _tilemap);
            _validCoveredCells.Clear();

            for (int i = 0; i < cells.Count; i++)
            {
                TileBase tile = _tilemap.GetTile(cells[i]);
                if (tile == null)
                {
                    continue;
                }
                _validCoveredCells.Add(cells[i]);
            }
        }
        for (int i = 0; i < _validCoveredCells.Count; i++)
        {
            Gizmos.color = new(1f, 0, 0, 0.4f);
            Gizmos.DrawCube(_validCoveredCells[i] + new Vector3(0.5f, 0.5f, 0f), Vector3Int.one - new Vector3(0.1f, 0.1f, 0));
        }
    }
}
