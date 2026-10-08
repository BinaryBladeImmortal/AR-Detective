using UnityEngine;

public class FootprintTrailGenerator : MonoBehaviour
{
    [Header("Footprint Template")]
    public GameObject footprintTemplate;

    [Header("Trail Locations")]
    public Transform startLocation;
    public Transform endLocation;

    [Header("Automatic Trail Settings")]
    public float footprintSpacing = 0.35f;
    public int minimumFootprints = 4;
    public int maximumFootprints = 20;

    [Header("Footprint Placement")]
    public float sideOffset = 0.15f;
    public float heightOffset = 0.01f;

    private bool generated = false;

    private void Start()
    {
        // Do NOT generate automatically.
        // ARTapPlacementManager calls GenerateTrail()
        // after the player taps a real horizontal surface.
    }

    public void GenerateTrail()
    {
        if (footprintTemplate == null)
        {
            Debug.LogError("Footprint Template is not assigned.");
            return;
        }

        if (startLocation == null)
        {
            Debug.LogError("Start Location is not assigned.");
            return;
        }

        if (endLocation == null)
        {
            Debug.LogError("End Location is not assigned.");
            return;
        }

        ClearGeneratedFootprints();

        Vector3 start = startLocation.position;
        Vector3 end = endLocation.position;

        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            Debug.LogWarning("Start and End locations are too close.");
            return;
        }

        direction.Normalize();

        Vector3 side =
            Vector3.Cross(Vector3.up, direction).normalized;

        int footprintCount =
            Mathf.RoundToInt(distance / footprintSpacing);

        footprintCount =
            Mathf.Clamp(
                footprintCount,
                minimumFootprints,
                maximumFootprints
            );

        Debug.Log(
            "FOOTPRINT TRAIL GENERATED | Distance: " +
            distance +
            " | Footprints: " +
            footprintCount
        );

        // Remember the template's original rotation.
        // Footprint1 uses X = -90° so it lies flat on the floor.
        Quaternion templateRotation =
            footprintTemplate.transform.localRotation;

        for (int i = 0; i < footprintCount; i++)
        {
            float progress;

            if (footprintCount == 1)
                progress = 0f;
            else
                progress = (float)i / (footprintCount - 1);

            Vector3 position =
                Vector3.Lerp(start, end, progress);

            float sideDirection =
                (i % 2 == 0) ? 1f : -1f;

            position +=
                side * sideOffset * sideDirection;

            position.y += heightOffset;

            // Rotate the trail direction,
            // then preserve Footprint1's original -90° X rotation.
            Quaternion trailRotation =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up
                );

            Quaternion finalRotation =
                trailRotation * templateRotation;

            GameObject footprint =
                Instantiate(
                    footprintTemplate,
                    position,
                    finalRotation
                );

            footprint.name =
                "GeneratedFootprint_" + (i + 1);

            footprint.transform.SetParent(transform);
        }

        // Hide the original template.
        footprintTemplate.SetActive(false);

        generated = true;

        Debug.Log("FOOTPRINT TRAIL READY");
    }

    public void ClearGeneratedFootprints()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child =
                transform.GetChild(i);

            if (child == null)
                continue;

            if (child == footprintTemplate.transform)
                continue;

            if (child == startLocation)
                continue;

            if (child == endLocation)
                continue;

            if (child.name.StartsWith("GeneratedFootprint_"))
            {
                Destroy(child.gameObject);
            }
        }

        generated = false;
    }

    public void ResetTrail()
    {
        ClearGeneratedFootprints();

        if (footprintTemplate != null)
            footprintTemplate.SetActive(false);

        generated = false;
    }
}