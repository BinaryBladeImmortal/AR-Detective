using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARSpatialObjectManager : MonoBehaviour
{
    [Header("AR References")]
    public ARPlaneManager planeManager;
    public Camera arCamera;

    [Header("Investigation Objects")]
    public GameObject detectiveBoard;
    public GameObject footprintTrail;
    public GameObject openBox;
    public GameObject suspiciousNote;
    public GameObject secretKey;
    public GameObject lockedBox;
    public GameObject finalClue;

    [Header("Investigation Manager")]
    public InvestigationManager investigationManager;

    [Header("Suspicious Note Locations")]
    public Transform[] suspiciousNoteLocations;

    [Header("Scanning Settings")]
    public float scanDuration = 5f;

    public float horizontalPlaneMinArea = 0.5f;
    public float verticalPlaneMinArea = 0.75f;

    public float minimumRaisedSurfaceHeight = 0.30f;
    public float maximumSurfaceDistance = 6f;

    public float positionStabilityTolerance = 0.15f;

    [Header("Object Activation Distance")]
    public float objectActivationDistance = 1.5f;

    [Header("Footprint Settings")]
    public float footprintSpacing = 0.35f;
    public int minimumFootprints = 4;
    public int maximumFootprints = 20;

    [Header("Scan Status")]
    public bool roomReady = false;
    public bool scanning = false;

    private ARPlane detectedFloor;
    private ARPlane detectedRaisedSurface;
    private ARPlane detectedWall;

    private float scanTimer = 0f;

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (planeManager == null)
        {
            planeManager =
                FindFirstObjectByType<ARPlaneManager>();
        }

        if (arCamera == null)
        {
            arCamera = Camera.main;
        }

        // InvestigationManager controls which
        // investigation object should be visible.
        HideAllInvestigationObjects();

        // Only scan the room here.
        // DO NOT start the investigation here.
        StartRoomScan();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (!scanning)
        {
            return;
        }

        ScanRoom();

        scanTimer += Time.deltaTime;

        if (scanTimer >= scanDuration)
        {
            CompleteRoomScan();
        }
    }

    // ============================================================
    // START ROOM SCAN
    // ============================================================

    private void StartRoomScan()
    {
        scanning = true;
        roomReady = false;
        scanTimer = 0f;

        detectedFloor = null;
        detectedRaisedSurface = null;
        detectedWall = null;

        Debug.Log(
            "AR SCAN: Starting room scan..."
        );
    }

    // ============================================================
    // SCAN ROOM
    // ============================================================

    private void ScanRoom()
    {
        if (planeManager == null)
        {
            return;
        }

        List<ARPlane> planes =
            new List<ARPlane>();

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane == null)
            {
                continue;
            }

            if (plane.trackingState != TrackingState.Tracking)
            {
                continue;
            }

            planes.Add(plane);
        }

        FindFloor(planes);
        FindRaisedSurface(planes);
        FindWall(planes);
    }

    // ============================================================
    // FIND FLOOR
    // ============================================================

    private void FindFloor(List<ARPlane> planes)
    {
        ARPlane bestFloor = null;

        float bestArea = 0f;

        foreach (ARPlane plane in planes)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp)
            {
                continue;
            }

            float area =
                plane.size.x * plane.size.y;

            if (area < horizontalPlaneMinArea)
            {
                continue;
            }

            float height =
                plane.transform.position.y;

            if (arCamera != null)
            {
                float cameraHeight =
                    arCamera.transform.position.y;

                if (height > cameraHeight - 0.15f)
                {
                    continue;
                }
            }

            if (area > bestArea)
            {
                bestArea = area;
                bestFloor = plane;
            }
        }

        if (bestFloor != null)
        {
            detectedFloor = bestFloor;

            Debug.Log(
                "AR SCAN: Floor found: " +
                detectedFloor.name
            );
        }
    }

    // ============================================================
    // FIND RAISED SURFACE
    // ============================================================

    private void FindRaisedSurface(List<ARPlane> planes)
    {
        if (detectedFloor == null)
        {
            return;
        }

        ARPlane bestSurface = null;

        float bestArea = 0f;

        float floorHeight =
            detectedFloor.transform.position.y;

        foreach (ARPlane plane in planes)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp)
            {
                continue;
            }

            if (plane == detectedFloor)
            {
                continue;
            }

            float area =
                plane.size.x * plane.size.y;

            if (area < horizontalPlaneMinArea)
            {
                continue;
            }

            float height =
                plane.transform.position.y;

            float heightDifference =
                height - floorHeight;

            if (heightDifference <
                minimumRaisedSurfaceHeight)
            {
                continue;
            }

            if (arCamera != null)
            {
                float distance =
                    Vector3.Distance(
                        arCamera.transform.position,
                        plane.transform.position
                    );

                if (distance >
                    maximumSurfaceDistance)
                {
                    continue;
                }
            }

            if (area > bestArea)
            {
                bestArea = area;
                bestSurface = plane;
            }
        }

        if (bestSurface != null)
        {
            detectedRaisedSurface = bestSurface;

            Debug.Log(
                "AR SCAN: Raised surface found: " +
                detectedRaisedSurface.name
            );
        }
    }

    // ============================================================
    // FIND WALL
    // ============================================================

    private void FindWall(List<ARPlane> planes)
    {
        ARPlane bestWall = null;

        float bestArea = 0f;

        foreach (ARPlane plane in planes)
        {
            if (plane.alignment != PlaneAlignment.Vertical)
            {
                continue;
            }

            float area =
                plane.size.x * plane.size.y;

            if (area < verticalPlaneMinArea)
            {
                continue;
            }

            if (area > bestArea)
            {
                bestArea = area;
                bestWall = plane;
            }
        }

        if (bestWall != null)
        {
            detectedWall = bestWall;

            Debug.Log(
                "AR SCAN: Wall found: " +
                detectedWall.name
            );
        }
    }

    // ============================================================
    // COMPLETE ROOM SCAN
    // ============================================================

    private void CompleteRoomScan()
    {
        scanning = false;
        roomReady = true;

        Debug.Log(
            "AR SCAN: Room scan completed."
        );

        if (detectedFloor == null)
        {
            Debug.LogWarning(
                "AR SCAN: Floor not found."
            );
        }

        if (detectedRaisedSurface == null)
        {
            Debug.LogWarning(
                "AR SCAN: Raised surface not found."
            );
        }

        if (detectedWall == null)
        {
            Debug.LogWarning(
                "AR SCAN: Wall not found."
            );
        }

        // --------------------------------------------------------
        // PLACE AR OBJECTS
        // --------------------------------------------------------

        PlaceDetectiveBoard();

        PlaceOpenBox();

        PlaceSuspiciousNote();

        PlaceSecretKey();

        PlaceLockedBox();

        PlaceFinalClue();

        // --------------------------------------------------------
        // IMPORTANT
        // --------------------------------------------------------
        // Do NOT start or restart the investigation here.
        //
        // InvestigationManager is now started by:
        //
        // UIManager -> PlayGame()
        //             -> StartInvestigation()
        //
        // This means:
        //
        // App opens
        //      ↓
        // Welcome Screen
        //      ↓
        // User presses Play
        //      ↓
        // Investigation starts
        //      ↓
        // Detective Board appears
        // --------------------------------------------------------

        Debug.Log(
            "AR SCAN: Room ready. Waiting for Play button."
        );
    }

    // ============================================================
    // PLACE DETECTIVE BOARD
    // ============================================================

    private void PlaceDetectiveBoard()
    {
        if (detectiveBoard == null)
        {
            Debug.LogWarning(
                "Detective Board is not assigned."
            );

            return;
        }

        if (detectedWall == null)
        {
            Debug.LogWarning(
                "Cannot place Detective Board because no wall was found."
            );

            return;
        }

        Vector3 position =
            detectedWall.transform.position;

        // Move slightly away from the wall.
        position +=
            detectedWall.transform.forward * 0.04f;

        detectiveBoard.transform.position =
            position;

        // ARFaceCamera handles
        // the final camera-facing rotation.
        ARFaceCamera faceCamera =
            detectiveBoard.GetComponent<ARFaceCamera>();

        if (faceCamera != null)
        {
            faceCamera.FaceCamera();
        }

        Debug.Log(
            "AR: Detective Board placed on detected wall."
        );
    }

    // ============================================================
    // PLACE OPEN BOX
    // ============================================================

    private void PlaceOpenBox()
    {
        if (openBox == null)
        {
            return;
        }

        if (detectedRaisedSurface == null)
        {
            Debug.LogWarning(
                "Cannot place Open Box because no raised surface was found."
            );

            return;
        }

        Vector3 position =
            detectedRaisedSurface.transform.position;

        position +=
            Vector3.up * 0.05f;

        openBox.transform.position =
            position;

        ARFaceCamera faceCamera =
            openBox.GetComponent<ARFaceCamera>();

        if (faceCamera != null)
        {
            faceCamera.FaceCamera();
        }

        Debug.Log(
            "AR: Open Box placed on raised surface."
        );
    }

    // ============================================================
    // PLACE SUSPICIOUS NOTE
    // ============================================================

    private void PlaceSuspiciousNote()
    {
        if (suspiciousNote == null)
        {
            return;
        }

        if (suspiciousNoteLocations == null ||
            suspiciousNoteLocations.Length == 0)
        {
            Debug.LogWarning(
                "No Suspicious Note locations assigned."
            );

            return;
        }

        int randomIndex =
            Random.Range(
                0,
                suspiciousNoteLocations.Length
            );

        Transform selectedLocation =
            suspiciousNoteLocations[randomIndex];

        if (selectedLocation == null)
        {
            return;
        }

        suspiciousNote.transform.position =
            selectedLocation.position;

        suspiciousNote.transform.rotation =
            selectedLocation.rotation;

        Debug.Log(
            "AR: Suspicious Note placed at " +
            selectedLocation.name
        );
    }

    // ============================================================
    // PLACE SECRET KEY
    // ============================================================

    private void PlaceSecretKey()
    {
        if (secretKey == null)
        {
            return;
        }

        if (detectedFloor == null)
        {
            Debug.LogWarning(
                "Cannot place Secret Key because no floor was found."
            );

            return;
        }

        Vector3 position =
            detectedFloor.transform.position;

        if (arCamera != null)
        {
            Vector3 forward =
                arCamera.transform.forward;

            forward.y = 0f;

            if (forward.sqrMagnitude > 0.001f)
            {
                forward.Normalize();

                position +=
                    forward * 2.5f;
            }
        }

        position.y += 0.05f;

        secretKey.transform.position =
            position;

        ARFaceCamera faceCamera =
            secretKey.GetComponent<ARFaceCamera>();

        if (faceCamera != null)
        {
            faceCamera.FaceCamera();
        }

        Debug.Log(
            "AR: Secret Key placed."
        );
    }

    // ============================================================
    // PLACE LOCKED BOX
    // ============================================================

    private void PlaceLockedBox()
    {
        if (lockedBox == null)
        {
            return;
        }

        Vector3 position;

        if (detectedRaisedSurface != null)
        {
            position =
                detectedRaisedSurface.transform.position;

            position +=
                detectedRaisedSurface.transform.forward * 0.8f;

            position.y += 0.05f;
        }
        else if (detectedFloor != null)
        {
            position =
                detectedFloor.transform.position;

            position.y += 0.05f;
        }
        else
        {
            Debug.LogWarning(
                "Cannot place Locked Box because no suitable surface was found."
            );

            return;
        }

        lockedBox.transform.position =
            position;

        ARFaceCamera faceCamera =
            lockedBox.GetComponent<ARFaceCamera>();

        if (faceCamera != null)
        {
            faceCamera.FaceCamera();
        }

        Debug.Log(
            "AR: Locked Box placed."
        );
    }

    // ============================================================
    // PLACE FINAL CLUE
    // ============================================================

    private void PlaceFinalClue()
    {
        if (finalClue == null)
        {
            return;
        }

        if (lockedBox == null)
        {
            return;
        }

        Vector3 position =
            lockedBox.transform.position;

        position +=
            Vector3.up * 0.25f;

        finalClue.transform.position =
            position;

        ARFaceCamera faceCamera =
            finalClue.GetComponent<ARFaceCamera>();

        if (faceCamera != null)
        {
            faceCamera.FaceCamera();
        }

        Debug.Log(
            "AR: Final Clue placed."
        );
    }

    // ============================================================
    // HIDE ALL INVESTIGATION OBJECTS
    // ============================================================

    private void HideAllInvestigationObjects()
    {
        if (detectiveBoard != null)
        {
            detectiveBoard.SetActive(false);
        }

        if (footprintTrail != null)
        {
            footprintTrail.SetActive(false);
        }

        if (openBox != null)
        {
            openBox.SetActive(false);
        }

        if (suspiciousNote != null)
        {
            suspiciousNote.SetActive(false);
        }

        if (secretKey != null)
        {
            secretKey.SetActive(false);
        }

        if (lockedBox != null)
        {
            lockedBox.SetActive(false);
        }

        if (finalClue != null)
        {
            finalClue.SetActive(false);
        }
    }

    // ============================================================
    // PUBLIC RE-SCAN
    // ============================================================

    public void RestartRoomScan()
    {
        HideAllInvestigationObjects();

        StartRoomScan();
    }
}