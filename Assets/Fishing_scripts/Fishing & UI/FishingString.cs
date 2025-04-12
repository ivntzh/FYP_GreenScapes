using System.Collections.Generic;
using UnityEngine;

public class FishingString : MonoBehaviour
{
    [Header("Rope Settings")]
    public Transform startPoint;       // Starting point (end of fishing rod)
    public Transform endPoint;         // Ending point (bait or hook)
    public int segmentCount = 10;      // Number of segments for the rope
    public float segmentLength = 0.1f; // Distance between each segment
    public float gravity = 0.98f;      // Gravity force applied to the rope
    public float stiffness = 0.2f;     // Tightness of the rope (spring stiffness)
    public float maxRopeLength = 1f;   // Maximum length of the rope

    [Header("Line Renderer Settings")]
    public LineRenderer lineRenderer;  // The LineRenderer component to render the rope
    public Material ropeMaterial;      // Material for the rope

    // New adjustable thickness values for a fishing line (thinner than a typical rope)
    [Tooltip("Set the starting thickness of the fishing line (e.g., 0.005 for a thin line)")]
    public float startThickness = 0.005f;
    [Tooltip("Set the ending thickness of the fishing line (e.g., 0.005 for a uniform line)")]
    public float endThickness = 0.005f;

    private List<Vector3> ropeSegments; // Points of the rope
    private Vector3[] previousPositions; // Positions from the last frame to calculate velocity

    [HideInInspector]
    public bool clampEndPoint = true;  // new: turn off during Pulling

    private void Start()
    {
        // If no LineRenderer is assigned, add one dynamically
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        InitializeRope();
    }

    private void Update()
    {
        SimulateRope();
        RenderRope();
    }

    private void InitializeRope()
    {
        // Initialize previousPositions array with segmentCount elements
        previousPositions = new Vector3[segmentCount];

        // Initialize rope segments list and populate with initial positions
        ropeSegments = new List<Vector3>(segmentCount);

        Vector3 segmentStart = startPoint.position;
        Vector3 segmentEnd = endPoint.position;
        Vector3 direction = (segmentEnd - segmentStart).normalized;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 segmentPos = segmentStart + direction * (segmentLength * i);
            ropeSegments.Add(segmentPos);
            previousPositions[i] = segmentPos;
        }

        // Configure the LineRenderer with the provided material and thickness values
        lineRenderer.positionCount = segmentCount;
        lineRenderer.material = ropeMaterial;
        // Set uniform thickness or set a curve if you prefer tapering
        lineRenderer.startWidth = startThickness;
        lineRenderer.endWidth = endThickness;
    }

    private void SimulateRope()
    {
        // Apply gravity and update segment positions (excluding fixed endpoints)
        for (int i = 1; i < ropeSegments.Count - 1; i++)
        {
            Vector3 velocity = ropeSegments[i] - previousPositions[i];
            previousPositions[i] = ropeSegments[i];
            ropeSegments[i] += velocity; // Apply velocity
            ropeSegments[i] += Vector3.down * gravity * Time.deltaTime; // Apply gravity
        }

        // **only** clamp the endPoint if clampEndPoint is true
        Vector3 rodToBait = endPoint.position - startPoint.position;
        float currentRopeLength = rodToBait.magnitude;
        if (clampEndPoint && currentRopeLength > maxRopeLength)
        {
            Vector3 clampedDirection = rodToBait.normalized;
            endPoint.position = startPoint.position + clampedDirection * maxRopeLength;
        }

        // Apply rope constraints to simulate tension across segments (more iterations for stability)
        for (int i = 0; i < 50; i++)
        {
            ApplyConstraints();
        }

        // Ensure the first and last segments match the startPoint and endPoint respectively
        ropeSegments[0] = startPoint.position;
        ropeSegments[ropeSegments.Count - 1] = endPoint.position;
    }

    private void ApplyConstraints()
    {
        for (int i = 0; i < ropeSegments.Count - 1; i++)
        {
            Vector3 diff = ropeSegments[i + 1] - ropeSegments[i];
            float dist = diff.magnitude;
            float error = segmentLength - dist;
            Vector3 correction = diff.normalized * error * 0.5f;

            if (i != 0)
            {
                ropeSegments[i] -= correction * stiffness;
            }
            if (i != ropeSegments.Count - 2)
            {
                ropeSegments[i + 1] += correction * stiffness;
            }
        }
    }

    private void RenderRope()
    {
        // Ensure the LineRenderer's positionCount equals the number of rope segments
        lineRenderer.positionCount = ropeSegments.Count;
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            lineRenderer.SetPosition(i, ropeSegments[i]);
        }
    }
}
