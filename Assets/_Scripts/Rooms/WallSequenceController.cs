using UnityEngine;

public class WallSequenceController : MonoBehaviour
{
    public WallVisual[] walls;

    int currentIndex = 0;

    public bool roomCompleted;
    void Awake()
    {
        foreach (WallVisual wall in walls)
        {
            wall.sequenceController = this;
        }

        // Auto-attach WallGroup component if not present in the scene
        if (GetComponent<WallGroup>() == null)
        {
            WallGroup group = gameObject.AddComponent<WallGroup>();
            group.sequenceController = this;
        }
    }
    public WallVisual CurrentWall
    {
        get
        {
            if (currentIndex >= walls.Length)
                return null;

            return walls[currentIndex];
        }
    }

    public bool CanPaint(WallVisual wall)
    {
        return wall == CurrentWall;
    }

    public void RecalculateCurrentIndex()
    {
        currentIndex = 0;
        roomCompleted = false;

        for (int i = 0; i < walls.Length; i++)
        {
            if (!walls[i].completed)
            {
                currentIndex = i;
                return;
            }
        }

        currentIndex = walls.Length;
        roomCompleted = true;
        Debug.Log("ROOM COMPLETE");
    }

    public void CheckProgress()
    {
        RecalculateCurrentIndex();
    }

    // RESET TOÀN BỘ
    public void ResetSequence()
    {
        foreach (WallVisual wall in walls)
        {
            wall.ResetWall();
        }

        RecalculateCurrentIndex();

        Debug.Log("SEQUENCE RESET");
    }
}