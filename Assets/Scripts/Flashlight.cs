using System;
using UnityEngine;

public class Flashlight : MonoBehaviour
{

  [Header("Charge Settings")]
  public float lifespan = 10f; // in seconds

  [Header("Stun Settings")]
  public float stunRange = 5f;
  public float stunDuration = 5f;
  public LayerMask detectionMask; // Stalker + environment

  private float charge = 1f;

  private Light beam;

  public void Awake()
  {
    beam = GetComponent<Light>();
  }

  public void Start()
  {
    beam.enabled = false;
  }

  public void Update()
  {
    if (charge == 0f)
    {
      if (IsOn())
      {
        Off();
        UIMaster.Instance.ShowBanner("\"Crap.\"");
      }
    }
    if (IsOn())
    {

      if (IsHittingStalkerEyes(out Stalker stalker))
      {
        UIMaster.Instance.ShowBanner($"\"Aha! How do you like being stunned for {stunDuration} seconds? 😎\"", stunDuration);
        stalker.Stun(stunDuration);
      }
      charge = Mathf.Clamp01(charge - Time.deltaTime / lifespan);
    }
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
      UIMaster.Instance.ShowBanner("\"Dead... I wonder if there's any batteries lying around.\"");
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
    return charge;
  }

  public void Charge(float amount)
  {
    charge = Mathf.Clamp01(charge + amount);
  }

}