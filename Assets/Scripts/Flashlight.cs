using Newtonsoft.Json;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Flashlight : MonoBehaviour
{

  [Header("Charge Settings")] // NOTE: charge is [0,1]
  public float expendRate = 0.05f;          // charge lost per second
  public float regenDelay = 2f;         // seconds before regen starts
  public float regenRate = 0.05f;       // charge regenerated per second
  private float regenTimer = 0f;
  private bool isRecharging = false;

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
    GameUI.Instance?.UpdateFlashlightBar(GetCharge());
  }

  void Update()
  {
    if (Time.timeScale == 0f || !Player.Instance) return;

    totalTime += Time.deltaTime;

    if (IsOn())
    {
      HandleDrain();
    }
    else
    {
      HandleRechargeState();
    }

    GameUI.Instance?.UpdateFlashlightBar(GetCharge());
  }

  private void HandleDrain()
  {
    regenTimer = 0f;
    isRecharging = false;

    flashlightTime += Time.deltaTime;
    SetCharge(GetCharge() - Time.deltaTime * expendRate);

    if (GetCharge() <= 0f)
    {
      Off();
      GameUI.Instance?.ShowBanner("\"Crap.\"");
      FirebaseAnalytics.LogDocument("flashlight_died", new { });
    }

    if (IsHittingStalkerEyes(out Stalker stalker) &&
        stalker.CurrentState != NavMeshEnemy.State.Stunned)
    {
      GameUI.Instance?.ShowBanner(
        $"\"Aha! How do you like being stunned for {stunDuration} seconds? 😎\"",
        stunDuration
      );

      stalker.Stun(stunDuration);
    }
  }

  private void HandleRechargeState()
  {
    if (GetCharge() >= 1f)
    {
      isRecharging = false;
      return;
    }

    regenTimer += Time.deltaTime;

    if (!isRecharging)
    {
      // update the UI bar
      // Waiting to enter recharge
      if (regenTimer >= regenDelay)
      {
        isRecharging = true;
        GameUI.Instance.SetFlashlightBarOpacity(0.1f);
        GameUI.Instance?.ShowBanner("\"Hold on... recharging.\"");
      }
    }
    else
    {
      // Actively recharging (locked out)
      SetCharge(GetCharge() + regenRate * Time.deltaTime);

      if (GetCharge() >= 1f)
      {
        SetCharge(1f);
        isRecharging = false;
        GameUI.Instance.SetFlashlightBarOpacity(1f);
        GameUI.Instance?.ShowBanner("\"Okay. Back in business.\"");
      }
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
    if (isRecharging)
    {
      GameUI.Instance?.ShowBanner("\"It’s recharging — can’t use it yet.\"");
      return;
    }

    if (IsOn())
      Off();
    else
      On();
  }

  private void On()
  {
    if (GetCharge() > 0f && !isRecharging)
    {
      regenTimer = 0f;
      beam.enabled = true;
    }
    else
    {
      GameUI.Instance?.ShowBanner(
        "\"Dead... I wonder if there's any batteries lying around.\""
      );
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