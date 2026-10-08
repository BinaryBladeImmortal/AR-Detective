using UnityEngine;
using TMPro;

/// <summary>
/// ===============================================================
/// AR DETECTIVE - UI MANAGER
/// ===============================================================
///
/// RESPONSIBILITIES:
///
/// 1. Welcome screen.
/// 2. Play button.
/// 3. Game HUD.
/// 4. Clue popup.
/// 5. Close clue button.
/// 6. Evidence counter.
/// 7. Assistant panel.
/// 8. Case Solved screen.
/// 9. Restart.
/// 10. Communication with InvestigationManager.
///
/// IMPORTANT:
///
/// UIManager does NOT control AR placement.
///
/// UIManager does NOT decide investigation stages.
///
/// InvestigationManager = STORY
/// ARTapPlacementManager = AR
/// UIManager = USER INTERFACE
///
/// ===============================================================
/// </summary>

public class UIManager : MonoBehaviour
{
    // ============================================================
    // WELCOME UI
    // ============================================================

    [Header("Welcome UI")]

    [SerializeField]
    private GameObject welcomePanel;

    [SerializeField]
    private GameObject gameHUD;


    // ============================================================
    // CLUE UI
    // ============================================================

    [Header("Clue UI")]

    [SerializeField]
    private GameObject cluePanel;

    [SerializeField]
    private TMP_Text clueTitle;

    [SerializeField]
    private TMP_Text clueMessage;


    // ============================================================
    // EVIDENCE UI
    // ============================================================

    [Header("Evidence UI")]

    [SerializeField]
    private TMP_Text evidenceText;

    [SerializeField]
    private TMP_Text clueEvidenceText;


    // ============================================================
    // ASSISTANT UI
    // ============================================================

    [Header("Assistant UI")]

    [SerializeField]
    private GameObject assistantPanel;

    [SerializeField]
    private TMP_Text assistantMessage;


    // ============================================================
    // CASE SOLVED UI
    // ============================================================

    [Header("Case Solved UI")]

    [SerializeField]
    private GameObject caseSolvedPanel;

    [SerializeField]
    private TMP_Text caseSolvedTitle;

    [SerializeField]
    private TMP_Text caseSolvedMessage;

    // ============================================================
    // GAME MANAGER
    // ============================================================

    [Header("Game Manager")]

    [SerializeField]
    private InvestigationManager investigationManager;


    // ============================================================
    // EVIDENCE SYSTEM
    // ============================================================

    [Header("Evidence System")]

    [SerializeField]
    private int maximumEvidence = 6;

    private int currentEvidence = 0;


    // ============================================================
    // INTERNAL STATE
    // ============================================================

    private bool initialized = false;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("Debug")]

    [SerializeField]
    private bool enableDebugLogs = true;

    [SerializeField]
    private bool showScreenDiagnostics = false;


    private string lastUIAction =
        "UIManager starting...";


    // ============================================================
    // UNITY AWAKE
    // ============================================================

    private void Awake()
    {
        FindReferences();

        InitializeUI();
    }


    // ============================================================
    // UNITY START
    // ============================================================

    private void Start()
    {
        FindReferences();

        InitializeUI();

        initialized = true;

        Log(
            "UIManager STARTED."
        );
    }


    // ============================================================
    // FIND REFERENCES
    // ============================================================

    private void FindReferences()
    {
        // --------------------------------------------------------
        // InvestigationManager
        // --------------------------------------------------------

        if (investigationManager == null)
        {
            investigationManager =
                FindFirstObjectByType<InvestigationManager>();
        }
    }


    // ============================================================
    // INITIALIZE UI
    // ============================================================

    private void InitializeUI()
    {
        // --------------------------------------------------------
        // Reset evidence.
        // --------------------------------------------------------

        ResetEvidence();


        // --------------------------------------------------------
        // Welcome screen ON.
        // --------------------------------------------------------

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
        }


        // --------------------------------------------------------
        // Game HUD OFF.
        // --------------------------------------------------------

        if (gameHUD != null)
        {
            gameHUD.SetActive(false);
        }


        // --------------------------------------------------------
        // Clue popup OFF.
        // --------------------------------------------------------

        if (cluePanel != null)
        {
            cluePanel.SetActive(false);
        }


        // --------------------------------------------------------
        // Assistant OFF.
        // --------------------------------------------------------

        if (assistantPanel != null)
        {
            assistantPanel.SetActive(false);
        }


        // --------------------------------------------------------
        // Case solved OFF.
        // --------------------------------------------------------

        if (caseSolvedPanel != null)
        {
            caseSolvedPanel.SetActive(false);
        }


        // --------------------------------------------------------
        // Clear clue text.
        // --------------------------------------------------------

        if (clueTitle != null)
        {
            clueTitle.text = "";
        }


        if (clueMessage != null)
        {
            clueMessage.text = "";
        }


        // --------------------------------------------------------
        // Clear assistant text.
        // --------------------------------------------------------

        if (assistantMessage != null)
        {
            assistantMessage.text = "";
        }


        // --------------------------------------------------------
        // Update evidence.
        // --------------------------------------------------------

        UpdateEvidenceUI();


        lastUIAction =
            "UI initialized.";
    }


    // ============================================================
    // PLAY GAME
    //
    // Connect this to the PLAY button.
    // ============================================================

    public void PlayGame()
    {
        Log(
            "PLAY BUTTON PRESSED."
        );


        // --------------------------------------------------------
        // Hide welcome.
        // --------------------------------------------------------

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
        }


        // --------------------------------------------------------
        // Show HUD.
        // --------------------------------------------------------

        if (gameHUD != null)
        {
            gameHUD.SetActive(true);
        }


        // --------------------------------------------------------
        // Hide any old popup.
        // --------------------------------------------------------

        HideCluePanel();


        // --------------------------------------------------------
        // Hide case solved.
        // --------------------------------------------------------

        if (caseSolvedPanel != null)
        {
            caseSolvedPanel.SetActive(false);
        }


        // --------------------------------------------------------
        // Reset evidence.
        // --------------------------------------------------------

        ResetEvidence();


        // --------------------------------------------------------
        // Start investigation.
        // --------------------------------------------------------

        FindReferences();


        if (investigationManager == null)
        {
            LogError(
                "PlayGame(): InvestigationManager not found!"
            );

            return;
        }


        investigationManager.StartInvestigation();


        // --------------------------------------------------------
        // Update assistant.
        // --------------------------------------------------------

        ShowAssistantForCurrentStage();


        lastUIAction =
            "Game started.";
    }


    // ============================================================
    // SHOW CLUE
    //
    // Called by InvestigationManager.
    // ============================================================

    public void ShowClue(
        string title,
        string message
    )
    {
        Log(
            "================================"
        );

        Log(
            "UIManager.ShowClue() CALLED"
        );


        // --------------------------------------------------------
        // Safety check.
        // --------------------------------------------------------

        if (cluePanel == null)
        {
            LogError(
                "CluePanel reference is NULL!"
            );

            return;
        }


        // --------------------------------------------------------
        // Set title.
        // --------------------------------------------------------

        if (clueTitle != null)
        {
            clueTitle.text =
                title;
        }
        else
        {
            LogError(
                "ClueTitle reference is NULL!"
            );
        }


        // --------------------------------------------------------
        // Set message.
        // --------------------------------------------------------

        if (clueMessage != null)
        {
            clueMessage.text =
                message;
        }
        else
        {
            LogError(
                "ClueMessage reference is NULL!"
            );
        }


        // --------------------------------------------------------
        // Hide assistant while clue is open.
        // --------------------------------------------------------

        if (assistantPanel != null)
        {
            assistantPanel.SetActive(false);
        }

        if (!cluePanel.activeSelf)
        {
            AddEvidence();
        }

        // --------------------------------------------------------
        // SHOW CLUE PANEL
        //
        // This is the important line.
        // --------------------------------------------------------

        cluePanel.SetActive(true);


        // --------------------------------------------------------
        // Make sure case solved isn't covering it.
        // --------------------------------------------------------

        if (caseSolvedPanel != null)
        {
            caseSolvedPanel.SetActive(false);
        }


        lastUIAction =
            "Clue popup shown: " +
            title;


        Log(
            "CluePanel ACTIVATED."
        );

        Log(
            "Clue title/message assigned."
        );
    }


    // ============================================================
    // CLOSE CLUE
    //
    // Connect this to the X / CLOSE button.
    // ============================================================

    public void CloseClue()
    {
        Log(
            "================================"
        );

        Log(
            "CLOSE CLUE BUTTON PRESSED"
        );


        // --------------------------------------------------------
        // Hide popup immediately.
        // --------------------------------------------------------

        HideCluePanel();


        // --------------------------------------------------------
        // Tell InvestigationManager to advance.
        // --------------------------------------------------------

        FindReferences();


        if (investigationManager == null)
        {
            LogError(
                "CloseClue(): InvestigationManager not found!"
            );

            return;
        }


        investigationManager.CloseClue();


        // --------------------------------------------------------
        // If the case isn't solved, show the assistant for the
        // new stage.
        // --------------------------------------------------------

        if (!investigationManager.IsCaseSolved())
        {
            ShowAssistantForCurrentStage();
        }


        lastUIAction =
            "Clue closed.";
    }


    // ============================================================
    // HIDE CLUE PANEL
    // ============================================================

    private void HideCluePanel()
    {
        if (cluePanel != null)
        {
            cluePanel.SetActive(false);
        }


        if (clueTitle != null)
        {
            clueTitle.text = "";
        }


        if (clueMessage != null)
        {
            clueMessage.text = "";
        }
    }


    // ============================================================
    // ADD EVIDENCE
    //
    // Evidence count is limited to maximumEvidence.
    // ============================================================

    public void AddEvidence()
    {
        if (currentEvidence >= maximumEvidence)
        {
            Log(
                "Evidence limit reached."
            );

            return;
        }


        currentEvidence++;

        Debug.Log(
            "EVIDENCE: " +
            currentEvidence +
            "/" +
            maximumEvidence
        );

        UpdateEvidenceUI();


        Log(
            "Evidence added: " +
            currentEvidence +
            "/" +
            maximumEvidence
        );
    }


    // ============================================================
    // RESET EVIDENCE
    // ============================================================

    public void ResetEvidence()
    {
        currentEvidence = 0;

        UpdateEvidenceUI();
    }


    // ============================================================
    // UPDATE EVIDENCE UI
    // ============================================================

    private void UpdateEvidenceUI()
    {
        string text =
            "Evidence: " +
            currentEvidence +
            "/" +
            maximumEvidence;

        if (evidenceText != null)
        {
            evidenceText.text = text;
        }

        if (clueEvidenceText != null)
        {
            clueEvidenceText.text = text;
        }
    }


    // ============================================================
    // SHOW ASSISTANT
    // ============================================================

    public void ShowAssistant(
        string message
    )
    {
        if (assistantPanel == null)
        {
            LogError(
                "AssistantPanel reference is NULL!"
            );

            return;
        }


        if (assistantMessage != null)
        {
            assistantMessage.text =
                message;
        }


        assistantPanel.SetActive(true);


        lastUIAction =
            "Assistant shown.";
    }


    public void ShowAssistantHint(int hintType)
    {
        if (investigationManager == null)
        {
            FindReferences();
        }

        if (investigationManager == null)
        {
            LogError(
                "ShowAssistantHint(): InvestigationManager not found!"
            );

            return;
        }

        string smallHint;
        string explainHint;
        string strongHint;

        switch (investigationManager.currentStage)
        {
            case InvestigationManager.InvestigationStage.DetectiveBoard:
                smallHint = "A good starting point is somewhere upright.";
                explainHint = "Aim at a wall and tap a suitable vertical surface to place the Detective Board.";
                strongHint = "Point at a vertical wall and tap to place the Detective Board.";
                break;

            case InvestigationManager.InvestigationStage.OpenBox:
                smallHint = "Look for a flat place nearby.";
                explainHint = "Aim at a table or other horizontal surface and tap to place the open box.";
                strongHint = "Point at a horizontal surface and tap to place the Open Box.";
                break;

            case InvestigationManager.InvestigationStage.SuspiciousNote:
                smallHint = "Something written may be easy to overlook.";
                explainHint = "Look around for the suspicious note, then tap it.";
                strongHint = "Find the suspicious note and tap it.";
                break;

            case InvestigationManager.InvestigationStage.SecretKey:
                smallHint = "A small object may hold the next lead.";
                explainHint = "Find a suitable horizontal surface and tap to place the Secret Key.";
                strongHint = "Point at a horizontal surface and tap to place the Secret Key.";
                break;

            case InvestigationManager.InvestigationStage.LockedBox:
                smallHint = "The next clue needs a steady place.";
                explainHint = "Find a suitable horizontal surface and tap to place the Locked Box.";
                strongHint = "Point at a horizontal surface and tap to place the Locked Box.";
                break;

            case InvestigationManager.InvestigationStage.FinalClue:
                smallHint = "You are close to the end of the trail.";
                explainHint = "Find a suitable horizontal surface and tap to place the final clue.";
                strongHint = "Point at a horizontal surface and tap to place the Final Clue.";
                break;

            case InvestigationManager.InvestigationStage.CaseSolved:
                smallHint = "The investigation is complete.";
                explainHint = "Review the evidence you collected.";
                strongHint = "The case is solved.";
                break;

            default:
                LogError(
                    "ShowAssistantHint(): Unknown investigation stage."
                );

                return;
        }

        string hint;

        switch (hintType)
        {
            case 1:
                hint = smallHint;
                break;

            case 3:
                hint = explainHint;
                break;

            case 2:
                hint = strongHint;
                break;

            default:
                LogError(
                    "ShowAssistantHint(): Hint type must be 1, 2, or 3."
                );

                return;
        }

        ShowAssistant(hint);
    }


    // ============================================================
    // HIDE ASSISTANT
    // ============================================================

    public void HideAssistant()
    {
        if (assistantPanel != null)
        {
            assistantPanel.SetActive(false);
        }
    }


    // ============================================================
    // SHOW ASSISTANT FOR CURRENT STAGE
    // ============================================================

    private void ShowAssistantForCurrentStage()
    {
        if (investigationManager == null)
        {
            return;
        }


        switch (
            investigationManager.currentStage
        )
        {
            case InvestigationManager.InvestigationStage.DetectiveBoard:

                ShowAssistant(
                    "Point your camera at a wall and tap a suitable vertical surface to place the Detective Board."
                );

                break;


            case InvestigationManager.InvestigationStage.OpenBox:

                ShowAssistant(
                    "Point your camera at a table, floor, or other horizontal surface and tap to place the box."
                );

                break;


            case InvestigationManager.InvestigationStage.SuspiciousNote:

                ShowAssistant(
                    "Look around the scene. Find and tap the suspicious note."
                );

                break;


            case InvestigationManager.InvestigationStage.SecretKey:

                ShowAssistant(
                    "Search the area for a suitable horizontal surface and place the Secret Key."
                );

                break;


            case InvestigationManager.InvestigationStage.LockedBox:

                ShowAssistant(
                    "Place the Locked Box on a suitable horizontal surface."
                );

                break;


            case InvestigationManager.InvestigationStage.FinalClue:

                ShowAssistant(
                    "Place the final clue on a suitable horizontal surface."
                );

                break;


            case InvestigationManager.InvestigationStage.CaseSolved:

                HideAssistant();

                break;
        }
    }


    // ============================================================
    // CASE SOLVED
    //
    // Called by InvestigationManager.
    // ============================================================

    public void ShowCaseSolved()
    {
        Log(
            "================================"
        );

        Log(
            "SHOW CASE SOLVED"
        );

        if (caseSolvedTitle != null)
        {
            caseSolvedTitle.text =
                "<size=80><b><color=#E8C15F>CASE SOLVED!</color></b></size>\n" +
                "<size=40><color=#F2EAD8>Great work, Detective.</color></size>";
        }

        if (caseSolvedMessage != null)
        {
            caseSolvedMessage.text =
                "The suspicious note led you to the Secret Key, which opened the locked box and revealed the missing prototype. " +
                "Every clue fit together, and the truth is finally uncovered.\n\n" +
                "<size=32><b><color=#E8C15F>Evidence collected: " +
                currentEvidence +
                "/" +
                maximumEvidence +
                "</color></b></size>";
        }

        // --------------------------------------------------------
        // Hide clue.
        // --------------------------------------------------------

        HideCluePanel();


        // --------------------------------------------------------
        // Hide assistant.
        // --------------------------------------------------------

        HideAssistant();


        // --------------------------------------------------------
        // Hide HUD.
        // --------------------------------------------------------

        if (gameHUD != null)
        {
            gameHUD.SetActive(false);
        }


        // --------------------------------------------------------
        // Show solved panel.
        // --------------------------------------------------------

        if (caseSolvedPanel != null)
        {
            caseSolvedPanel.SetActive(true);
        }
        else
        {
            LogError(
                "CaseSolvedPanel reference is NULL!"
            );
        }


        lastUIAction =
            "CASE SOLVED SCREEN SHOWN.";
    }


    // ============================================================
    // RESTART GAME
    //
    // Connect this to the Restart button.
    // ============================================================

    public void RestartGame()
    {
        Log(
            "================================"
        );

        Log(
            "RESTART GAME"
        );


        // --------------------------------------------------------
        // Hide solved screen.
        // --------------------------------------------------------

        if (caseSolvedPanel != null)
        {
            caseSolvedPanel.SetActive(false);
        }


        // --------------------------------------------------------
        // Hide clue.
        // --------------------------------------------------------

        HideCluePanel();


        // --------------------------------------------------------
        // Reset evidence.
        // --------------------------------------------------------

        ResetEvidence();


        // --------------------------------------------------------
        // Show HUD.
        // --------------------------------------------------------

        if (gameHUD != null)
        {
            gameHUD.SetActive(true);
        }


        // --------------------------------------------------------
        // Hide welcome.
        // --------------------------------------------------------

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
        }


        // --------------------------------------------------------
        // Find InvestigationManager.
        // --------------------------------------------------------

        FindReferences();


        if (investigationManager == null)
        {
            LogError(
                "RestartGame(): InvestigationManager not found!"
            );

            return;
        }


        // --------------------------------------------------------
        // Restart investigation.
        // --------------------------------------------------------

        investigationManager.RestartInvestigation();


        // --------------------------------------------------------
        // Show assistant.
        // --------------------------------------------------------

        ShowAssistantForCurrentStage();


        lastUIAction =
            "Game restarted.";
    }


    // ============================================================
    // OPTIONAL PUBLIC BUTTON METHODS
    //
    // These are useful if buttons are connected through the
    // Unity Inspector.
    // ============================================================

    public void OnPlayButtonPressed()
    {
        PlayGame();
    }


    public void OnCloseButtonPressed()
    {
        CloseClue();
    }


    public void OnRestartButtonPressed()
    {
        RestartGame();
    }


    // ============================================================
    // GETTERS
    // ============================================================

    public int GetCurrentEvidence()
    {
        return currentEvidence;
    }


    public int GetMaximumEvidence()
    {
        return maximumEvidence;
    }


    public bool IsClueVisible()
    {
        if (cluePanel == null)
        {
            return false;
        }

        return cluePanel.activeSelf;
    }


    public bool IsCaseSolvedVisible()
    {
        if (caseSolvedPanel == null)
        {
            return false;
        }

        return caseSolvedPanel.activeSelf;
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
            "[AR DETECTIVE / UI] " +
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
            "[AR DETECTIVE / UI] " +
            message
        );
    }


    // ============================================================
    // OPTIONAL SCREEN DIAGNOSTIC
    //
    // OFF by default.
    // ============================================================

    private void OnGUI()
    {
        if (!showScreenDiagnostics)
        {
            return;
        }


        GUIStyle style =
            new GUIStyle(GUI.skin.box);


        style.fontSize = 22;

        style.normal.textColor =
            Color.white;


        string clueState =
            cluePanel != null &&
            cluePanel.activeSelf
                ? "OPEN"
                : "CLOSED";


        string stage =
            investigationManager != null
                ? investigationManager.currentStage.ToString()
                : "NULL";


        string text =
            "UI DIAGNOSTIC\n" +
            "----------------------\n" +
            "STAGE: " +
            stage +
            "\n" +
            "CLUE PANEL: " +
            clueState +
            "\n" +
            "EVIDENCE: " +
            currentEvidence +
            "/" +
            maximumEvidence +
            "\n" +
            "LAST UI ACTION:\n" +
            lastUIAction;


        GUI.Box(
            new Rect(
                10,
                320,
                650,
                260
            ),
            text,
            style
        );
    }
}