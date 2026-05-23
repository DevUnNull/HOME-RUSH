using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid")]
    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;

    public CellData[] grid;

    private void Awake()
    {
        Instance = this;

        grid = new CellData[width * height];
    }

    public int GetIndex(int x, int z)
    {
        return x + (z * width);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int z = Mathf.FloorToInt(worldPos.z / cellSize);

        return new Vector2Int(x, z);
    }

    public int WorldToIndex(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);

        return GetIndex(gridPos.x, gridPos.y);
    }

    public ref CellData GetCellRef(int index)
    {
        return ref grid[index];
    }
}