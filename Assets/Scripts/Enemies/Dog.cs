using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Dog : Enemy
{
  public Transform[] route;
  int index = 1;
  NavMeshAgent agent;

  void Awake()
  {
    agent = GetComponent<NavMeshAgent>();
  }

  void Start()
  {
    agent.Warp(route[0].position);
    agent.SetDestination(route[index % route.Length].position);
  }

  void Update()
  {
    if (state == State.Stunned) return;

    // Check if a path is pending (still being calculated)
    if (!agent.pathPending)
    {
      // Check if the remaining distance is less than the stopping distance
      if (agent.remainingDistance <= agent.stoppingDistance)
      {
        // Also check if the agent has stopped moving or has no path
        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
        {
          // The agent has reached the target
          Debug.Log("Destination reached!");
          index += 1;
          agent.SetDestination(route[index % route.Length].position);
        }
      }
    }
  }

  protected override IEnumerator StunRoutine(float duration)
  {
    state = State.Stunned;
    agent.isStopped = true;

    yield return new WaitForSeconds(duration);

    state = State.Wandering;
    agent.isStopped = false;
  }

}