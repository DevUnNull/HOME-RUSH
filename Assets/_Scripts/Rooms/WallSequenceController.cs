using Fusion;
using UnityEngine;

public class NetworkWallSequence : NetworkBehaviour
{
    [Networked]
    public int CurrentIndex { get; set; }

    [Networked]
    public bool RoomCompleted { get; set; }

    public NetworkWallVisual[] walls;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentIndex = 0;
            RoomCompleted = false;
        }

        foreach (NetworkWallVisual wall in walls)
        {
            if (wall != null)
                wall.sequenceController = this;
        }
    }

    public NetworkWallVisual CurrentWall
    {
        get
        {
            if (Object == null)
            {
                Debug.LogWarning("NetworkWallSequence.CurrentWall: Object is null (not spawned yet)");
                return null;
            }

            if (CurrentIndex >= walls.Length)
                return null;

            return walls[CurrentIndex];
        }
    }

    public NetworkWallVisual GetWallToErase()
    {
        if (Object == null) return null;

        // Nếu cả phòng đã xong, ta xóa bức tường cuối cùng
        if (RoomCompleted && walls.Length > 0)
        {
            return walls[walls.Length - 1];
        }

        if (CurrentIndex >= walls.Length) return null;

        var currentWall = walls[CurrentIndex];

        // Nếu bức tường hiện tại đã tô được 1 ít (Progress > 0), ta xóa nó
        if (currentWall != null && currentWall.Progress > 0)
        {
            return currentWall;
        }

        // Nếu bức tường hiện tại chưa tô gì (Progress == 0) và không phải bức đầu tiên, ta lùi lại xóa bức trước đó
        if (CurrentIndex > 0)
        {
            return walls[CurrentIndex - 1];
        }

        return currentWall;
    }

    public bool CanPaint(NetworkWallVisual wall)
    {
        return wall == CurrentWall;
    }

    public void CheckProgress()
    {
        if (!Object.HasStateAuthority)
            return;

        RoomCompleted = false;

        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] == null || !walls[i].Completed)
            {
                CurrentIndex = i;
                return;
            }
        }

        CurrentIndex = walls.Length;
        RoomCompleted = true;
    }
}