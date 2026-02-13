using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Singleton<Player>
{

  [Header("Look")]
  public Transform cameraPivot;
  public float mouseSensitivity = 120f;
  public float minPitch = -80f;
  public float maxPitch = 80f;

  [Header("Sibling")]
  public GameObject siblingPrefab;
  public Vector3 siblingSpawnOffset = new Vector3(0f, 0f, 0f);

  private float pitch;

  private InputAction lookAction;
  private Vector2 lookInput;

  protected override void Awake()
  {
    base.Awake();
    lookAction = InputSystem.actions.FindAction("Look");
  }

  void OnEnable()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    // Spawn sibling prefab as a sibling (same parent)
    if (siblingPrefab != null)
    {
      Vector3 spawnPos = transform.position + siblingSpawnOffset;

      if (Sibling.Instance)
      {
        Sibling.Instance.transform.position = spawnPos;
      }
      else
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

    lookAction.performed += OnLook;
    lookAction.canceled += OnLook;
  }

  void OnDisable()
  {
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    lookAction.performed -= OnLook;
    lookAction.canceled -= OnLook;
  }

  private void OnLook(InputAction.CallbackContext context)
  {
    lookInput = context.ReadValue<Vector2>();
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
