using Fusion;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    [Networked, OnChangedRender(nameof(OnDirectionChanged))]
    public CameraDirection CurrentCameraDirection { get; set; } = CameraDirection.Up;

    [SerializeField] private CinemachineCamera up;
    [SerializeField] private CinemachineCamera left;
    [SerializeField] private CinemachineCamera down;
    [SerializeField] private CinemachineCamera right;

    public void ChangeCameraDirection(Rotation rotation)
    {
        if (rotation == Rotation.Left)
        {
            if (CurrentCameraDirection == CameraDirection.Up)
            {
                CurrentCameraDirection = CameraDirection.Left;
            }
            else if (CurrentCameraDirection == CameraDirection.Left)
            {
                CurrentCameraDirection = CameraDirection.Down;
            }
            else if (CurrentCameraDirection == CameraDirection.Down)
            {
                CurrentCameraDirection = CameraDirection.Right;
            }
            else if (CurrentCameraDirection == CameraDirection.Right)
            {
                CurrentCameraDirection = CameraDirection.Up;
            }
            return;
        }

        if (CurrentCameraDirection == CameraDirection.Up)
        {
            CurrentCameraDirection = CameraDirection.Right;
        }
        else if (CurrentCameraDirection == CameraDirection.Right)
        {
            CurrentCameraDirection = CameraDirection.Down;
        }
        else if (CurrentCameraDirection == CameraDirection.Down)
        {
            CurrentCameraDirection = CameraDirection.Left;
        }
        else if (CurrentCameraDirection == CameraDirection.Left)
        {
            CurrentCameraDirection = CameraDirection.Up;
        }
    }

    private void OnDirectionChanged()
    {
        if (CurrentCameraDirection == CameraDirection.Up)
        {
            TurnOffAllCameras();
            up.gameObject.SetActive(true);
        }
        else if (CurrentCameraDirection == CameraDirection.Left)
        {
            TurnOffAllCameras();
            left.gameObject.SetActive(true);
        }
        else if (CurrentCameraDirection == CameraDirection.Down)
        {
            TurnOffAllCameras();
            down.gameObject.SetActive(true);
        }
        else if (CurrentCameraDirection == CameraDirection.Right)
        {
            TurnOffAllCameras();
            right.gameObject.SetActive(true);
        }
    }

    private void TurnOffAllCameras()
    {
        up.gameObject.SetActive(false);
        left.gameObject.SetActive(false);
        down.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
    }
}

public enum Rotation
{
    Left,
    Right
}

public enum CameraDirection
{
    Up,
    Down,
    Left,
    Right
}
