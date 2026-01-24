using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Sibling : MonoBehaviour
{
  public float lostTimeThreshold = 3f;   // Time stuck before hiding

  private Transform player;
  private NavMeshAgent agent;

  private Vector3 lastPosition;
  private float stuckTimer;

  private bool isHiding = false;         // To handle Hide() state

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();

    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    if (playerObj == null)
    {
      Debug.LogError("Sibling: No GameObject with tag 'Player' found in the scene.");
      enabled = false;
      return;
    }
    player = playerObj.transform;

    lastPosition = transform.position;
    stuckTimer = 0f;
  }

  void Update()
  {
    if (player == null || isHiding)
      return;

    float currentDistance = Vector3.Distance(transform.position, player.position);

    // Only move if farther than stopping distance
    if (currentDistance > agent.stoppingDistance)
    {
      agent.SetDestination(player.position);
    }
    else
    {
      // Player is within follow distance, stop moving
      agent.ResetPath();
    }

    // --- Check if stuck ---
    if ((transform.position - lastPosition).magnitude < 0.01f && !agent.pathPending)
    {
      stuckTimer += Time.deltaTime;
      if (stuckTimer >= lostTimeThreshold)
      {
        Hide();
        stuckTimer = 0f;
      }
    }
    else
    {
      stuckTimer = 0f;
    }

    lastPosition = transform.position;
  }

  private void Hide()
  {
    isHiding = true;
    agent.ResetPath();

    Debug.Log("Sibling is lost! Calling Hide().");

    // TODO: move to a random hiding spot
    // Example:
    // Transform spot = GetRandomHidingSpot();
    // agent.Warp(spot.position);
  }

  // Optional helper
  // private Transform GetRandomHidingSpot() { ... }
}
