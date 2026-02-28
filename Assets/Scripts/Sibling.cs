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

  private NavMeshAgent agent;
  private Animator anim;
  private Collectible collectible;
  private AudioSource audioSrc;

  // Data
  static int numberTimesLost = 0;

  private float lostTimer = 0f; // used with lostTimeMax
  private float hidingTimer = 0f; // used to log amount of time the sibling was hiding for before found

  public enum State { Hiding, Walking, Running };
  public State state { get; private set; } = State.Walking;

  void Awake()
  {
    Instance = this;
    agent = GetComponent<NavMeshAgent>();
    audioSrc = GetComponent<AudioSource>();
    anim = GetComponentInChildren<Animator>();
    collectible = GetComponentInChildren<Collectible>();
  }

  void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
  }

  void Start()
  {
    SetState(State.Walking);
  }

  void Update()
  {
    anim.SetFloat("speed", agent.velocity.magnitude, 0.5f, Time.deltaTime);

    if (state == State.Hiding)
    {
      hidingTimer += Time.deltaTime;
      return;
    }
    else if (state == State.Running)
    {
      return;
    }

    // atp, state == State.Walking

    float currentDistance = Vector3.Distance(transform.position, Player.Instance.transform.position);

    // Only move if farther than stopping distance
    if (currentDistance > agent.stoppingDistance)
    {
      agent.SetDestination(Player.Instance.transform.position);
    }
    else
    {
      // Player is within follow distance, stop moving
      agent.ResetPath();
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
    SetState(State.Running);
    agent.SetDestination(spot);
    StartCoroutine(WaitForArrivalThenHide());
    FirebaseAnalytics.LogDocument("sibling_hid", new { cause = context });
  }


  public void Unhide()
  {
    FirebaseAnalytics.LogDocument("sibling_found", new { time_spent = hidingTimer });
    hidingTimer = 0f;

    GameUI.Instance?.ShowBanner("\"There you are! Don't go running off again, you hear me?\"");
    SetState(State.Walking);
  }

  public void Respond()
  {
    audioSrc.Play();
    FirebaseAnalytics.LogDocument("echo_used", new { });
  }

  void SetState(State newState)
  {
    anim.SetBool("hiding", newState == State.Hiding);

    state = newState;

    switch (state)
    {
      case State.Walking:
        collectible.SetActive(false);
        agent.speed = walkSpeed;
        break;

      case State.Running:
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
    if (numberTimesLost == 2) // this is the second time they've lost sibling, let them echo!
    {
      GameUI.Instance?.ShowTutorialPopup("Echo", "I keep losing my little sibling! Maybe I should try calling out to them with SPACEBAR.");
      Player.Instance.AddToInventory("Echo");
    }

    GameUI.Instance?.ShowBanner(
        "\"Playing hide and seek again? Where did you go, little sibling?\""
    );
  }
}
