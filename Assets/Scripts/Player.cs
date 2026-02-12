using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Singleton<Player>
{
  [Header("Movement")]
  public float moveSpeed = 2f;
  public float mouseSensitivity = 120f;
  public float sprintSpeed = 4f;

  [Header("Look")]
  public Transform cameraPivot;
  public float minPitch = -80f;
  public float maxPitch = 80f;

  [Header("Sibling")]
  public GameObject siblingPrefab;
  public Vector3 siblingSpawnOffset = new Vector3(0f, 0f, 0f);
  private Flashlight flashlight;

  private float pitch;

  // Input Actions
  private Vector2 moveInput;
  private Vector2 lookInput;

  public Vector2 MoveInput => moveInput;

  void OnEnable()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    // Reset player speed and sprinting state
    GetComponent<PlayerMovement>().SetSprinting(false);
    GetComponent<PlayerMovement>().SetSpeed();

    // Create flashlight
    flashlight = gameObject.GetComponentInChildren<Flashlight>();

    // Spawn sibling prefab as a sibling (same parent)
    if (siblingPrefab != null)
    {
      Vector3 spawnPos = transform.position + siblingSpawnOffset;

      if (Sibling.Instance)
      {
        Sibling.Instance.transform.position = spawnPos;
      } else
      {
        Instantiate(
          siblingPrefab,
          spawnPos,
          Quaternion.identity,
          transform.parent   // 👈 THIS is what makes it a sibling
      );
      }
    }
    else
    {
      Debug.LogWarning("Player: No siblingPrefab assigned.");
    }
  }

  void OnDisable()
  {
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void OnMove(InputValue value)
  {
    moveInput = value.Get<Vector2>();
  }

  public void OnLook(InputValue value)
  {
    lookInput = value.Get<Vector2>();
  }

  public void OnSprint(InputValue value)
  {
      GetComponent<PlayerMovement>()
          .SetSprinting(value.isPressed);
  }

  void Update()
  {
    HandleLook();
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
