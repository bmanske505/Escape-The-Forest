using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.AI;

public class Stalker : NavMeshEnemy
{
  private Transform player;

  private Vector3 lastKnownPlayerPosition;
  private float wanderTimer;

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

    player = GameObject.FindGameObjectWithTag("Player").transform;

    CurrentState = State.Walking;
  }

  protected override void Update()
  {
    base.Update();
    switch (CurrentState)
    {
      case State.Walking:
        agent.SetDestination(Player.Instance.transform.position);
        break;
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