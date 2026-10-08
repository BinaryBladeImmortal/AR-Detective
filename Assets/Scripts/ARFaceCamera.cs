using UnityEngine;

public class ARFaceCamera : MonoBehaviour
{
    [Header("Camera Facing")]
    public bool faceCamera = true;

    [Tooltip("Enable this for objects attached to a vertical wall.")]
    public bool onVerticalSurface = false;

    [Tooltip("Rotate continuously while the object is active.")]
    public bool continuousRotation = false;

    [Tooltip("Optional camera. If empty, Main Camera is used.")]
    public Camera targetCamera;

    [Tooltip("Fixed model correction applied after yaw-only camera facing.")]
    public Vector3 modelRotationOffset;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (faceCamera)
        {
            FaceCamera();
        }
    }

    private void OnEnable()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (faceCamera)
        {
            FaceCamera();
        }
    }

    private void LateUpdate()
    {
        if (!faceCamera || !continuousRotation)
        {
            return;
        }

        FaceCamera();
    }

    public void FaceCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        Vector3 direction =
            targetCamera.transform.position - transform.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        if (onVerticalSurface)
        {
            // Object is attached to a wall.
            // Keep its top pointing upward while facing the camera.
            transform.rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                ) * Quaternion.Euler(modelRotationOffset);
        }
        else
        {
            // Object is on the floor/table/etc.
            // Only rotate around the vertical axis.
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            transform.rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                ) * Quaternion.Euler(modelRotationOffset);
        }
    }
}