using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
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


  private CharacterController controller;
  private Flashlight flashlight;

  // Input Actions
  private PlayerInput input;
  private Vector2 moveInput;
  private Vector2 lookInput;

  private float pitch;

  void Awake()
  {
    input = GetComponent<PlayerInput>();
  }

  void Start()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    // Create flashlight
    flashlight = gameObject.AddComponent<Flashlight>();
    flashlight.AttachTo(cameraPivot);

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

  public void OnMove(InputAction.CallbackContext context)
  {
    moveInput = context.ReadValue<Vector2>();
  }

  public void OnLook(InputAction.CallbackContext context)
  {
    lookInput = context.ReadValue<Vector2>();
  }

  public void OnFlashlight(InputAction.CallbackContext context)
  {
    if (!context.started)
    {
      return;
    }
    flashlight.Toggle();
  }

  void Update()
  {
    HandleLook();
    HandleMovement();
  }

  void HandleMovement()
  {
    Vector2 input = moveInput;

    Vector3 move =
        transform.right * input.x +
        transform.forward * input.y;

    controller.Move(move * moveSpeed * Time.deltaTime);
  }

  void HandleLook()
  {
    Vector2 mouseDelta = lookInput * mouseSensitivity * Time.deltaTime;

    // Horizontal look (player body)
    transform.Rotate(Vector3.up * mouseDelta.x);

    // Vertical look (camera)
    pitch -= mouseDelta.y;
    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
  }
}
