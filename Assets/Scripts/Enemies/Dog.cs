using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;

public class Dog : NavMeshEnemy
{
  Vector3[] route;
  int index = 1;
  Animator anim;

  protected override void Awake()
  {
    base.Awake();
    anim = GetComponentInChildren<Animator>();
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
    agent.SetDestination(route[index]);
  }

  protected override void Update()
  {
    base.Update();
    anim.speed = agent.speed / 2f;
    if (CurrentState == State.Stunned) return;

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
          index = (index + 1) % route.Length;
          agent.SetDestination(route[index]);
        }
      }
    }
  }
}