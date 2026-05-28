using UnityEngine;

public class FloorPainter : MonoBehaviour
{
    public ToolController tool;

    public float paintSpeed = 1f;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Paint();
        }
    }

    void Paint()
    {
        if (!tool.TryGetHit(out RaycastHit hit))
            return;

        int index = GridManager.Instance.WorldToIndex(hit.point);

        ref CellData cell = ref GridManager.Instance.GetCellRef(index);

        if (cell.targetMaterialID != tool.materialID)
            return;

        cell.progress += Time.deltaTime * paintSpeed;

        cell.progress = Mathf.Clamp01(cell.progress);

        if (cell.progress >= 1f)
        {
            cell.isCompleted = true;
        }

        Debug.Log(cell.progress);
    }
}