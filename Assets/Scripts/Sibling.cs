using Newtonsoft.Json;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Sibling : MonoBehaviour
{
  public static Sibling Instance;

  public float lostRadius = 5f; // The max distance the player can stray from sibling before being considered "abandoned"
  public float lostTimeMax = 3f;   // Seconds between sibling getting abandoned and going into hiding
  public Vector2 hideRange = new Vector2(0, 10); // Min and max for hiding

  private Transform player;
  private NavMeshAgent agent;
  private Collectible collectible;
  private AudioSource audioSrc;

  // Data
  static int numberTimesLost = 0;

  private float lostTimer = 0f; // used with lostTimeMax
  private float hidingTimer = 0f; // used to log amount of time the sibling was hiding for before found
  public bool IsHiding { get; private set; } = false;

  void Awake()
  {
    Instance = this;
    agent = GetComponent<NavMeshAgent>();
    audioSrc = GetComponent<AudioSource>();
  }

  void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
  }

  void Start()
  {
    collectible = GetComponentInChildren<Collectible>();

    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    if (playerObj == null)
    {
      Debug.LogError("Sibling: No GameObject with tag 'Player' found in the scene.");
      enabled = false;
      return;
    }
    player = playerObj.transform;
  }

  void Update()
  {
    if (player == null) { return; }
    else if (IsHiding)
    {
      hidingTimer += Time.deltaTime;
      return;
    }

    float currentDistance = Vector3.Distance(transform.position, player.position);

    // Only move if farther than stopping distance
    if (currentDistance > agent.stoppingDistance)
    {
      agent.SetDestination(player.position);
    }
    else
    {
      // Player is within follow distance, stop moving
      agent.ResetPath();
    }

    // --- Check if stuck or sufficiently far from player
    if ((transform.position - player.position).magnitude > lostRadius)
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
    Vector3 spot = Utilities.GetNavTarget(player.transform.position, hideRange, agent);

    // Warp FIRST so no accidental pickup
    agent.Warp(spot);
    agent.ResetPath();

    IsHiding = true;

    // Now it’s safe to be collectible again
    collectible.SetActive(true);

    GameUI.Instance.ShowBanner("\"Playing hide and seek again? Where did you go, little sibling?\"");

    numberTimesLost += 1;
    if (numberTimesLost == 2) // this is the second time they've lost sibling, let them echo!
    {
      GameUI.Instance.ShowTutorialPopup("Echo", "I keep losing my little sibling! Maybe I should try calling out to them with SPACEBAR.");
      Player.Instance.AddToInventory("Echo");
    }

    FirebaseAnalytics.LogDocument("sibling_hid", JsonConvert.SerializeObject(new { level = LevelMaster.Instance.GetLevel(), cause = context }));
  }


  public void Unhide()
  {
    FirebaseAnalytics.LogDocument("sibling_found", JsonConvert.SerializeObject(new { time_spent = hidingTimer }));
    hidingTimer = 0f;

    GameUI.Instance.ShowBanner("\"There you are! Don't go running off again, you hear me?\"");
    IsHiding = false;
    collectible.SetActive(false);
  }

  public void Respond()
  {
    audioSrc.Play();
    FirebaseAnalytics.LogDocument("echo_used", JsonConvert.SerializeObject(new { level = LevelMaster.Instance.GetLevel() }));
  }
}
