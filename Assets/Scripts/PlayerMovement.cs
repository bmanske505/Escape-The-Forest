using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
  [Header("Stamina")]

  public float walkSpeed = 2f;
  public float sprintSpeed = 4f;

  public float maxStamina = 100f;
  public float staminaDrainPerSecond = 25f;
  public float staminaRegenPerSecond = 15f;
  public float regenDelay = 1.5f;

  private CharacterController controller;
  private float stamina;

  private float regenTimer;

  [Header("Input")]

  private InputAction sprintAction;
  bool sprinting;

  private InputAction moveAction;

  void Awake()
  {
    controller = GetComponent<CharacterController>();
    stamina = maxStamina;

    sprintAction = InputSystem.actions.FindAction("Sprint");
    moveAction = InputSystem.actions.FindAction("Move");
  }

  /**
  void OnEnable()
  {
    sprintAction.started += OnSprint;
    sprintAction.canceled += OnSprint;
    moveAction.performed += OnMove;
    moveAction.canceled += OnMove;
  }

  void OnDisable()
  {
    sprintAction.started -= OnSprint;
    sprintAction.canceled -= OnSprint;
    moveAction.performed -= OnMove;
    moveAction.canceled += OnMove;
  }

  private void OnSprint(CallbackContext context)
  {
    if (context.started)
    {
      SetSprinting(true);
    }
    else if (context.canceled)
    {
      SetSprinting(false);
    }
  }

  private void OnMove(CallbackContext context)
  {
    moveInput = context.ReadValue<Vector2>();
  }
  */

  void Update()
  {
    HandleStamina();
    Debug.Log(sprintAction.IsPressed());
    SetSprint(sprintAction.IsPressed());
  }

  void FixedUpdate()
  {
    HandleMovement();
  }

  void HandleMovement()
  {
    Vector2 moveInput = moveAction.ReadValue<Vector2>();
    Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
    Vector3 worldMove = transform.TransformDirection(move);

    float speed = sprinting ? sprintSpeed : walkSpeed;

    Vector3 delta = worldMove * speed * Time.fixedDeltaTime;

    controller.Move(delta);
  }

  public void HandleStamina()
  {
    if (sprinting && stamina > 0f)
    {
      stamina -= staminaDrainPerSecond * Time.deltaTime;
      regenTimer = regenDelay;

      if (stamina <= 0f)
      {
        stamina = 0f;
        sprinting = false;
      }
    }
    else
    {
      if (regenTimer > 0f)
      {
        regenTimer -= Time.deltaTime;
      }
      else
      {
        stamina += staminaRegenPerSecond * Time.deltaTime;
        stamina = Mathf.Min(stamina, maxStamina);
      }
    }
    GameUI.Instance.UpdateStaminaBar(stamina);
  }

  public void SetSprint(bool value)
  {
    if (value && stamina <= 0f)
      return;

    sprinting = value;
  }
}