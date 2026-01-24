using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Sibling : MonoBehaviour
{
  public float speed = 3f;
  public float distance = 2f;
  public float obstacleAvoidDistance = 1f;
  public float lostTimeThreshold = 3f;

  private Transform player;
  private Rigidbody rb;

  private Vector3 lastPosition;
  private float stuckTimer;

  void Start()
  {
    rb = GetComponent<Rigidbody>();
    rb.isKinematic = true; // So it moves via script but still collides
    rb.constraints = RigidbodyConstraints.FreezeRotation;

    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

    if (playerObj == null)
    {
      Debug.LogError("Sibling: No GameObject with tag 'Player' found in the scene.");
      enabled = false;
      return;
    }

    player = playerObj.transform;
    lastPosition = transform.position;
    stuckTimer = 0f;
  }

  void Update()
  {
    if (player == null)
      return;

    float currentDistance = Vector3.Distance(transform.position, player.position);

    if (currentDistance <= distance)
      return;

    Vector3 direction = (player.position - transform.position).normalized;

    // --- Simple obstacle avoidance ---
    if (Physics.Raycast(transform.position, direction, out RaycastHit hit, obstacleAvoidDistance))
    {
      // Hit something in front
      // Steer slightly left or right
      Vector3 avoidDir = Vector3.Cross(hit.normal, Vector3.up).normalized;
      direction = (direction + avoidDir).normalized;
    }

    // Move using Rigidbody
    rb.MovePosition(transform.position + direction * speed * Time.deltaTime);

    // --- Check if stuck ---
    if ((transform.position - lastPosition).magnitude < 0.01f)
    {
      stuckTimer += Time.deltaTime;
      if (stuckTimer >= lostTimeThreshold)
      {
        Hide();
        stuckTimer = 0f;
      }
    }
    else
    {
      stuckTimer = 0f;
    }

    lastPosition = transform.position;
  }

  private void Hide()
  {
    // TODO: pick a random hiding spot in the maze and move there
    Debug.Log("Sibling is lost! Calling Hide().");
    /*
    This function should choose a random HidingSpot node in the map (these should be added!!) and place the Sibling there
    - May need a boolean value IsHiding or something so that sibling will only follow the player if not hiding...
    */
  }
}
