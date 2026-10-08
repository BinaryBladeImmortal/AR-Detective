using UnityEngine;

public class EvidenceInteraction : MonoBehaviour
{
    public EvidenceManager evidenceManager;

    private Evidence evidence;

    void Start()
    {
        evidence = GetComponent<Evidence>();

        Debug.Log("Evidence Interaction Ready: " + gameObject.name);
    }

    void OnMouseDown()
    {
        Debug.Log("CLICKED: " + gameObject.name);

        Discover();
    }

    void Update()
    {
        // Mobile touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                CheckRaycast(touch.position);
            }
        }

        // PC mouse
        if (Input.GetMouseButtonDown(0))
        {
            CheckRaycast(Input.mousePosition);
        }
    }

    void CheckRaycast(Vector2 screenPosition)
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EvidenceInteraction clickedInteraction =
                hit.collider.GetComponentInParent<EvidenceInteraction>();

            if (clickedInteraction == this)
            {
                Debug.Log("CLICKED THROUGH CHILD: " + gameObject.name);

                Discover();
            }
        }
    }

    void Discover()
    {
        if (evidenceManager == null)
        {
            Debug.LogError(
                "Please assign EvidenceManager to " + gameObject.name
            );
            return;
        }

        if (evidence == null)
        {
            Debug.LogError(
                "Evidence component missing on " + gameObject.name
            );
            return;
        }

        evidenceManager.DiscoverEvidence(evidence);
    }
}