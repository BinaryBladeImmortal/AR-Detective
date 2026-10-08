using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ARTapPlacementManager : MonoBehaviour
{
    [Header("AR References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Investigation Manager")]
    [SerializeField] private InvestigationManager investigationManager;

    [Header("Investigation Objects")]
    [SerializeField] private GameObject detectiveBoard;
    [SerializeField] private GameObject openBox;
    [SerializeField] private GameObject suspiciousNote;
    [SerializeField] private GameObject secretKey;
    [SerializeField] private GameObject lockedBox;
    [SerializeField] private GameObject finalClue;

    [Header("Tap Settings")]
    [SerializeField] private float objectTapRadius = 600f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showOnScreenDiagnostics = true;

    private GameObject currentObject;

    private bool objectPlaced = false;
    private bool placementAllowed = false;

    private InvestigationManager.InvestigationStage preparedStage =
        InvestigationManager.InvestigationStage.DetectiveBoard;

    private readonly List<ARRaycastHit> raycastHits =
        new List<ARRaycastHit>();

    private string lastAction = "Initializing...";
    private string lastError = "";
    private string lastPlaneInfo = "No plane yet.";

    private bool planeEventsSubscribed = false;

    private void Awake()
    {
        FindReferences();

        Log("ARTapPlacementManager AWAKE.");
    }

    private void Start()
    {
        FindReferences();

        SubscribeToPlaneEvents();

        ResetPlacementState();

        PrepareForStage(
            InvestigationManager.InvestigationStage.DetectiveBoard
        );

        Log("ARTapPlacementManager STARTED.");
    }

    private void Update()
    {
        FindReferencesIfMissing();

        SubscribeToPlaneEventsIfNeeded();

        SyncInvestigationStage();

        if (!TryGetScreenTap(out Vector2 screenPosition))
        {
            return;
        }

        if (IsPointerOverUI(screenPosition))
        {
            return;
        }

        HandleScreenTap(screenPosition);
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            return false;
        }

        PointerEventData pointerEventData =
            new PointerEventData(eventSystem)
            {
                position = screenPosition
            };

        List<RaycastResult> raycastResults =
            new List<RaycastResult>();

        eventSystem.RaycastAll(
            pointerEventData,
            raycastResults
        );

        foreach (RaycastResult raycastResult in raycastResults)
        {
            if (raycastResult.gameObject != null &&
                raycastResult.gameObject.GetComponentInParent<Selectable>() != null)
            {
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // REFERENCE FINDING
    // ============================================================

    private void FindReferences()
    {
        if (raycastManager == null)
        {
            raycastManager =
                FindFirstObjectByType<ARRaycastManager>();
        }

        if (planeManager == null)
        {
            planeManager =
                FindFirstObjectByType<ARPlaneManager>();
        }

        if (investigationManager == null)
        {
            investigationManager =
                FindFirstObjectByType<InvestigationManager>();
        }
    }

    private void FindReferencesIfMissing()
    {
        if (raycastManager == null ||
            planeManager == null ||
            investigationManager == null)
        {
            FindReferences();
        }
    }

    // ============================================================
    // PLANE EVENT SUBSCRIPTION
    // ============================================================

    private void SubscribeToPlaneEventsIfNeeded()
    {
        if (!planeEventsSubscribed &&
            planeManager != null)
        {
            SubscribeToPlaneEvents();
        }
    }

    private void SubscribeToPlaneEvents()
    {
        if (planeManager == null)
        {
            return;
        }

        if (planeEventsSubscribed)
        {
            return;
        }

        planeManager.planesChanged +=
            OnPlanesChanged;

        planeEventsSubscribed = true;

        Log("AR plane events subscribed.");
    }

    private void UnsubscribeFromPlaneEvents()
    {
        if (planeManager == null)
        {
            return;
        }

        if (!planeEventsSubscribed)
        {
            return;
        }

        planeManager.planesChanged -=
            OnPlanesChanged;

        planeEventsSubscribed = false;

        Log("AR plane events unsubscribed.");
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlaneEvents();
    }

    // ============================================================
    // STAGE SYNCHRONIZATION
    // ============================================================

    private void SyncInvestigationStage()
    {
        if (investigationManager == null)
        {
            return;
        }

        InvestigationManager.InvestigationStage managerStage =
            investigationManager.currentStage;

        if (managerStage != preparedStage)
        {
            PrepareForStage(managerStage);
        }
    }

    // ============================================================
    // STAGE PREPARATION
    // ============================================================

    public void PrepareForStage(
        InvestigationManager.InvestigationStage newStage
    )
    {
        preparedStage = newStage;

        objectPlaced = false;
        placementAllowed = false;

        currentObject = null;

        lastError = "";

        DisableAllInvestigationObjects();

        // --------------------------------------------------------
        // CASE SOLVED
        // --------------------------------------------------------

        if (newStage ==
            InvestigationManager.InvestigationStage.CaseSolved)
        {
            ShowAllPlanes();

            lastAction =
                "CASE SOLVED - no AR placement required.";

            Log(lastAction);

            return;
        }

        // --------------------------------------------------------
        // GET OBJECT FOR CURRENT STAGE
        // --------------------------------------------------------

        currentObject =
            GetObjectForStage(newStage);

        // --------------------------------------------------------
        // SUSPICIOUS NOTE
        // --------------------------------------------------------

        if (newStage ==
            InvestigationManager.InvestigationStage.SuspiciousNote)
        {
            placementAllowed = false;

            ShowAllPlanes();

            lastAction =
                "Suspicious Note ready at predefined location.";

            Log(lastAction);

            return;
        }

        // --------------------------------------------------------
        // NORMAL AR PLACEMENT STAGE
        // --------------------------------------------------------

        if (currentObject != null)
        {
            placementAllowed = true;

            ShowAllPlanes();

            lastAction =
                "READY: " +
                newStage +
                ". TAP ON A DETECTED SURFACE.";

            Log(lastAction);
        }
        else
        {
            lastError =
                "No object assigned for stage: " +
                newStage;

            LogError(lastError);
        }
    }

    // ============================================================
    // GET OBJECT FOR STAGE
    // ============================================================

    private GameObject GetObjectForStage(
        InvestigationManager.InvestigationStage stage
    )
    {
        switch (stage)
        {
            case InvestigationManager.InvestigationStage.DetectiveBoard:

                return detectiveBoard;

            case InvestigationManager.InvestigationStage.OpenBox:

                return openBox;

            case InvestigationManager.InvestigationStage.SuspiciousNote:

                return suspiciousNote;

            case InvestigationManager.InvestigationStage.SecretKey:

                return secretKey;

            case InvestigationManager.InvestigationStage.LockedBox:

                return lockedBox;

            case InvestigationManager.InvestigationStage.FinalClue:

                return finalClue;

            case InvestigationManager.InvestigationStage.CaseSolved:

                return null;
        }

        return null;
    }

    // ============================================================
    // SCREEN TAP DETECTION
    // ============================================================

    private bool TryGetScreenTap(
        out Vector2 screenPosition
    )
    {
        screenPosition = Vector2.zero;

        // --------------------------------------------------------
        // UNITY NEW INPUT SYSTEM
        // --------------------------------------------------------

#if ENABLE_INPUT_SYSTEM

        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.primaryTouch.press
                .wasPressedThisFrame)
            {
                screenPosition =
                    Touchscreen.current.primaryTouch.position
                    .ReadValue();

                lastAction =
                    "TOUCH DETECTED → " +
                    screenPosition;

                Log(lastAction);

                return true;
            }
        }

        // --------------------------------------------------------
        // MOUSE / EDITOR
        // --------------------------------------------------------

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton
                .wasPressedThisFrame)
            {
                screenPosition =
                    Mouse.current.position
                    .ReadValue();

                lastAction =
                    "MOUSE TAP DETECTED → " +
                    screenPosition;

                Log(lastAction);

                return true;
            }
        }

#endif

        // --------------------------------------------------------
        // OLD INPUT SYSTEM FALLBACK
        // --------------------------------------------------------

#if ENABLE_LEGACY_INPUT_MANAGER

        if (Input.touchCount > 0)
        {
            Touch touch =
                Input.GetTouch(0);

            if (touch.phase ==
                TouchPhase.Began)
            {
                screenPosition =
                    touch.position;

                lastAction =
                    "LEGACY TOUCH DETECTED → " +
                    screenPosition;

                Log(lastAction);

                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            screenPosition =
                Input.mousePosition;

            lastAction =
                "LEGACY MOUSE TAP DETECTED → " +
                screenPosition;

            Log(lastAction);

            return true;
        }

#endif

        return false;
    }

    // ============================================================
    // SCREEN TAP HANDLER
    // ============================================================

    private void HandleScreenTap(
        Vector2 screenPosition
    )
    {
        if (investigationManager == null)
        {
            lastError =
                "InvestigationManager is NULL.";

            LogError(lastError);

            return;
        }

        InvestigationManager.InvestigationStage stage =
            investigationManager.currentStage;

        // --------------------------------------------------------
        // CASE SOLVED
        // --------------------------------------------------------

        if (stage ==
            InvestigationManager.InvestigationStage.CaseSolved)
        {
            lastAction =
                "Tap ignored - case already solved.";

            return;
        }

        // --------------------------------------------------------
        // OBJECT ALREADY PLACED
        // --------------------------------------------------------

        if (objectPlaced &&
            currentObject != null)
        {
            if (TryTapCurrentObject(
                screenPosition))
            {
                return;
            }

            lastAction =
                "Object already placed. Tap the object.";

            return;
        }

        // --------------------------------------------------------
        // SUSPICIOUS NOTE
        // --------------------------------------------------------

        if (stage ==
            InvestigationManager.InvestigationStage.SuspiciousNote)
        {
            if (currentObject != null)
            {
                if (TryTapCurrentObject(
                    screenPosition))
                {
                    return;
                }
            }

            lastAction =
                "Tap did not hit Suspicious Note.";

            return;
        }

        // --------------------------------------------------------
        // PLACEMENT CHECK
        // --------------------------------------------------------

        if (!placementAllowed)
        {
            lastAction =
                "Placement currently not allowed.";

            return;
        }

        // --------------------------------------------------------
        // PLACE OBJECT
        // --------------------------------------------------------

        TryPlaceCurrentStageObject(
            screenPosition
        );
    }

    // ============================================================
    // AR OBJECT PLACEMENT
    // ============================================================

    private void TryPlaceCurrentStageObject(
        Vector2 screenPosition
    )
    {
        if (currentObject == null)
        {
            lastError =
                "Current object is NULL.";

            LogError(lastError);

            return;
        }

#if UNITY_EDITOR
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            lastError =
                "Main Camera not found.";

            LogError(lastError);

            return;
        }

        currentObject.transform.position =
            mainCamera.transform.position +
            mainCamera.transform.forward * 1.5f;

        ARFaceCamera faceCamera =
            currentObject.GetComponent<ARFaceCamera>();

        if (faceCamera != null)
        {
            faceCamera.FaceCamera();
        }

        currentObject.SetActive(true);
        objectPlaced = true;
        placementAllowed = false;
        lastError = "";
        lastAction =
            currentObject.name +
            " PLACED IN FRONT OF CAMERA (EDITOR).";

        Log(lastAction);

        return;
#endif

        if (raycastManager == null)
        {
            lastError =
                "ARRaycastManager is NULL.";

            LogError(lastError);

            return;
        }

        lastAction =
            "TAP RECEIVED → Searching for AR plane...";

        Log(lastAction);

        bool hitSomething =
            raycastManager.Raycast(
                screenPosition,
                raycastHits,
                TrackableType.PlaneWithinPolygon
            );

        // --------------------------------------------------------
        // NO PLANE HIT
        // --------------------------------------------------------

        if (!hitSomething ||
            raycastHits.Count == 0)
        {
            lastAction =
                "TAP RECEIVED, BUT NO AR PLANE WAS HIT.";

            lastError =
                "Move phone slowly over the surface and tap directly on the detected plane.";

            Log(lastAction);

            return;
        }

        // --------------------------------------------------------
        // GET HIT
        // --------------------------------------------------------

        ARRaycastHit hit =
            raycastHits[0];

        ARPlane hitPlane =
            hit.trackable as ARPlane;

        if (hitPlane == null)
        {
            lastAction =
                "AR hit found, but it was not an ARPlane.";

            lastError =
                "Invalid AR plane hit.";

            LogError(lastError);

            return;
        }

        lastPlaneInfo =
            hitPlane.alignment.ToString();

        Log(
            "PLANE HIT → " +
            hitPlane.alignment
        );

        // --------------------------------------------------------
        // DETECTIVE BOARD
        // --------------------------------------------------------

        if (preparedStage ==
            InvestigationManager.InvestigationStage.DetectiveBoard)
        {
            if (hitPlane.alignment !=
                PlaneAlignment.Vertical)
            {
                lastAction =
                    "PLANE HIT, BUT IT IS NOT VERTICAL.";

                lastError =
                    "For Detective Board, point at a wall.";

                Log(lastAction);

                return;
            }
        }

        // --------------------------------------------------------
        // OTHER AR OBJECTS
        // --------------------------------------------------------

        else
        {
            if (hitPlane.alignment !=
                PlaneAlignment.HorizontalUp &&
                hitPlane.alignment !=
                PlaneAlignment.HorizontalDown)
            {
                lastAction =
                    "PLANE HIT, BUT IT IS NOT HORIZONTAL.";

                lastError =
                    "Point at a floor/table/bed surface.";

                Log(lastAction);

                return;
            }
        }

        // --------------------------------------------------------
        // GET AR POSE
        // --------------------------------------------------------

        Pose hitPose =
            hit.pose;

        // --------------------------------------------------------
        // PLACE OBJECT
        // --------------------------------------------------------

        currentObject.transform.SetPositionAndRotation(
            hitPose.position,
            hitPose.rotation
        );

        // --------------------------------------------------------
        // FACE CAMERA
        // --------------------------------------------------------

        ARFaceCamera faceCamera2 =
            currentObject.GetComponent<ARFaceCamera>();

        if (faceCamera2 != null)
        {
            faceCamera2.FaceCamera();
        }

        // --------------------------------------------------------
        // ACTIVATE OBJECT
        // --------------------------------------------------------

        currentObject.SetActive(true);

        objectPlaced = true;

        placementAllowed = false;

        // --------------------------------------------------------
        // HIDE PLANES
        // --------------------------------------------------------

        HideAllPlanes();

        // --------------------------------------------------------
        // UPDATE DEBUG
        // --------------------------------------------------------

        lastAction =
            currentObject.name +
            " PLACED SUCCESSFULLY!";

        lastError = "";

        Log(lastAction);
    }

    // ============================================================
    // OBJECT TAP
    // ============================================================

    private bool TryTapCurrentObject(
        Vector2 screenPosition
    )
    {
        if (currentObject == null)
        {
            return false;
        }

        Camera mainCamera =
            Camera.main;

        if (mainCamera == null)
        {
            lastError =
                "Main Camera not found.";

            LogError(lastError);

            return false;
        }

        Ray ray =
            mainCamera.ScreenPointToRay(
                screenPosition
            );

        RaycastHit physicsHit;

        if (Physics.Raycast(
            ray,
            out physicsHit,
            1000f
        ))
        {
            Transform hitTransform =
                physicsHit.transform;

            if (IsPartOfCurrentObject(
                hitTransform
            ))
            {
                ObjectTapped();

                return true;
            }
        }

        // --------------------------------------------------------
        // SCREEN DISTANCE FALLBACK
        // --------------------------------------------------------

        Vector3 screenObjectPosition =
            mainCamera.WorldToScreenPoint(
                currentObject.transform.position
            );

        if (screenObjectPosition.z > 0f)
        {
            float distance =
                Vector2.Distance(
                    screenPosition,
                    new Vector2(
                        screenObjectPosition.x,
                        screenObjectPosition.y
                    )
                );

            if (distance <= objectTapRadius)
            {
                ObjectTapped();

                return true;
            }
        }

        return false;
    }

    // ============================================================
    // CHECK OBJECT HIERARCHY
    // ============================================================

    private bool IsPartOfCurrentObject(
        Transform hitTransform
    )
    {
        if (hitTransform == null ||
            currentObject == null)
        {
            return false;
        }

        if (hitTransform.gameObject ==
            currentObject)
        {
            return true;
        }

        return hitTransform.IsChildOf(
            currentObject.transform
        );
    }

    // ============================================================
    // OBJECT TAPPED
    // ============================================================

    private void ObjectTapped()
    {
        if (investigationManager == null)
        {
            lastError =
                "InvestigationManager is NULL.";

            LogError(lastError);

            return;
        }

        if (currentObject == null)
        {
            lastError =
                "Current object is NULL.";

            LogError(lastError);

            return;
        }

        lastAction =
            "OBJECT TAPPED → " +
            currentObject.name;

        Log(lastAction);

        investigationManager.InteractWithObject(
            preparedStage
        );
    }

    // ============================================================
    // HIDE ALL AR PLANES
    // ============================================================

    public void HideAllPlanes()
    {
        if (planeManager == null)
        {
            return;
        }

        foreach (ARPlane plane in
                 planeManager.trackables)
        {
            HidePlane(
                plane
            );
        }

        lastAction =
            "AR plane visualization hidden.";

        Log(lastAction);
    }

    // ============================================================
    // SHOW ALL AR PLANES
    // ============================================================

    public void ShowAllPlanes()
    {
        if (planeManager == null)
        {
            return;
        }

        foreach (ARPlane plane in
                 planeManager.trackables)
        {
            if (plane == null)
            {
                continue;
            }

            ARPlaneMeshVisualizer visualizer =
                plane.GetComponent<
                    ARPlaneMeshVisualizer
                >();

            if (visualizer != null)
            {
                visualizer.enabled = true;
            }

            MeshCollider meshCollider =
                plane.GetComponent<MeshCollider>();

            if (meshCollider != null)
            {
                meshCollider.enabled = true;
            }
        }

        lastAction =
            "AR plane visualization visible.";

        Log(lastAction);
    }

    // ============================================================
    // HIDE ONE PLANE
    // ============================================================

    private void HidePlane(
        ARPlane plane
    )
    {
        if (plane == null)
        {
            return;
        }

        ARPlaneMeshVisualizer visualizer =
            plane.GetComponent<
                ARPlaneMeshVisualizer
            >();

        if (visualizer != null)
        {
            visualizer.enabled = false;
        }

        MeshCollider meshCollider =
            plane.GetComponent<MeshCollider>();

        if (meshCollider != null)
        {
            meshCollider.enabled = false;
        }
    }

    // ============================================================
    // NEW PLANES
    // ============================================================

    private void OnPlanesChanged(
        ARPlanesChangedEventArgs args
    )
    {
        if (!objectPlaced)
        {
            return;
        }

        if (args.added != null)
        {
            foreach (ARPlane plane in args.added)
            {
                HidePlane(
                    plane
                );
            }
        }

        if (args.updated != null)
        {
            foreach (ARPlane plane in args.updated)
            {
                HidePlane(
                    plane
                );
            }
        }
    }

    // ============================================================
    // DISABLE ALL INVESTIGATION OBJECTS
    // ============================================================

    public void DisableAllInvestigationObjects()
    {
        SetObjectActive(
            detectiveBoard,
            false
        );

        SetObjectActive(
            openBox,
            false
        );

        SetObjectActive(
            suspiciousNote,
            false
        );

        SetObjectActive(
            secretKey,
            false
        );

        SetObjectActive(
            lockedBox,
            false
        );

        SetObjectActive(
            finalClue,
            false
        );
    }

    // ============================================================
    // OBJECT ACTIVE HELPER
    // ============================================================

    private void SetObjectActive(
        GameObject obj,
        bool active
    )
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    // ============================================================
    // RESET
    // ============================================================

    public void ResetPlacement()
    {
        DisableAllInvestigationObjects();

        currentObject = null;

        objectPlaced = false;

        placementAllowed = false;

        preparedStage =
            InvestigationManager.InvestigationStage.DetectiveBoard;

        lastAction =
            "Placement reset.";

        lastError = "";

        ShowAllPlanes();

        Log(lastAction);
    }

    private void ResetPlacementState()
    {
        currentObject = null;

        objectPlaced = false;

        placementAllowed = false;

        preparedStage =
            InvestigationManager.InvestigationStage.DetectiveBoard;

        lastAction =
            "Initializing...";

        lastError = "";
    }

    // ============================================================
    // DEBUG LOGGING
    // ============================================================

    private void Log(
        string message
    )
    {
        if (!enableDebugLogs)
        {
            return;
        }

        Debug.Log(
            "[AR DETECTIVE] " +
            message
        );
    }

    private void LogError(
        string message
    )
    {
        if (!enableDebugLogs)
        {
            return;
        }

        Debug.LogError(
            "[AR DETECTIVE] " +
            message
        );
    }

    // ============================================================
    // ON SCREEN DIAGNOSTICS
    // ============================================================

    private void OnGUI()
    {
        if (!showOnScreenDiagnostics)
        {
            return;
        }

        GUIStyle style =
            new GUIStyle(
                GUI.skin.box
            );

        style.fontSize = 24;

        style.normal.textColor =
            Color.white;

        string objectName =
            currentObject != null
                ? currentObject.name
                : "NULL";

        string stageName =
            investigationManager != null
                ? investigationManager.currentStage.ToString()
                : "NULL";

        string managerName =
            investigationManager != null
                ? "FOUND"
                : "NULL";

        string text =
            "AR DETECTIVE\n" +
            "----------------------\n" +
            "STAGE: " +
            stageName +
            "\n" +
            "MANAGER: " +
            managerName +
            "\n" +
            "CURRENT OBJECT: " +
            objectName +
            "\n" +
            "OBJECT PLACED: " +
            objectPlaced +
            "\n" +
            "PLACEMENT ALLOWED: " +
            placementAllowed +
            "\n" +
            "PLANE: " +
            lastPlaneInfo +
            "\n" +
            "ACTION: " +
            lastAction;

        if (!string.IsNullOrEmpty(lastError))
        {
            text +=
                "\nERROR: " +
                lastError;
        }

        GUI.Box(
            new Rect(
                10,
                10,
                650,
                330
            ),
            text,
            style
        );
    }
}