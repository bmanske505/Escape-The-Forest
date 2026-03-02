using System.Collections;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.AI;

public abstract class NavMeshEnemy : MonoBehaviour
{
  protected NavMeshAgent agent;

  public enum State { Walking, Stunned }
  public State CurrentState { get; protected set; }
  [SerializeField] string tip;


  protected virtual void Awake()
  {
    agent = GetComponent<NavMeshAgent>();
    agent.updateRotation = false; // set manually in update for snappy rotation
  }

  protected virtual void Update()
  {
    if (agent.velocity.sqrMagnitude > 0.1f)
    {
      // Calculate direction and rotate instantly
      Quaternion lookRotation = Quaternion.LookRotation(agent.velocity);
      transform.rotation = lookRotation;
    }
  }

  void OnTriggerEnter(Collider other)
  {
    if (CurrentState == State.Stunned) return;
    if (other.CompareTag("Player"))
    {
      // Change so that player spawns at the start of the maze and the stalker resets
      FirebaseAnalytics.LogDocument("player_died", new { type = GetType().Name });
      GameUI.Instance?.ShowDeathPopup(tip);
    }
  }
}