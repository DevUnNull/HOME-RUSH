using UnityEngine;

public class WallSequenceController : MonoBehaviour
{
    public WallVisual[] walls;

    int currentIndex = 0;

    public bool roomCompleted;

    public WallVisual CurrentWall
    {
        get
        {
            if (currentIndex >= walls.Length)
                return null;

            return walls[currentIndex];
        }
    }

    public bool CanPaint(WallVisual floor)
    {
        return floor == CurrentWall;
    }

    public void CheckProgress()
    {
        if (CurrentWall == null)
            return;

        if (CurrentWall.completed)
        {
            currentIndex++;

            Debug.Log("NEXT WALL");

            if (currentIndex >= walls.Length)
            {
                roomCompleted = true;

                Debug.Log("ROOM COMPLETE");
            }
        }
    }
}