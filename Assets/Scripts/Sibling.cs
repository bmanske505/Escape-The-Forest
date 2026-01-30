using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Sibling : MonoBehaviour
{
  public float lostTimeThreshold = 3f;   // Time stuck before hiding
  public float followRadius = 5f; // The distance within which the sibling is considered "following" the player

  private Transform player;
  private NavMeshAgent agent;
  private Collectible collectible;

  private Vector3 lastPosition;
  private float lostTimer = 0f;
  private bool isHiding = false;         // To handle Hide() state
  private Vector3[] hidingSpots;

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    collectible = GetComponentInChildren<Collectible>();

    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    if (playerObj == null)
    {
      Debug.LogError("Sibling: No GameObject with tag 'Player' found in the scene.");
      enabled = false;
      return;
    }
    player = playerObj.transform;

    lastPosition = transform.position;
    hidingSpots = Array.ConvertAll(GameObject.FindGameObjectsWithTag("HidingSpot"), obj => obj.transform.position);
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

    // --- Check if stuck or sufficiently far from player
    if ((transform.position - player.position).magnitude > followRadius)
    {
      lostTimer += Time.deltaTime;
      if (lostTimer >= lostTimeThreshold)
      {
        Hide();
        lostTimer = 0f;
      }
    }
    else
    {
      lostTimer = 0f;
    }

    lastPosition = transform.position;
  }

  public void Hide()
  {
    Vector3 spot = hidingSpots[UnityEngine.Random.Range(0, hidingSpots.Length)];
    agent.Warp(spot);
    agent.ResetPath();

    isHiding = true;
    collectible.SetActive(true); // can now be picked up by player

    Debug.Log("Sibling is lost!");
  }

  public void Unhide()
  {
    isHiding = false;
    collectible.SetActive(false);
  }
}
