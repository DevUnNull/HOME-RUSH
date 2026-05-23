using UnityEngine;

public class WallPainter : MonoBehaviour
{
    public float paintSpeed = 1f;

    public WallSequenceController sequenceController;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Paint();
        }
    }

    void Paint()
    {
        WallVisual currentWall = sequenceController.CurrentWall;

        if (currentWall == null)
            return;

        currentWall.AddProgress(Time.deltaTime * paintSpeed);

        sequenceController.CheckProgress();
    }
}