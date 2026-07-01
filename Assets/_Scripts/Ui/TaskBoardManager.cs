using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class TaskBoardManager : MonoBehaviour
{
    public static TaskBoardManager Instance { get; private set; }

    [SerializeField] private GameObject taskBoardUI;

    private Camera mainCamera;
    private Camera overlayCamera;
    private Volume postProcessVolume;

    private int highlightLayer;
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
    private GameObject badRoom;

    private void Awake()
    {
        if (taskBoardUI != null) taskBoardUI.SetActive(false);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        highlightLayer = LayerMask.NameToLayer("Highlight");
        if (highlightLayer == -1)
        {
            Debug.LogError("Highlight layer not found! Ensure it is created in TagManager.");
            return;
        }
        
        // 1. Store original layers of BadRoom and its children
        badRoom = GameObject.Find("BadRoom");
        if (badRoom != null)
        {
            StoreOriginalLayers(badRoom);
        }
        else
        {
            Debug.LogWarning("BadRoom not found in the scene.");
        }

        // 2. Setup Main Camera
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // We KEEP the Highlight layer in Main Camera's culling mask 
            // so that the room doesn't disappear when the task board is closed.

            // 3. Setup Overlay Camera
            GameObject overlayCamObj = new GameObject("OverlayCamera");
            overlayCamObj.transform.SetParent(mainCamera.transform, false);
            overlayCamera = overlayCamObj.AddComponent<Camera>();
            
            overlayCamera.clearFlags = CameraClearFlags.Depth;
            overlayCamera.cullingMask = 1 << highlightLayer;
            overlayCamera.depth = mainCamera.depth + 1; // render after main
            
            // For URP, set Render Type to Overlay
            var overlayCamData = overlayCamObj.AddComponent<UniversalAdditionalCameraData>();
            overlayCamData.renderType = CameraRenderType.Overlay;
            
            var mainCamData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            if (mainCamData != null)
            {
                mainCamData.cameraStack.Add(overlayCamera);
            }
            
            overlayCamObj.SetActive(false);

            // 4. Setup Post Processing Volume
            GameObject volumeObj = new GameObject("TaskBoardVolume");
            volumeObj.transform.SetParent(transform);
            postProcessVolume = volumeObj.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 100;
            
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>();
            colorAdjustments.postExposure.Override(-3f); // Darken the scene
            
            postProcessVolume.profile = profile;
            volumeObj.SetActive(false);
        }
    }

    private void StoreOriginalLayers(GameObject obj)
    {
        if (obj == null) return;
        originalLayers[obj] = obj.layer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            StoreOriginalLayers(child.gameObject);
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void RestoreOriginalLayers()
    {
        foreach (var kvp in originalLayers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.layer = kvp.Value;
            }
        }
    }

    public void ToggleTaskBoard()
    {
        if (taskBoardUI != null)
        {
            bool isActive = !taskBoardUI.activeSelf;
            taskBoardUI.SetActive(isActive);
            
            if (isActive)
            {
                if (badRoom != null)
                {
                    SetLayerRecursively(badRoom, highlightLayer);
                }
            }
            else
            {
                RestoreOriginalLayers();
            }

            if (overlayCamera != null)
            {
                overlayCamera.gameObject.SetActive(isActive);
            }
            
            if (postProcessVolume != null)
            {
                postProcessVolume.gameObject.SetActive(isActive);
            }
        }
    }

    public void CloseTaskBoard()
    {
        if (taskBoardUI != null && taskBoardUI.activeSelf)
        {
            ToggleTaskBoard();
        }
    }
}
