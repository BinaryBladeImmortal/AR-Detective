using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EvidenceManager : MonoBehaviour
{
    public List<Evidence> discoveredEvidence = new List<Evidence>();
    public int totalRequiredEvidence = 3;

    private bool caseSolved = false;
    void Start()
    {
       Debug.Log("EvidenceManager STARTED — Required Evidence: " + totalRequiredEvidence);
    }

    [Header("Investigation UI")]
    public GameObject evidencePanel;
    public TMP_Text evidenceNameText;
    public TMP_Text evidenceDescriptionText;
    public TMP_Text evidenceCounterText;
    public GameObject caseSolvedPanel;

    public void DiscoverEvidence(Evidence evidence)
    {
        if (evidence == null)
            return;

        if (!discoveredEvidence.Contains(evidence))
        {
            discoveredEvidence.Add(evidence);

            Debug.Log("Evidence discovered: " + evidence.evidenceName);
        }

        ShowEvidence(evidence);

        if (evidenceCounterText != null)
        {
            evidenceCounterText.text =
                "Evidence: " + discoveredEvidence.Count + " / " + totalRequiredEvidence;
        }

        if (discoveredEvidence.Count >= totalRequiredEvidence)
        {
            caseSolved = true;
            Debug.Log("CASE SOLVED!");
        }
    }

    void ShowEvidence(Evidence evidence)
    {
        if (evidencePanel != null)
            evidencePanel.SetActive(true);

        if (evidenceNameText != null)
            evidenceNameText.text = evidence.evidenceName;

        if (evidenceDescriptionText != null)
            evidenceDescriptionText.text = evidence.description;
    }

    public bool HasEvidence(Evidence evidence)
    {
        return discoveredEvidence.Contains(evidence);
    }

    public void CloseEvidencePanel()
    {
        if (evidencePanel != null)
            evidencePanel.SetActive(false);

        // Only show Case Solved after the player
        // has finished reading the final clue.
        if (caseSolved)
        {
            if (caseSolvedPanel != null)
                caseSolvedPanel.SetActive(true);
        }
    }

    public void CloseCaseSolvedPanel()
    {
        if (caseSolvedPanel != null)
            caseSolvedPanel.SetActive(false);
    }
}