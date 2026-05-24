using UnityEngine;

public class WallPainter : MonoBehaviour
{
    [Header("Tool")]
    [SerializeField] private ToolController tool;

    [Header("Paint")]
    [SerializeField] private float paintSpeed = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Color lastAppliedColor;

    private bool hasPainted = false;

    private float nextSearchTime = 0f;

    //================================================
    // UPDATE
    //================================================

    void Update()
    {
        // CHỜ PLAYER SPAWN
        if (!TryFindTool())
            return;

        // HOLD CHUỘT TRÁI ĐỂ SƠN
        if (Input.GetMouseButton(0))
        {
            Paint();
        }
    }

    //================================================
    // AUTO FIND TOOL
    //================================================

    bool TryFindTool()
    {
        // ĐÃ CÓ TOOL
        if (tool != null)
            return true;

        // GIẢM TẦN SUẤT SEARCH
        if (Time.time < nextSearchTime)
            return false;

        nextSearchTime = Time.time + 1f;

        tool = FindFirstObjectByType<ToolController>();

        if (tool != null)
        {
            if (debugLogs)
                Debug.Log("ToolController Found!");
        }
        else
        {
            if (debugLogs)
                Debug.Log("Waiting ToolController Spawn...");
        }

        return tool != null;
    }

    //================================================
    // PAINT
    //================================================

    void Paint()
    {
        Debug.Log("PAINT START");

        if (PaintColorManager.Instance == null)
        {
            Debug.LogError("NO COLOR MANAGER");
            return;
        }

        Color selectedColor =
            PaintColorManager.Instance.currentColor;

        //------------------------------------------------
        // HIT GROUP & WALL
        //------------------------------------------------

        if (!tool.TryGetWallGroup(out WallGroup group, out WallVisual hitWall))
        {
            Debug.Log("NO WALL GROUP HIT");
            return;
        }

        Debug.Log("HIT GROUP : " + group.name);
        if (hitWall != null)
        {
            Debug.Log("HIT WALL : " + hitWall.name);
        }

        //------------------------------------------------
        // GET SEQUENCE
        //------------------------------------------------

        WallSequenceController sequence =
            group.sequenceController;

        if (sequence == null)
        {
            Debug.LogError("NO SEQUENCE");
            return;
        }

        Debug.Log("SEQUENCE FOUND");

        //------------------------------------------------
        // REPAINT / RESET LOGIC (IF COLOR IS DIFFERENT)
        //------------------------------------------------

        if (hitWall != null)
        {
            // Check if we can paint this wall
            bool isCurrentWall = (hitWall == sequence.CurrentWall);
            bool isRepaintingCompletedWall = (hitWall.completed && selectedColor != hitWall.currentPaintColor);

            if (!isCurrentWall && !isRepaintingCompletedWall)
            {
                // Cannot paint this wall (not current wall or it is already completed with the selected color)
                return;
            }

            // If it's a completed wall and we want to paint with a different color, reset it first
            if (isRepaintingCompletedWall)
            {
                Debug.Log("Repainting completed wall: " + hitWall.name);
                hitWall.ResetWall();
                sequence.RecalculateCurrentIndex();
            }
            else if (isCurrentWall && hitWall.progress > 0f && selectedColor != hitWall.currentPaintColor)
            {
                // If it's the current wall, in progress, but we paint with a different color, reset it too
                Debug.Log("Changing color on current wall: " + hitWall.name);
                hitWall.ResetWall();
            }
        }

        //------------------------------------------------
        // CURRENT WALL
        //------------------------------------------------

        WallVisual currentWall =
            sequence.CurrentWall;

        if (currentWall == null)
        {
            Debug.LogError("CURRENT WALL NULL");
            return;
        }

        Debug.Log("CURRENT WALL : " + currentWall.name);

        //------------------------------------------------
        // APPLY COLOR
        //------------------------------------------------

        currentWall.SetPaintColor(selectedColor);

        currentWall.AddProgress(
            Time.deltaTime * paintSpeed
        );

        sequence.CheckProgress();

        Debug.Log("PAINT SUCCESS");
    }
}