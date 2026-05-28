using UnityEngine;

public class RoomValidator : MonoBehaviour
{
    public int roomID;

    public bool IsRoomCompleted()
    {
        CellData[] grid = GridManager.Instance.grid;

        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i].roomID != roomID)
                continue;

            if (!grid[i].isCompleted)
                return false;
        }

        return true;
    }
}