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