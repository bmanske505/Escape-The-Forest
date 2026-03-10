using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;

public class Dog : NavMeshEnemy
{
  [Header("Route")]
  [SerializeField] private Transform routeRoot; // Drag DogRoute1, DogRoute2, etc

  Transform[] route;
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
    route = routeRoot
        .GetComponentsInChildren<Transform>()
        .Where(t => t != routeRoot) // exclude parent
        .OrderBy(t => ExtractRouteIndex(t.name))
        .ToArray();
  }

  int ExtractRouteIndex(string name)
  {
    // Matches digits inside parentheses: (1), (23), etc.
    var match = Regex.Match(name, @"\((\d+)\)");

    if (!match.Success)
    {
      Debug.LogError($"'{name}' is missing a route number!");
      return int.MaxValue; // shove bad ones to the end
    }

    return int.Parse(match.Groups[1].Value);
  }

  void Start()
  {
    agent.Warp(route[0].position);
    agent.SetDestination(route[index].position);
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
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
          // The agent has reached the target
          Debug.Log("Destination reached!");
          index = (index + 1) % route.Length;
          agent.SetDestination(route[index].position);
        }
      }
    }
  }
}