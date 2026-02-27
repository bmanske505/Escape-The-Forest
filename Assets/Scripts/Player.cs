using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
  public static Player Instance;

  [Header("Look")]
  public Transform cameraPivot;
  public float minPitch = -80f;
  public float maxPitch = 80f;
  float lookScaler = Application.platform == RuntimePlatform.WebGLPlayer ? 0.1f : 1.2f;

  [Header("Sibling")]
  public GameObject siblingPrefab;
  public Vector3 siblingSpawnOffset = new Vector3(0f, 0f, 0f);

  private float pitch;
  private static string localInventory;

  private InputAction lookAction;
  private Vector2 lookInput;

  void Awake()
  {
    Instance = this;
    lookAction = InputSystem.actions.FindAction("Look");
    localInventory = PlayerPrefs.GetString("inventory", "");
    PopulateInventory();
  }

  void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
  }

  void OnEnable()
  {
    Time.timeScale = 1f;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    // Spawn sibling prefab as a sibling (same parent)

    Vector3 spawnPos = transform.position + siblingSpawnOffset;

    Instantiate(
      siblingPrefab,
      spawnPos,
      Quaternion.identity,
      transform.parent   // 👈 THIS is what makes it a sibling
    );

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
    pitch -= lookInput.y * PlayerPrefs.GetFloat("sensitivity_y", 100f) * lookScaler * Time.deltaTime;
    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

    float yaw = lookInput.x * PlayerPrefs.GetFloat("sensitivity_x", 100f) * lookScaler * Time.deltaTime;
    transform.Rotate(0f, yaw, 0f, Space.World);
  }

  void PopulateInventory()
  {
    string[] inventory = localInventory.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

    foreach (string item in inventory)
    {

      // Check if component already exists
      EnableComponent(item);
    }
  }

  public void AddToInventory(string name)
  {
    if (localInventory != "") // we have at least one item, add a comma
    {
      localInventory += ",";
    }
    localInventory += name;
    EnableComponent(name);
  }

  public string GetInventory()
  {
    return localInventory;
  }

  private void EnableComponent(string name)
  {

    // Get the Type from the string
    System.Type type = System.Type.GetType(name);
    if (type == null)
    {
      Debug.LogError($"Type '{name}' not found!");
      return;
    }

    if (!typeof(Component).IsAssignableFrom(type))
    {
      Debug.LogError($"Type '{name}' is not a Component!");
      return;
    }

    // Add the component if player doesn't already have it
    Component comp = GetComponentInChildren(type, true);
    if (comp == null)
    {
      gameObject.AddComponent(type);
    }
    else if (comp is MonoBehaviour mb)
    {
      mb.enabled = true;
      comp.gameObject.SetActive(true);
    }
  }
}
