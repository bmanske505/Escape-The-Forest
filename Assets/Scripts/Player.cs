using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
  [Header("Movement")]
  public float moveSpeed = 5f;
  public float mouseSensitivity = 120f;

  [Header("Look")]
  public Transform cameraPivot;
  public float minPitch = -80f;
  public float maxPitch = 80f;

  [Header("Sibling")]
  public GameObject siblingPrefab;
  public Vector3 siblingSpawnOffset = new Vector3(0f, 0f, 0f);
  private Flashlight flashlight;

  // Movement
  private CharacterController controller;
  private Vector3 velocity;
  private float pitch;


  // Input Actions
  private Vector2 moveInput;
  private Vector2 lookInput;

  void Awake()
  {
    controller = GetComponent<CharacterController>();
  }

  void Start()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    // Create flashlight
    flashlight = gameObject.GetComponentInChildren<Flashlight>();

    // Spawn sibling prefab as a sibling (same parent)
    if (siblingPrefab != null)
    {
      Vector3 spawnPos = transform.position + siblingSpawnOffset;

      Instantiate(
          siblingPrefab,
          spawnPos,
          Quaternion.identity,
          transform.parent   // 👈 THIS is what makes it a sibling
      );
    }
    else
    {
      Debug.LogWarning("Player: No siblingPrefab assigned.");
    }
  }

  public void OnMove(InputValue value)
  {
    moveInput = value.Get<Vector2>();
  }

  public void OnLook(InputValue value)
  {
    lookInput = value.Get<Vector2>();
  }

  public void OnFlashlight(InputValue value)
  {
    flashlight.Toggle();
  }

  void Update()
  {
    HandleLook();
  }

  void FixedUpdate()
  {
    HandleMovement();
  }


  void HandleMovement()
  {
    Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
    move = Vector3.ClampMagnitude(move, 1f);

    Vector3 worldMove = transform.TransformDirection(move);

    // Apply gravity
    if (controller.isGrounded && velocity.y < 0)
      velocity.y = 0f;

    velocity.y += Physics.gravity.y * Time.fixedDeltaTime;

    Vector3 delta = worldMove * moveSpeed * Time.fixedDeltaTime;
    delta.y = velocity.y * Time.fixedDeltaTime;

    controller.Move(delta);
  }



  void HandleLook()
  {
    pitch -= lookInput.y * mouseSensitivity * Time.deltaTime;
    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

    float yaw = lookInput.x * mouseSensitivity * Time.deltaTime;
    transform.Rotate(0f, yaw, 0f, Space.World);
  }
}
