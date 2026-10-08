using UnityEngine;
using UnityEngine.InputSystem;

public class InvestigationObjectTap : MonoBehaviour
{
    private InvestigationObject investigationObject;

    private void Awake()
    {
        investigationObject =
            GetComponent<InvestigationObject>();

        if (investigationObject == null)
        {
            investigationObject =
                GetComponentInParent<InvestigationObject>();
        }
    }

    private void Update()
    {
        if (Touchscreen.current == null)
            return;

        if (!Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return;

        Vector2 touchPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();

        Ray ray =
            Camera.main.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            InvestigationObject tappedObject =
                hit.collider.GetComponentInParent<InvestigationObject>();

            if (tappedObject == investigationObject)
            {
                investigationObject.Interact();
            }
        }
    }
}