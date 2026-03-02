using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{

  public float speed = 2f;

  CharacterController controller;

  private float regenTimer;

  [Header("Input")]

  private InputAction sprintAction;
  bool sprintInput;

  private InputAction moveAction;
  Vector2 moveInput;

  void Awake()
  {
    controller = GetComponent<CharacterController>();

    sprintAction = InputSystem.actions.FindAction("Sprint");
    moveAction = InputSystem.actions.FindAction("Move");
  }

  void OnEnable()
  {
    // We're just gonna disable sprint entirely lol
    //sprintAction.started += OnSprint;
    //sprintAction.canceled += OnSprint;
    moveAction.performed += OnMove;
    moveAction.canceled += OnMove;
  }

  void OnDisable()
  {
    // We're just gonna disable sprint entirely lol
    //sprintAction.started -= OnSprint;
    //sprintAction.canceled -= OnSprint;
    moveAction.performed -= OnMove;
    moveAction.canceled -= OnMove;
  }

  private void OnMove(CallbackContext context)
  {
    moveInput = context.ReadValue<Vector2>();
  }

  void Update()
  {
    // We're just gonna disable sprint entirely lol
    HandleMovement(); // not physics based
  }

  void HandleMovement()
  {
    // Horizontal input
    Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
    Vector3 worldMove = transform.TransformDirection(move).normalized;

    Vector3 velocity = worldMove * speed;

    // Ground check
    if (controller.isGrounded)
    {
      // Small downward force to keep us snapped to ground
      velocity.y = -1f;
    }
    else
    {
      // Apply gravity while airborne
      velocity.y = -9.8f;
    }

    // Move character
    controller.Move(velocity * Time.deltaTime);
  }
}