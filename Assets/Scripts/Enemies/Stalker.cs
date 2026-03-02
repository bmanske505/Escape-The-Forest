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
  public float repathRate = 0.25f;  // update path 4x/sec
  public float repathThreshold = 0.5f; // only update if player moved this much

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
  }

  protected override void Update()
  {
    base.Update();
    if (CurrentState != State.Walking || !agent.isOnNavMesh) return;

    // Project player onto NavMesh
    NavMeshHit hit;
    if (!NavMesh.SamplePosition(Player.Instance.transform.position, out hit, 2f, NavMesh.AllAreas)) return;

    // Only update path if player moved enough or timer expired
    if (Vector3.Distance(agent.destination, hit.position) > repathThreshold || Time.time > repathTimer)
    {
      agent.SetDestination(hit.position);
      repathTimer = Time.time + repathRate;
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