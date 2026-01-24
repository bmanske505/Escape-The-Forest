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
  private InputAction moveAction;
  private InputAction lookAction;
  private InputAction flashlightAction;

  private float pitch;

  void Awake()
  {
    controller = GetComponent<CharacterController>();

    // ================= INPUT SETUP =================
    moveAction = new InputAction("Move", InputActionType.Value);
    moveAction.AddCompositeBinding("2DVector")
        .With("Up", "<Keyboard>/w")
        .With("Down", "<Keyboard>/s")
        .With("Left", "<Keyboard>/a")
        .With("Right", "<Keyboard>/d");

    moveAction.AddCompositeBinding("2DVector")
        .With("Up", "<Keyboard>/upArrow")
        .With("Down", "<Keyboard>/downArrow")
        .With("Left", "<Keyboard>/leftArrow")
        .With("Right", "<Keyboard>/rightArrow");

    lookAction = new InputAction("Look", InputActionType.Value);
    lookAction.AddBinding("<Mouse>/delta");

    flashlightAction = new InputAction("Flashlight", InputActionType.Button);
    flashlightAction.AddBinding("<Keyboard>/space");
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


  void OnEnable()
  {
    moveAction.Enable();
    lookAction.Enable();
    flashlightAction.Enable();
  }

  void OnDisable()
  {
    moveAction.Disable();
    lookAction.Disable();
    flashlightAction.Disable();
  }

  void Update()
  {
    HandleLook();
    HandleMovement();
    HandleFlashlight();
  }

  void HandleMovement()
  {
    Vector2 input = moveAction.ReadValue<Vector2>();

    Vector3 move =
        transform.right * input.x +
        transform.forward * input.y;

    controller.Move(move * moveSpeed * Time.deltaTime);
  }

  void HandleLook()
  {
    Vector2 mouseDelta = lookAction.ReadValue<Vector2>() * mouseSensitivity * Time.deltaTime;

    // Horizontal look (player body)
    transform.Rotate(Vector3.up * mouseDelta.x);

    // Vertical look (camera)
    pitch -= mouseDelta.y;
    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
  }

  void HandleFlashlight()
  {
    if (flashlightAction.IsPressed())
      flashlight.On();
    else
      flashlight.Off();
  }
}
