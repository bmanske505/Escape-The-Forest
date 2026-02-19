using System;
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

  [Header("Input")]
  InputAction flashInput;

  private static float charge = 1f;

  private Light beam;

  public void Awake()
  {
    beam = GetComponent<Light>();
    Instance = this;
    flashInput = InputSystem.actions.FindAction("Flash");
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
    GameUI.Instance.UpdateFlashlightBar(charge);
  }

  void Update()
  {
    if (charge == 0f)
    {
      if (IsOn())
      {
        Off();
        GameUI.Instance.ShowBanner("\"Crap.\"");
      }
    }
    if (IsOn())
    {

      if (IsHittingStalkerEyes(out Stalker stalker))
      {
        GameUI.Instance.ShowBanner($"\"Aha! How do you like being stunned for {stunDuration} seconds? 😎\"", stunDuration);
        stalker.Stun(stunDuration);
      }
      charge = Mathf.Clamp01(charge - Time.deltaTime / lifespan);
      GameUI.Instance.UpdateFlashlightBar(charge);
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
    if (charge > 0f)
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
        Debug.Log("hit eye");
        stalker = hit.collider.GetComponentInParent<Stalker>();
        return true;
      }
    }

    return false;
  }

  public float GetCharge()
  {
    return charge;
  }

  public void Charge(float amount)
  {
    charge = Mathf.Clamp01(charge + amount);
  }

}