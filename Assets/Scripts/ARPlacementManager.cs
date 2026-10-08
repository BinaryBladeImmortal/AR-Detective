using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    [Header("Crime Scene")]
    public GameObject evidencePrefab;

    private ARRaycastManager raycastManager;

    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    private GameObject placedCrimeScene;

    private bool placementLocked = false;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        if (raycastManager == null)
        {
            Debug.LogError(
                "ARPlacementManager requires an ARRaycastManager on the same GameObject."
            );
        }
    }

    void Update()
    {
        // Crime scene has already been placed.
        if (placementLocked)
            return;

        // Safety checks.
        if (raycastManager == null)
            return;

        if (evidencePrefab == null)
        {
            Debug.LogError("CrimeScene prefab is not assigned.");
            return;
        }

        // No touch.
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        // Only react to the first moment of the touch.
        if (touch.phase != TouchPhase.Began)
            return;

        // Only detect horizontal planes.
        bool foundPlane = raycastManager.Raycast(
            touch.position,
            hits,
            TrackableType.PlaneWithinPolygon
        );

        if (!foundPlane || hits.Count == 0)
        {
            Debug.Log("No valid horizontal plane found.");
            return;
        }

        // Use the first valid plane hit.
        Pose hitPose = hits[0].pose;

        // Create the crime scene exactly once.
        placedCrimeScene = Instantiate(
            evidencePrefab,
            hitPose.position,
            hitPose.rotation
        );

        placementLocked = true;

        Debug.Log(
            "Crime Scene placed successfully and placement locked."
        );
    }
}