using UnityEngine;

public class InvestigationObject : MonoBehaviour
{
    [Header("Investigation")]
    public InvestigationManager.InvestigationStage objectStage;

    public string objectName;

    private InvestigationManager manager;

    private void Awake()
    {
        manager =
            FindFirstObjectByType<InvestigationManager>();
    }

    public void Interact()
    {
        if (manager == null)
        {
            manager =
                FindFirstObjectByType<InvestigationManager>();
        }

        if (manager == null)
        {
            Debug.LogError(
                "InvestigationManager not found."
            );

            return;
        }

        manager.InteractWithObject(
            objectStage
        );
    }
}