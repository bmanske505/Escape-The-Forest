using System.Collections;
using Scripts.FirebaseScripts;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
  protected enum State { Wandering, Chasing, Searching, Stunned }
  protected State state;
  public void Stun(float duration)
  {
    if (state == State.Stunned) return; // prevent chain stuns
    FirebaseAnalytics.LogDocument("enemy_stunned", new { type = GetType().Name });
    StartCoroutine(StunRoutine(duration));
  }

  void OnTriggerEnter(Collider other)
  {
    if (state == State.Stunned) return;
    if (other.CompareTag("Player"))
    {
      // Change so that player spawns at the start of the maze and the stalker resets
      FirebaseAnalytics.LogDocument("player_died", new { type = GetType().Name });
      GameUI.Instance?.ShowDeathPopup();
    }
    /*
    else if (other.CompareTag("Sibling"))
    {
      other.GetComponent<Sibling>().Hide(GetType().Name);
    }*/
  }

  protected abstract IEnumerator StunRoutine(float duration);
}