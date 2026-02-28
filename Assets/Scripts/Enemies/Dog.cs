using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;

public class Dog : Enemy
{
  Vector3[] route;
  int index = 1;
  NavMeshAgent agent;

  void Awake()
  {
    agent = GetComponent<NavMeshAgent>();
    BuildRoute();
  }

  void BuildRoute()
  {
    route = GameObject.FindGameObjectsWithTag("DogRouteNode")
        .OrderBy(go => ExtractRouteIndex(go.name))
        .Select(go => go.transform.position)
        .ToArray();
  }

  int ExtractRouteIndex(string name)
  {
    // Matches digits inside parentheses: (1), (23), etc.
    var match = Regex.Match(name, @"\((\d+)\)");

    if (!match.Success)
    {
      Debug.LogError($"DogRouteNode object '{name}' is missing a route number!");
      return int.MaxValue; // shove bad ones to the end
    }

    return int.Parse(match.Groups[1].Value);
  }

  void Start()
  {
    agent.Warp(route[0]);
    agent.SetDestination(route[index % route.Length]);
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
          agent.SetDestination(route[index % route.Length]);
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