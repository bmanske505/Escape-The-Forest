using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 25f;
    public float staminaRegenPerSecond = 15f;
    public float regenDelay = 1.5f;

    public float CurrentStamina { get; private set; }
    public bool IsSprinting => isSprinting;

    private Player player;
    private CharacterController controller;

    private Vector3 velocity;
    private float regenTimer;
    private bool isSprinting;

    void Awake()
    {
        player = GetComponent<Player>();
        controller = GetComponent<CharacterController>();
        CurrentStamina = maxStamina;
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleStamina();
    }

    void HandleMovement()
    {
        Vector2 input = GetMoveInput();
        Vector3 move = new Vector3(input.x, 0f, input.y);
        move = Vector3.ClampMagnitude(move, 1f);
        Vector3 worldMove = transform.TransformDirection(move);

        // Ground check
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = 0f;

        velocity.y += Physics.gravity.y * Time.fixedDeltaTime;

        float speed = isSprinting ? player.sprintSpeed : player.moveSpeed;

        Vector3 delta = worldMove * speed * Time.fixedDeltaTime;
        delta.y = velocity.y * Time.fixedDeltaTime;

        controller.Move(delta);
    }

    void HandleStamina()
    {
        if (isSprinting && CurrentStamina > 0f)
        {
            CurrentStamina -= staminaDrainPerSecond * Time.fixedDeltaTime;
            regenTimer = regenDelay;

            if (CurrentStamina <= 0f)
            {
                CurrentStamina = 0f;
                isSprinting = false;
            }
        }
        else
        {
            if (regenTimer > 0f)
            {
                regenTimer -= Time.fixedDeltaTime;
            }
            else
            {
                CurrentStamina += staminaRegenPerSecond * Time.fixedDeltaTime;
                CurrentStamina = Mathf.Min(CurrentStamina, maxStamina);
            }
        }
    }

    // Called by Player.cs (via sprint input)
    public void SetSprinting(bool value)
    {
        if (value && CurrentStamina <= 0f)
            return;

        isSprinting = value;
    }

    Vector2 GetMoveInput()
    {
        // Access private moveInput using reflection-safe method
        // Cleaner alternative: expose a getter in Player if you want
        return player.MoveInput;
    }
}