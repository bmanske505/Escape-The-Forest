using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Sibling : MonoBehaviour
{
  public float lostRadius = 5f; // The max distance the player can stray from sibling before being considered "abandoned"
  public float lostTimeMax = 3f;   // Seconds between sibling getting abandoned and going into hiding
  public Vector2 hideRange = new Vector2(0, 10); // Min and max for hiding

  private Transform player;
  private NavMeshAgent agent;
  private Collectible collectible;

  private Vector3 lastPosition;
  private float lostTimer = 0f;
  public bool IsHiding { get; private set; } = false;

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
  }

  void Update()
  {
    if (player == null || IsHiding)
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
    if ((transform.position - player.position).magnitude > lostRadius)
    {
      lostTimer += Time.deltaTime;
      if (lostTimer >= lostTimeMax)
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
    Vector3 spot = Utilities.RandomNavSphere(player.transform.position, hideRange);
    agent.Warp(spot);
    agent.ResetPath();

    IsHiding = true;
    collectible.SetActive(true); // can now be picked up by player

    UIMaster.Instance.ShowBanner("Your sibling got scared and ran away! You need to find them.");
  }

  public void Unhide()
  {
    IsHiding = false;
    collectible.SetActive(false);
  }
}
