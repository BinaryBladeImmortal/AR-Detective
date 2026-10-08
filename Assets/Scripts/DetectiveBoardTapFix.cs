using UnityEngine;
using UnityEngine.InputSystem;

public class DetectiveBoardTapFix : MonoBehaviour
{
    private InvestigationManager investigationManager;

    private void Start()
    {
        investigationManager =
            FindFirstObjectByType<InvestigationManager>();
    }

    private void Update()
    {
        if (Touchscreen.current == null)
            return;

        if (!Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return;

        Vector2 touchPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();

        if (Camera.main == null)
            return;

        Ray ray =
            Camera.main.ScreenPointToRay(touchPosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            InvestigationObject obj =
                hit.collider.GetComponentInParent<InvestigationObject>();

            if (obj != null)
            {
                Debug.Log("DETECTIVE BOARD TAPPED");

                if (investigationManager != null)
                {
                    investigationManager.StartInvestigation();
                    obj.Interact();
                }
            }
        }
    }
}