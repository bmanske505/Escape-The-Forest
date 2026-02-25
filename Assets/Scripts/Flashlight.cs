using Newtonsoft.Json;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Flashlight : MonoBehaviour
{

  [Header("Charge Settings")]
  public float lifespan = 10f; // in seconds

  [Header("Stun Settings")]
  public float stunRange = 6f;
  public float stunDuration = 5f;
  public LayerMask detectionMask; // Stalker + environment
  public static Flashlight Instance;
  private static float localCharge;

  // use for flashlight analytics
  private float flashlightTime = 0f;
  private float totalTime = 0f;


  [Header("Input")]
  InputAction flashInput;

  private Light beam;

  public void Awake()
  {
    Instance = this;
    beam = GetComponent<Light>();
    flashInput = InputSystem.actions.FindAction("Flash");
    SetCharge(PlayerPrefs.GetFloat("flashlight_charge", 1f));
  }

  void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
  }

  void OnEnable()
  {
    flashInput.performed += OnFlash;
  }

  void OnDisable()
  {
    flashInput.performed -= OnFlash;
  }

  void Start()
  {
    beam.enabled = false;
    GameUI.Instance.UpdateFlashlightBar(GetCharge());
  }

  void Update()
  {
    print($"Saved charge: {PlayerPrefs.GetFloat("flashlight_charge")}, local charge: {localCharge}");
    if (Time.timeScale == 0f || !Player.Instance) return; // player not in game or paused

    totalTime += Time.deltaTime;

    if (GetCharge() == 0f)
    {
      if (IsOn())
      {
        Off();
        GameUI.Instance.ShowBanner("\"Crap.\"");

        FirebaseAnalytics.LogDocument("flashlight_died", new { level = LevelMaster.Instance.GetLevel() });
      }
    }
    if (IsOn())
    {
      if (Time.timeScale != 0f && Player.Instance) // player is in a game and the game is not paused
      {
        flashlightTime += Time.deltaTime;
      }

      if (IsHittingStalkerEyes(out Stalker stalker))
      {
        GameUI.Instance.ShowBanner($"\"Aha! How do you like being stunned for {stunDuration} seconds? 😎\"", stunDuration);
        stalker.Stun(stunDuration);
      }
      SetCharge(Mathf.Clamp01(GetCharge() - Time.deltaTime / lifespan));
      GameUI.Instance.UpdateFlashlightBar(GetCharge());
    }
  }

  private void OnFlash(CallbackContext context)
  {
    Toggle();
  }

  public bool IsOn()
  {
    return beam.enabled;
  }

  public void Toggle()
  {
    if (IsOn())
    {
      Off();
    }
    else
    {
      On();
    }
  }

  private void On()
  {
    if (GetCharge() > 0f)
    {
      beam.enabled = true;
    }
    else
    {
      GameUI.Instance.ShowBanner("\"Dead... I wonder if there's any batteries lying around.\"");
    }
  }

  private void Off()
  {
    beam.enabled = false;
  }

  public bool IsHittingStalkerEyes(out Stalker stalker)
  {
    stalker = null;

    if (!IsOn())
      return false;

    Ray ray = new Ray(transform.position, transform.forward);

    if (Physics.Raycast(ray, out RaycastHit hit, stunRange, detectionMask))
    {
      // Check if we hit an eye
      if (hit.collider.CompareTag("StalkerEye"))
      {
        stalker = hit.collider.GetComponentInParent<Stalker>();
        return true;
      }
    }

    return false;
  }

  public float GetCharge()
  {
    return localCharge;
  }

  public void Charge(float amount)
  {
    SetCharge(GetCharge() + amount);
  }

  public void SetCharge(float newCharge)
  {
    localCharge = Mathf.Clamp01(newCharge);
  }

  public float GetUseRatio()
  {
    return flashlightTime / totalTime;
  }

}