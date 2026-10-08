using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARFloorDetector : MonoBehaviour
{
    [Header("AR References")]
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;

    [Header("Crime Scene")]
    public GameObject crimeScenePrefab;

    [Header("Floor Detection")]
    [Tooltip("Minimum physical area required for a plane to be considered.")]
    public float minimumPlaneArea = 1.0f;

    [Tooltip("Maximum distance below the camera that can still be considered.")]
    public float maximumFloorDistance = 3.0f;

    [Tooltip("How long a floor candidate must remain stable.")]
    public float stabilityTime = 0.8f;

    [Header("Placement")]
    public bool allowPlacement = true;

    [Tooltip("Prevents accidental multiple placements.")]
    public bool lockAfterPlacement = true;

    [Header("Debug")]
    public bool showDebug = false;

    // Public result
    public ARPlane CurrentFloorPlane { get; private set; }

    public bool FloorDetected
    {
        get { return CurrentFloorPlane != null; }
    }

    public bool CrimeScenePlaced
    {
        get { return placedCrimeScene != null; }
    }

    private ARPlane candidateFloor;
    private float candidateStartTime;

    private GameObject placedCrimeScene;

    private static readonly List<ARRaycastHit> raycastHits =
        new List<ARRaycastHit>();

    void Awake()
    {
        if (planeManager == null)
            planeManager = GetComponent<ARPlaneManager>();

        if (raycastManager == null)
            raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        FindBestFloor();

        if (allowPlacement && !CrimeScenePlaced)
        {
            HandlePlacement();
        }
    }

    // =========================================================
    // FLOOR DETECTION
    // =========================================================

    void FindBestFloor()
    {
        if (planeManager == null)
            return;

        ARPlane bestPlane = null;
        float bestScore = float.MinValue;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (!IsValidFloorCandidate(plane))
                continue;

            float score = CalculateFloorScore(plane);

            if (score > bestScore)
            {
                bestScore = score;
                bestPlane = plane;
            }
        }

        if (bestPlane == null)
        {
            CurrentFloorPlane = null;
            candidateFloor = null;
            candidateStartTime = 0f;
            return;
        }

        // New candidate
        if (candidateFloor != bestPlane)
        {
            candidateFloor = bestPlane;
            candidateStartTime = Time.time;
        }

        // Wait for candidate to remain stable
        if (Time.time - candidateStartTime >= stabilityTime)
        {
            CurrentFloorPlane = bestPlane;
        }
    }

    bool IsValidFloorCandidate(ARPlane plane)
    {
        if (plane == null)
            return false;

        if (plane.trackingState != TrackingState.Tracking)
            return false;

        if (plane.alignment != PlaneAlignment.HorizontalUp)
            return false;

        float area = plane.size.x * plane.size.y;

        if (area < minimumPlaneArea)
            return false;

        // If the device provides an actual floor classification,
        // immediately accept it as a strong candidate.
        if (plane.classification == PlaneClassification.Floor)
            return true;

        // Ignore planes that are unrealistically far below/above
        // the camera for our room-scale use case.
        float verticalDistance =
            Mathf.Abs(
                plane.transform.position.y -
                Camera.main.transform.position.y
            );

        if (verticalDistance > maximumFloorDistance)
            return false;

        return true;
    }

    float CalculateFloorScore(ARPlane plane)
    {
        float score = 0f;

        float area = plane.size.x * plane.size.y;

        // -----------------------------------------------------
        // 1. Large planes are more useful.
        // -----------------------------------------------------

        score += Mathf.Clamp(area, 0f, 20f) * 2f;

        // -----------------------------------------------------
        // 2. Lower horizontal surfaces are more likely to be
        //    the floor than beds/tables.
        // -----------------------------------------------------

        float cameraHeight =
            Camera.main != null
                ? Camera.main.transform.position.y
                : 1.5f;

        float heightDifference =
            cameraHeight -
            plane.transform.position.y;

        // Prefer surfaces below the camera.
        if (heightDifference > 0f)
        {
            score += Mathf.Clamp(
                heightDifference * 10f,
                0f,
                25f
            );
        }

        // -----------------------------------------------------
        // 3. Strong bonus if ARCore explicitly says "Floor".
        // -----------------------------------------------------

        if (plane.classification == PlaneClassification.Floor)
        {
            score += 100f;
        }

        // -----------------------------------------------------
        // 4. Penalize surfaces that are very close to camera
        //    because they are more likely to be beds/tables.
        // -----------------------------------------------------

        if (heightDifference < 0.5f)
        {
            score -= 15f;
        }

        return score;
    }

    // =========================================================
    // PLACEMENT
    // =========================================================

    void HandlePlacement()
    {
        if (crimeScenePrefab == null)
            return;

        if (raycastManager == null)
            return;

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        bool hitSomething =
            raycastManager.Raycast(
                touch.position,
                raycastHits,
                TrackableType.PlaneWithinPolygon
            );

        if (!hitSomething || raycastHits.Count == 0)
            return;

        ARPlane hitPlane =
            raycastHits[0].trackable as ARPlane;

        // -----------------------------------------------------
        // If we know which plane was identified as the floor,
        // don't place the crime scene on an obvious raised
        // horizontal surface.
        // -----------------------------------------------------

        if (CurrentFloorPlane != null &&
            hitPlane != null &&
            hitPlane != CurrentFloorPlane)
        {
            // If the tapped plane is much higher than the
            // detected floor, ignore it.
            float heightDifference =
                Mathf.Abs(
                    hitPlane.transform.position.y -
                    CurrentFloorPlane.transform.position.y
                );

            if (heightDifference > 0.20f)
            {
                if (showDebug)
                {
                    Debug.Log(
                        "Placement ignored: tapped raised surface."
                    );
                }

                return;
            }
        }

        Pose hitPose = raycastHits[0].pose;

        placedCrimeScene =
            Instantiate(
                crimeScenePrefab,
                hitPose.position,
                hitPose.rotation
            );

        if (lockAfterPlacement)
        {
            allowPlacement = false;
        }

        if (showDebug)
        {
            Debug.Log(
                "Crime Scene placed successfully."
            );
        }
    }

    // =========================================================
    // DEBUG
    // =========================================================

    void OnGUI()
    {
        if (!showDebug)
            return;

        GUIStyle style =
            new GUIStyle(GUI.skin.label);

        style.fontSize = 20;
        style.normal.textColor = Color.white;

        string text =
            "AR DETECTIVE DEBUG\n\n";

        text +=
            "Tracked planes: " +
            CountHorizontalPlanes() +
            "\n";

        text +=
            "Floor detected: " +
            FloorDetected +
            "\n";

        if (CurrentFloorPlane != null)
        {
            float area =
                CurrentFloorPlane.size.x *
                CurrentFloorPlane.size.y;

            text +=
                "Floor height: " +
                CurrentFloorPlane.transform.position.y
                    .ToString("F2") +
                "m\n";

            text +=
                "Floor size: " +
                CurrentFloorPlane.size.x
                    .ToString("F2") +
                " x " +
                CurrentFloorPlane.size.y
                    .ToString("F2") +
                "m\n";

            text +=
                "Classification: " +
                CurrentFloorPlane.classification +
                "\n";
        }

        text +=
            "Crime scene placed: " +
            CrimeScenePlaced;

        GUI.Label(
            new Rect(
                20,
                20,
                600,
                250
            ),
            text,
            style
        );
    }

    int CountHorizontalPlanes()
    {
        if (planeManager == null)
            return 0;

        int count = 0;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane.trackingState != TrackingState.Tracking)
                continue;

            if (plane.alignment != PlaneAlignment.HorizontalUp)
                continue;

            count++;
        }

        return count;
    }

    // =========================================================
    // OPTIONAL RESET
    // =========================================================

    public void ResetCrimeScene()
    {
        if (placedCrimeScene != null)
        {
            Destroy(placedCrimeScene);
            placedCrimeScene = null;
        }

        allowPlacement = true;
    }
}