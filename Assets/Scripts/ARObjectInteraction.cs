using UnityEngine;
using UnityEngine.InputSystem;

public class ARObjectInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public float rayDistance = 100f;

    private void Update()
    {
        if (Touchscreen.current == null)
        {
            return;
        }

        if (!Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return;
        }

        Vector2 touchPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();

        TryInteract(touchPosition);
    }

    private void TryInteract(Vector2 screenPosition)
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found.");
            return;
        }

        Ray ray =
            Camera.main.ScreenPointToRay(screenPosition);

        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                rayDistance
            );

        if (hits.Length == 0)
        {
            return;
        }

        InvestigationObject closestObject = null;

        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            InvestigationObject investigationObject =
                hit.collider.GetComponentInParent<InvestigationObject>();

            if (investigationObject == null)
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestObject = investigationObject;
            }
        }

        if (closestObject != null)
        {
            Debug.Log(
                "Tapped investigation object: " +
                closestObject.objectName
            );

            closestObject.Interact();
        }
    }
}