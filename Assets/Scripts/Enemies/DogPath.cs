using UnityEngine;
using UnityEngine.AI;

public class DogPath : MonoBehaviour
{
    private NavMeshAgent agent;
    private LineRenderer lineRenderer;

    void Start()
    {
        if (PlayerPrefs.GetInt("ab_group") == 0)
        {
            this.enabled = false;
        }
        
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>();

        // Ensure LineRenderer is configured (e.g., width, material in Inspector)
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component missing from GameObject");
            // Add component if not present (optional)
            // lineRenderer = gameObject.AddComponent<LineRenderer>(); 
        }
    
    }

    void Update()
    {
        // Check if the agent has a path
        if (agent.hasPath)
        {
            // Get the corners of the path
            Vector3[] corners = agent.path.corners;

            // Set the number of positions in the LineRenderer to the number of corners
            lineRenderer.positionCount = corners.Length;

            // Set the positions of the LineRenderer to the path corners
            lineRenderer.SetPositions(corners);

            lineRenderer.widthMultiplier = 0.1f;
        }
        else
        {
            // Optional: Hide the line if there is no current path
            lineRenderer.positionCount = 0;
        }
    }
}
