using System.Collections;
using Newtonsoft.Json;
using Scripts.FirebaseScripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Sibling : MonoBehaviour
{
  public static Sibling Instance;

  [Header("Movement")]
  public float walkSpeed = 0.8f;
  public float runSpeed = 2f;

  [Header("Hiding")]

  public float lostRadius = 5f; // The max distance the player can stray from sibling before being considered "abandoned"
  public float lostTimeMax = 5f;   // Seconds between sibling getting abandoned and going into hiding
  public Vector2 hideRange = new Vector2(0, 10); // Min and max for hiding

  [Header("Agent")]
  float repathTimer;
  public float repathRate = 0.25f;  // update path 4x/sec
  public float repathThreshold = 0.5f; // only update if player moved this much

  private NavMeshAgent agent;
  private Animator anim;
  private Collectible collectible;
  private AudioSource audioSrc;

  // Data
  static int numberTimesLost = 0;

  private float lostTimer = 0f; // used with lostTimeMax
  private float hidingTimer = 0f; // used to log amount of time the sibling was hiding for before found

  public enum State { Hiding, Following, Fleeing };
  public State state { get; private set; } = State.Following;

  void Awake()
  {
    Instance = this;
    agent = GetComponent<NavMeshAgent>();
    audioSrc = GetComponent<AudioSource>();
    anim = GetComponentInChildren<Animator>();
    collectible = GetComponentInChildren<Collectible>();

    agent.updateRotation = false; // set manually in update for snappy rotation
  }

  void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
  }

  void Start()
  {
    SetState(State.Following);
  }

  void Update()
  {
    if (agent.velocity.sqrMagnitude > 0.1f)
    {
      // Calculate direction and rotate instantly
      Quaternion lookRotation = Quaternion.LookRotation(agent.velocity);
      transform.rotation = lookRotation;
    }

    anim.SetFloat("speed", agent.velocity.magnitude, 0.5f, Time.deltaTime);

    switch (state)
    {
      case State.Following:
        if (agent.isOnNavMesh)
        {
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
        // --- Check if stuck or sufficiently far from player
        if ((transform.position - Player.Instance.transform.position).magnitude > lostRadius)
        {
          lostTimer += Time.deltaTime;
          if (lostTimer >= lostTimeMax)
          {
            Hide("abandoned");
            lostTimer = 0f;
          }
        }
        else
        {
          lostTimer = 0f;
        }
        break;
      case State.Hiding:
        hidingTimer += Time.deltaTime;
        return;
      case State.Fleeing:
        return;
    }
  }

  public void Hide(string context)
  {
    string tag = "HidingSpot";
    GameObject[] hidingSpots = GameObject.FindGameObjectsWithTag(tag);
    Vector3 spot;

    // Specified behavior if none found
    if (hidingSpots == null || hidingSpots.Length == 0)
    {
      Debug.LogWarning($"No GameObjects found with tag '{tag}'. Using fallback position.");
      spot = Utilities.GetNavTarget(Player.Instance.transform.position, hideRange, agent);
    }
    else
    {
      int index = Random.Range(0, hidingSpots.Length);
      spot = hidingSpots[index].transform.position;
    }

    Hide(context, spot);
  }

  public void Hide(string context, Vector3 spot)
  {
    SetState(State.Fleeing);
    agent.SetDestination(spot);
    StartCoroutine(WaitForArrivalThenHide());
    FirebaseAnalytics.LogDocument("sibling_hid", new { cause = context });
  }


  public void Unhide()
  {
    FirebaseAnalytics.LogDocument("sibling_found", new { time_spent = hidingTimer });
    hidingTimer = 0f;

    GameUI.Instance?.ShowBanner("\"There you are! Don't go running off again, you hear me?\"");
    SetState(State.Following);
  }

  public void Respond()
  {
    audioSrc.Play();
    FirebaseAnalytics.LogDocument("echo_used", new { distance = (Player.Instance.transform.position - transform.position).magnitude });
  }

  void SetState(State newState)
  {
    anim.SetBool("hiding", newState == State.Hiding);

    state = newState;

    switch (state)
    {
      case State.Following:
        collectible.SetActive(false);
        agent.speed = walkSpeed;
        break;

      case State.Fleeing:
        collectible.SetActive(false);
        agent.isStopped = false;
        agent.speed = runSpeed;
        break;

      case State.Hiding:
        collectible.SetActive(true);
        agent.ResetPath();
        agent.speed = 0f;
        break;
    }
  }

  IEnumerator WaitForArrivalThenHide()
  {
    // Wait until path is calculated
    yield return new WaitUntil(() => agent.pathPending == false);

    // Wait until sibling reaches hiding spot
    yield return new WaitUntil(() =>
        agent.remainingDistance <= agent.stoppingDistance &&
        agent.velocity.sqrMagnitude < 0.01f
    );
    SetState(State.Hiding);

    numberTimesLost += 1;
    if (numberTimesLost >= 2 && !Player.Instance.HasItem("echo")) // this is the second time they've lost sibling, let them echo!
    {
      GameUI.Instance?.ShowTutorialPopup("Echo", "I keep losing my little sibling! Maybe I should try calling out to them with SPACEBAR.");
      Player.Instance.AddToInventory("Echo");
    }

    GameUI.Instance?.ShowBanner(
        "\"Playing hide and seek again? Where did you go, little sibling?\""
    );
  }
}
