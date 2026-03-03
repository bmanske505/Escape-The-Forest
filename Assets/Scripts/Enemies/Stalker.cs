using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.AI;

public class Stalker : NavMeshEnemy
{
  [Header("Agent")]
  float repathTimer;
  public float repathRate = 1f;  // update path 1x/sec
  public float repathThreshold = 0.5f; // only update if player moved this much
  private NavMeshPath path;
  private HashSet<GameObject> eyes = new HashSet<GameObject>(); // for stun animation (prototyped)

  void Start()
  {
    foreach (Transform child in transform)
    {
      if (child.name == "Eye")
      {
        eyes.Add(child.gameObject);
      }
    }

    CurrentState = State.Walking;
    path = new NavMeshPath();
  }

  protected override void Update()
  {
    base.Update();
    if (CurrentState != State.Walking || !agent.isOnNavMesh) return;

    if (Time.time < repathTimer) return;
    repathTimer = Time.time + repathRate;

    // Sample with a larger radius - 2f is too tight if player is slightly off-mesh
    NavMeshHit hit;
    if (!NavMesh.SamplePosition(Player.Instance.GetPointOnSurface(), out hit, 5f, NavMesh.AllAreas)) return;

    // Skip if player hasn't moved enough
    if (Vector3.Distance(agent.destination, hit.position) <= repathThreshold) return;

    // Calculate path first to validate it before committing
    if (agent.CalculatePath(hit.position, path))
    {
      if (path.status == NavMeshPathStatus.PathComplete)
      {
        agent.SetPath(path);
      }
      else if (path.status == NavMeshPathStatus.PathPartial)
      {
        // Path is partial - navigate to the closest reachable point instead
        // This keeps the stalker moving toward the player rather than idling
        agent.SetPath(path);
      }
      // PathInvalid: do nothing, keep current path
    }
  }

  public void Stun(float duration)
  {
    if (CurrentState == State.Stunned) return; // prevent chain stuns
    FirebaseAnalytics.LogDocument("stalker_stunned", new { });
    StartCoroutine(StunRoutine(duration));
  }

  protected IEnumerator StunRoutine(float duration)
  {
    foreach (GameObject eye in eyes)
    {
      eye.SetActive(false);
    }

    CurrentState = State.Stunned;
    agent.isStopped = true;

    yield return new WaitForSeconds(duration);

    foreach (GameObject eye in eyes)
    {
      eye.SetActive(true);
    }

    CurrentState = State.Walking;
    agent.isStopped = false;
  }
}