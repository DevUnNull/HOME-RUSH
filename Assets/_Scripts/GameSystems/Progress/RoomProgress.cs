using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class RoomProgress : NetworkBehaviour
{
    [Networked]
    public Color RequiredColor { get; set; }

    [SerializeField] private string roomName = "BadRoom";
    public string RoomName => roomName;

    private List<NetworkWallVisual> walls = new List<NetworkWallVisual>();

    public override void Spawned()
    {
        base.Spawned();
        // Automatically find all walls in this room
        walls.AddRange(GetComponentsInChildren<NetworkWallVisual>());
    }

    public float GetRoomProgress()
    {
        if (walls.Count == 0) return 0f;

        float totalProgress = 0f;
        float tolerance = 0.05f; // Tolerance for color comparison due to floating point precision

        foreach (var wall in walls)
        {
            bool isColorMatch = Mathf.Abs(wall.PaintColor.r - RequiredColor.r) < tolerance &&
                                Mathf.Abs(wall.PaintColor.g - RequiredColor.g) < tolerance &&
                                Mathf.Abs(wall.PaintColor.b - RequiredColor.b) < tolerance;

            if (isColorMatch)
            {
                totalProgress += wall.Progress;
            }
        }

        return totalProgress / walls.Count;
    }
}
