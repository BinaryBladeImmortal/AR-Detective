using UnityEngine;

/// <summary>
/// ===============================================================
/// AR DETECTIVE - INVESTIGATION MANAGER
/// ===============================================================
///
/// This is the STORY / GAMEPLAY controller.
///
/// Responsibilities:
///
/// 1. Controls the current investigation stage.
/// 2. Makes sure only the correct investigation object is active.
/// 3. Tells ARTapPlacementManager which object belongs to the
///    current stage.
/// 4. Receives object interaction from ARTapPlacementManager.
/// 5. Tells UIManager which clue popup to display.
/// 6. Waits for the player to close the clue popup.
/// 7. Moves the investigation to the next stage.
/// 8. Places the Suspicious Note at one of the predefined locations.
/// 9. Handles restart.
/// 10. Handles Case Solved.
///
/// IMPORTANT ARCHITECTURE:
///
/// InvestigationManager
///        ↓
/// controls GAME FLOW
///
/// ARTapPlacementManager
///        ↓
/// controls AR PLACEMENT + TAPS
///
/// UIManager
///        ↓
/// controls POPUPS + UI
///
/// ===============================================================
/// </summary>

public class InvestigationManager : MonoBehaviour
{
    // ============================================================
    // INVESTIGATION STAGES
    // ============================================================

    public enum InvestigationStage
    {
        DetectiveBoard,
        OpenBox,
        SuspiciousNote,
        SecretKey,
        LockedBox,
        FinalClue,
        CaseSolved
    }


    // ============================================================
    // CURRENT STAGE
    // ============================================================

    [Header("Current Investigation")]

    public InvestigationStage currentStage =
        InvestigationStage.DetectiveBoard;


    // ============================================================
    // INVESTIGATION OBJECTS
    // ============================================================

    [Header("Investigation Objects")]

    [Tooltip("Detective Board placed on a vertical AR surface.")]
    public GameObject detectiveBoard;

    [Tooltip("Open Box placed on a horizontal AR surface.")]
    public GameObject openBox;

    [Tooltip("Suspicious Note placed at one predefined location.")]
    public GameObject suspiciousNote;

    [Tooltip("Secret Key placed on a horizontal AR surface.")]
    public GameObject secretKey;

    [Tooltip("Locked Box placed on a horizontal AR surface.")]
    public GameObject lockedBox;

    [Tooltip("Final Clue placed on a horizontal AR surface.")]
    public GameObject finalClue;


    // ============================================================
    // SUSPICIOUS NOTE LOCATIONS
    // ============================================================

    [Header("Suspicious Note Locations")]

    [Tooltip(
        "Predefined locations where the Suspicious Note can appear."
    )]
    public Transform[] suspiciousNoteLocations;


    // ============================================================
    // UI MANAGER
    // ============================================================

    [Header("UI")]

    [Tooltip(
        "Reference to the UIManager GameObject."
    )]
    public UIManager uiManager;


    // ============================================================
    // INTERNAL STATE
    // ============================================================

    private bool clueOpen = false;

    private bool investigationStarted = false;

    private bool changingStage = false;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("Debug")]

    [SerializeField]
    private bool enableDebugLogs = true;


    // ============================================================
    // UNITY START
    // ============================================================

    private void Start()
    {
        // --------------------------------------------------------
        // Reset internal state.
        // --------------------------------------------------------

        clueOpen = false;

        investigationStarted = false;

        changingStage = false;


        // --------------------------------------------------------
        // Make sure every investigation object starts hidden.
        // --------------------------------------------------------

        DisableAllInvestigationObjects();


        // --------------------------------------------------------
        // Always start from Detective Board.
        // --------------------------------------------------------

        currentStage =
            InvestigationStage.DetectiveBoard;


        Log(
            "InvestigationManager initialized."
        );

        Log(
            "Initial stage = DetectiveBoard."
        );
    }


    // ============================================================
    // START INVESTIGATION
    //
    // Called by UIManager when PLAY is pressed.
    // ============================================================

    public void StartInvestigation()
    {
        // --------------------------------------------------------
        // Prevent starting twice.
        // --------------------------------------------------------

        if (investigationStarted)
        {
            Log(
                "StartInvestigation ignored - already started."
            );

            return;
        }


        // --------------------------------------------------------
        // Update state.
        // --------------------------------------------------------

        investigationStarted = true;

        clueOpen = false;

        changingStage = false;


        Log(
            "=============================="
        );

        Log(
            "INVESTIGATION STARTED"
        );


        // --------------------------------------------------------
        // Start from Detective Board.
        // --------------------------------------------------------

        SetStage(
            InvestigationStage.DetectiveBoard
        );
    }


    // ============================================================
    // INTERACT WITH OBJECT
    //
    // Called by ARTapPlacementManager after the correct object
    // has been tapped.
    // ============================================================

    public void InteractWithObject(
        InvestigationStage objectStage
    )
    {
        if (uiManager == null)
        {
            uiManager =
                FindFirstObjectByType<UIManager>();
        }

        // --------------------------------------------------------
        // Safety check.
        // --------------------------------------------------------

        if (!investigationStarted)
        {
            Log(
                "Object interaction ignored - investigation not started."
            );

            return;
        }


        // --------------------------------------------------------
        // Prevent another interaction while popup is open.
        // --------------------------------------------------------

        if (clueOpen)
        {
            Log(
                "Object interaction ignored - clue popup already open."
            );

            return;
        }


        // --------------------------------------------------------
        // Make sure the tapped object belongs to the current stage.
        // --------------------------------------------------------

        if (objectStage != currentStage)
        {
            Log(
                "Wrong object interaction."
            );

            Log(
                "Current Stage = " +
                currentStage
            );

            Log(
                "Tapped Object Stage = " +
                objectStage
            );

            return;
        }


        // --------------------------------------------------------
        // Diagnostic.
        // --------------------------------------------------------

        Log(
            "OBJECT INTERACTION RECEIVED"
        );

        Log(
            "Stage = " +
            currentStage
        );


        // --------------------------------------------------------
        // Show the appropriate clue.
        // --------------------------------------------------------

        switch (currentStage)
        {
            case InvestigationStage.DetectiveBoard:

                ShowClue(
                    "Detective Board",
                    "A strange case has been reported. Follow the evidence to discover what happened."
                );

                break;


            case InvestigationStage.OpenBox:

                ShowClue(
                    "Open Box",
                    "The box is empty, but someone left a clue behind. Search the area for a suspicious note."
                );

                break;


            case InvestigationStage.SuspiciousNote:

                ShowClue(
                    "Suspicious Note",
                    "The note contains a hidden hint about the Secret Key. Keep investigating."
                );

                break;


            case InvestigationStage.SecretKey:

                ShowClue(
                    "Secret Key",
                    "You found the hidden key. It may unlock something important."
                );

                break;


            case InvestigationStage.LockedBox:

                ShowClue(
                    "Locked Box",
                    "The Secret Key fits the lock. Open the box to discover the final clue."
                );

                break;


            case InvestigationStage.FinalClue:

                ShowClue(
                    "Final Clue",
                    "The evidence finally reveals what happened. You solved the case!"
                );

                break;


            case InvestigationStage.CaseSolved:

                Log(
                    "Case is already solved."
                );

                break;
        }
    }


    // ============================================================
    // SHOW CLUE
    //
    // This is the communication point between:
    //
    // InvestigationManager → UIManager
    // ============================================================

    private void ShowClue(
        string title,
        string message
    )
    {
        // --------------------------------------------------------
        // UIManager safety check.
        // --------------------------------------------------------

        if (uiManager == null)
        {
            LogError(
                "UIManager reference is NULL!"
            );

            return;
        }


        // --------------------------------------------------------
        // Mark popup as open.
        // --------------------------------------------------------

        clueOpen = true;


        Log(
            "ShowClue() called."
        );

        Log(
            "Title = " +
            title
        );


        Log(
            "UIManager FOUND."
        );


        // --------------------------------------------------------
        // Send popup request to UIManager.
        // --------------------------------------------------------

        uiManager.ShowClue(
            title,
            message
        );


        Log(
            "uiManager.ShowClue() executed."
        );
    }


    // ============================================================
    // CLOSE CLUE
    //
    // Called by UIManager when the player presses X / CLOSE.
    //
    // This is the main STORY ADVANCEMENT point.
    // ============================================================

    public void CloseClue()
    {
        // --------------------------------------------------------
        // Investigation must be active.
        // --------------------------------------------------------

        if (!investigationStarted)
        {
            Log(
                "CloseClue ignored - investigation not started."
            );

            return;
        }


        // --------------------------------------------------------
        // Nothing to close.
        // --------------------------------------------------------

        if (!clueOpen)
        {
            Log(
                "CloseClue ignored - no clue is open."
            );

            return;
        }


        // --------------------------------------------------------
        // Prevent duplicate stage changes.
        // --------------------------------------------------------

        if (changingStage)
        {
            Log(
                "CloseClue ignored - stage transition already running."
            );

            return;
        }


        changingStage = true;

        clueOpen = false;


        Log(
            "=============================="
        );

        Log(
            "CLUE CLOSED"
        );

        Log(
            "Current stage was: " +
            currentStage
        );


        // --------------------------------------------------------
        // Determine next stage.
        // --------------------------------------------------------

        switch (currentStage)
        {
            case InvestigationStage.DetectiveBoard:

                SetStage(
                    InvestigationStage.OpenBox
                );

                break;


            case InvestigationStage.OpenBox:

                SetStage(
                    InvestigationStage.SuspiciousNote
                );

                break;


            case InvestigationStage.SuspiciousNote:

                SetStage(
                    InvestigationStage.SecretKey
                );

                break;


            case InvestigationStage.SecretKey:

                SetStage(
                    InvestigationStage.LockedBox
                );

                break;


            case InvestigationStage.LockedBox:

                SetStage(
                    InvestigationStage.FinalClue
                );

                break;


            case InvestigationStage.FinalClue:

                SetStage(
                    InvestigationStage.CaseSolved
                );

                break;


            case InvestigationStage.CaseSolved:

                Log(
                    "No stage exists after CaseSolved."
                );

                break;
        }


        changingStage = false;
    }


    // ============================================================
    // SET STAGE
    //
    // This is the central stage-transition function.
    // ============================================================

    public void SetStage(
        InvestigationStage newStage
    )
    {
        // --------------------------------------------------------
        // Update current stage.
        // --------------------------------------------------------

        currentStage =
            newStage;


        // --------------------------------------------------------
        // Make sure no popup remains logically open.
        // --------------------------------------------------------

        clueOpen = false;


        // --------------------------------------------------------
        // Debug.
        // --------------------------------------------------------

        Log(
            "=============================="
        );

        Log(
            "STAGE CHANGED → " +
            newStage
        );


        // --------------------------------------------------------
        // First disable every investigation object.
        //
        // This guarantees only one object can be active.
        // --------------------------------------------------------

        DisableAllInvestigationObjects();


        // --------------------------------------------------------
        // Tell ARTapPlacementManager about the new stage.
        // --------------------------------------------------------

        ARTapPlacementManager placementManager =
            FindFirstObjectByType<ARTapPlacementManager>();


        if (placementManager != null)
        {
            placementManager.PrepareForStage(
                newStage
            );


            Log(
                "ARTapPlacementManager.PrepareForStage() called."
            );
        }
        else
        {
            LogError(
                "ARTapPlacementManager not found!"
            );
        }


        // --------------------------------------------------------
        // STAGE-SPECIFIC SETUP
        // --------------------------------------------------------

        switch (newStage)
        {
            // ====================================================
            // DETECTIVE BOARD
            // ====================================================

            case InvestigationStage.DetectiveBoard:

                PrepareDetectiveBoard();

                break;


            // ====================================================
            // OPEN BOX
            // ====================================================

            case InvestigationStage.OpenBox:

                PrepareOpenBox();

                break;


            // ====================================================
            // SUSPICIOUS NOTE
            // ====================================================

            case InvestigationStage.SuspiciousNote:

                PrepareSuspiciousNote();

                break;


            // ====================================================
            // SECRET KEY
            // ====================================================

            case InvestigationStage.SecretKey:

                PrepareSecretKey();

                break;


            // ====================================================
            // LOCKED BOX
            // ====================================================

            case InvestigationStage.LockedBox:

                PrepareLockedBox();

                break;


            // ====================================================
            // FINAL CLUE
            // ====================================================

            case InvestigationStage.FinalClue:

                PrepareFinalClue();

                break;


            // ====================================================
            // CASE SOLVED
            // ====================================================

            case InvestigationStage.CaseSolved:

                PrepareCaseSolved();

                break;
        }
    }


    // ============================================================
    // DETECTIVE BOARD SETUP
    // ============================================================

    private void PrepareDetectiveBoard()
    {
        if (detectiveBoard != null)
        {
            detectiveBoard.SetActive(false);
        }


        Log(
            "READY FOR DETECTIVE BOARD."
        );

        Log(
            "Point at a vertical surface and tap."
        );
    }


    // ============================================================
    // OPEN BOX SETUP
    // ============================================================

    private void PrepareOpenBox()
    {
        if (openBox != null)
        {
            openBox.SetActive(false);
        }


        Log(
            "READY FOR OPEN BOX."
        );

        Log(
            "Point at a horizontal surface and tap."
        );
    }


    // ============================================================
    // SUSPICIOUS NOTE SETUP
    // ============================================================

    private void PrepareSuspiciousNote()
    {
        // --------------------------------------------------------
        // Place the note at one of the predefined locations.
        // --------------------------------------------------------

        PlaceSuspiciousNoteRandomly();


        // --------------------------------------------------------
        // Activate the note.
        // --------------------------------------------------------

        if (suspiciousNote != null)
        {
            suspiciousNote.SetActive(true);

            Log(
                "SUSPICIOUS NOTE ACTIVATED."
            );
        }
        else
        {
            LogError(
                "Suspicious Note reference is NULL!"
            );
        }
    }


    // ============================================================
    // SECRET KEY SETUP
    // ============================================================

    private void PrepareSecretKey()
    {
        if (secretKey != null)
        {
            secretKey.SetActive(false);
        }


        Log(
            "READY FOR SECRET KEY."
        );

        Log(
            "Point at a horizontal surface and tap."
        );
    }


    // ============================================================
    // LOCKED BOX SETUP
    // ============================================================

    private void PrepareLockedBox()
    {
        if (lockedBox != null)
        {
            lockedBox.SetActive(false);
        }


        Log(
            "READY FOR LOCKED BOX."
        );

        Log(
            "Point at a horizontal surface and tap."
        );
    }


    // ============================================================
    // FINAL CLUE SETUP
    // ============================================================

    private void PrepareFinalClue()
    {
        if (finalClue != null)
        {
            finalClue.SetActive(false);
        }


        Log(
            "READY FOR FINAL CLUE."
        );

        Log(
            "Point at a horizontal surface and tap."
        );
    }


    // ============================================================
    // CASE SOLVED
    // ============================================================

    private void PrepareCaseSolved()
    {
        // --------------------------------------------------------
        // No investigation object remains active.
        // --------------------------------------------------------

        DisableAllInvestigationObjects();


        // --------------------------------------------------------
        // Tell UIManager to show the final screen.
        // --------------------------------------------------------

        if (uiManager != null)
        {
            uiManager.ShowCaseSolved();

            Log(
                "UIManager.ShowCaseSolved() called."
            );
        }
        else
        {
            LogError(
                "Cannot show Case Solved - UIManager is NULL."
            );
        }


        // --------------------------------------------------------
        // Tell AR system to show planes again.
        // --------------------------------------------------------

        ARTapPlacementManager placementManager =
            FindFirstObjectByType<ARTapPlacementManager>();


        if (placementManager != null)
        {
            placementManager.ShowAllPlanes();
        }


        Log(
            "=============================="
        );

        Log(
            "CASE SOLVED!"
        );

        Log(
            "The investigation is complete."
        );
    }


    // ============================================================
    // DISABLE ALL INVESTIGATION OBJECTS
    // ============================================================

    private void DisableAllInvestigationObjects()
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
    // SAFE OBJECT ACTIVATION
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
    // PLACE SUSPICIOUS NOTE
    // ============================================================

    private void PlaceSuspiciousNoteRandomly()
    {
        // --------------------------------------------------------
        // Check note.
        // --------------------------------------------------------

        if (suspiciousNote == null)
        {
            LogError(
                "Suspicious Note is not assigned."
            );

            return;
        }


        // --------------------------------------------------------
        // Check locations.
        // --------------------------------------------------------

        if (suspiciousNoteLocations == null ||
            suspiciousNoteLocations.Length == 0)
        {
            LogError(
                "No Suspicious Note locations assigned."
            );

            return;
        }


        // --------------------------------------------------------
        // Build a list of valid locations.
        // --------------------------------------------------------

        int validLocationCount = 0;


        for (
            int i = 0;
            i < suspiciousNoteLocations.Length;
            i++
        )
        {
            if (suspiciousNoteLocations[i] != null)
            {
                validLocationCount++;
            }
        }


        if (validLocationCount == 0)
        {
            LogError(
                "All Suspicious Note locations are empty."
            );

            return;
        }


        // --------------------------------------------------------
        // Randomly choose one valid location.
        // --------------------------------------------------------

        Transform selectedLocation = null;


        int safetyCounter = 0;


        while (
            selectedLocation == null &&
            safetyCounter < 100
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    suspiciousNoteLocations.Length
                );


            selectedLocation =
                suspiciousNoteLocations[randomIndex];


            safetyCounter++;
        }


        // --------------------------------------------------------
        // Safety fallback.
        // --------------------------------------------------------

        if (selectedLocation == null)
        {
            for (
                int i = 0;
                i < suspiciousNoteLocations.Length;
                i++
            )
            {
                if (suspiciousNoteLocations[i] != null)
                {
                    selectedLocation =
                        suspiciousNoteLocations[i];

                    break;
                }
            }
        }


        // --------------------------------------------------------
        // Still nothing?
        // --------------------------------------------------------

        if (selectedLocation == null)
        {
            LogError(
                "Could not find a valid Suspicious Note location."
            );

            return;
        }


        // --------------------------------------------------------
        // Apply position.
        // --------------------------------------------------------

        suspiciousNote.transform.position =
            selectedLocation.position;


        // --------------------------------------------------------
        // Apply rotation.
        // --------------------------------------------------------

        suspiciousNote.transform.rotation =
            selectedLocation.rotation;


        // --------------------------------------------------------
        // Diagnostic.
        // --------------------------------------------------------

        Log(
            "Suspicious Note placed at: " +
            selectedLocation.name
        );
    }


    // ============================================================
    // RESTART INVESTIGATION
    //
    // Called by UIManager when the player presses Restart.
    // ============================================================

    public void RestartInvestigation()
    {
        Log(
            "=============================="
        );

        Log(
            "INVESTIGATION RESTARTING..."
        );


        // --------------------------------------------------------
        // Reset state.
        // --------------------------------------------------------

        investigationStarted = true;

        clueOpen = false;

        changingStage = false;


        // --------------------------------------------------------
        // Reset stage.
        // --------------------------------------------------------

        SetStage(
            InvestigationStage.DetectiveBoard
        );


        // --------------------------------------------------------
        // Diagnostic.
        // --------------------------------------------------------

        Log(
            "INVESTIGATION RESTARTED."
        );
    }


    // ============================================================
    // PUBLIC STATE HELPERS
    //
    // These are useful to UIManager and future systems.
    // ============================================================

    public bool IsInvestigationStarted()
    {
        return investigationStarted;
    }


    public bool IsClueOpen()
    {
        return clueOpen;
    }


    public bool IsCaseSolved()
    {
        return currentStage ==
               InvestigationStage.CaseSolved;
    }


    public InvestigationStage GetCurrentStage()
    {
        return currentStage;
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
            "[AR DETECTIVE / INVESTIGATION] " +
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
            "[AR DETECTIVE / INVESTIGATION] " +
            message
        );
    }
}