using UnityEngine;

[System.Serializable]
public struct CellData
{
    public int roomID;

    public float progress;

    public bool isCompleted;

    public int targetMaterialID;

    public int currentMaterialID;
}