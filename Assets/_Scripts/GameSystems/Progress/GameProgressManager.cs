using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class GameProgressManager : NetworkBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    private List<RoomProgress> rooms = new List<RoomProgress>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Spawned()
    {
        base.Spawned();
        RefreshRooms();
    }

    public void RefreshRooms()
    {
        rooms = FindObjectsOfType<RoomProgress>().ToList();
    }

    public float GetTotalProgress()
    {
        if (rooms.Count == 0) return 0f;

        float total = 0f;
        foreach (var room in rooms)
        {
            total += room.GetRoomProgress();
        }

        return total / rooms.Count;
    }

    public RoomProgress GetRoomByName(string name)
    {
        return rooms.FirstOrDefault(r => r.RoomName == name);
    }
    
    public List<RoomProgress> GetAllRooms()
    {
        return rooms;
    }
}
