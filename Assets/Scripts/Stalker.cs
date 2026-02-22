using System.Collections;
using Newtonsoft.Json;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.AI;

public class Stalker : MonoBehaviour
{

  [Header("Movement")]
  private float wanderSpeed; // pulled from navmesh
  public float chaseSpeed = 3f;
  public Vector2 wanderRange;
  public float wanderInterval = 3f;

  [Header("Vision")]
  public float viewDistance = 15f;
  public float viewAngle = 90f;
  public LayerMask obstacleMask;

  private NavMeshAgent agent;
  private Transform player;

  private Vector3 lastKnownPlayerPosition;
  private float wanderTimer;

  private enum State { Wandering, Chasing, Searching, Stunned }
  private State currentState;

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    player = GameObject.FindGameObjectWithTag("Player").transform;

    currentState = State.Wandering;
    wanderTimer = wanderInterval;
    wanderSpeed = agent.speed;
  }

  void Update()
  {
    switch (currentState)
    {
      case State.Wandering:
        Wander();
        LookForPlayer();
        break;

      case State.Chasing:
        ChasePlayer();
        break;

      case State.Searching:
        SearchLastKnownPosition();
        LookForPlayer();
        break;
    }
  }

  // ================= STATES =================

  void Wander()
  {
    wanderTimer += Time.deltaTime;

    if (wanderTimer >= wanderInterval)
    {
      Vector3 newPos = Utilities.GetNavTarget(transform.position, wanderRange, agent);
      agent.SetDestination(newPos);
      wanderTimer = 0f;
    }
  }

  void ChasePlayer()
  {
    agent.speed = chaseSpeed;
    agent.SetDestination(player.position);

    if (CanSeePlayer())
    {
      lastKnownPlayerPosition = player.position;
    }
    else
    {
      agent.speed = wanderSpeed;
      agent.SetDestination(lastKnownPlayerPosition);
      currentState = State.Searching;
    }
  }

  void SearchLastKnownPosition()
  {
    if (!agent.pathPending && agent.remainingDistance <= 0.5f)
    {
      currentState = State.Wandering;
      wanderTimer = wanderInterval;
    }
  }

  // ================= VISION =================

  void LookForPlayer()
  {
    if (CanSeePlayer())
    {
      GameUI.Instance.ShowBanner("\"RUN\"");
      currentState = State.Chasing;
      lastKnownPlayerPosition = player.position;
    }
  }

  bool CanSeePlayer()
  {
    Vector3 directionToPlayer = (player.position - transform.position).normalized;
    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

    if (distanceToPlayer > viewDistance)
      return false;

    float angle = Vector3.Angle(transform.forward, directionToPlayer);
    if (angle > viewAngle / 2f)
      return false;

    if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer,
        distanceToPlayer, obstacleMask))
    {
      return false;
    }

    return true;
  }

  // ================= PLAYER CATCH =================

  void OnTriggerEnter(Collider other)
  {
    if (currentState == State.Stunned) return;
    if (other.CompareTag("Player"))
    {
      // Change so that player spawns at the start of the maze and the stalker resets
      FirebaseAnalytics.LogEventParameter("player_died", JsonConvert.SerializeObject(new { level = LevelMaster.Instance.GetLevel() }));
      GameUI.Instance.ShowDeathPopup();
    }
    else if (other.CompareTag("Sibling"))
    {
      other.GetComponent<Sibling>().Hide("stalker");
    }
  }

  public void Stun(float duration)
  {
    if (currentState == State.Stunned) return; // prevent chain stuns
    FirebaseAnalytics.LogEventParameter("stalker_stunned", JsonConvert.SerializeObject(new { level = LevelMaster.Instance.GetLevel()}));
    StartCoroutine(StunRoutine(duration));
  }

  private IEnumerator StunRoutine(float duration)
  {
    currentState = State.Stunned;
    agent.isStopped = true;
    agent.ResetPath(); // optional: immediately clear current destination

    // Teleport to random location after stun
    Vector3 randomPos = Utilities.GetNavTarget(transform.position, wanderRange, agent);
    agent.Warp(randomPos);

    yield return new WaitForSeconds(duration);

    currentState = State.Wandering;
    agent.speed = wanderSpeed; // <-- reset speed
    agent.isStopped = false;
  }
}