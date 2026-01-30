using UnityEngine;

public class Flashlight : MonoBehaviour
{
  [Header("Light Settings")]
  public float range = 15f;
  public float spotAngle = 45f;
  public float intensity = 2f;

  private Light lightSource;
  private bool isOn;

  public void AttachTo(Transform parent)
  {
    GameObject lightObj = new GameObject("Flashlight");
    lightObj.transform.SetParent(parent);
    lightObj.transform.localPosition = Vector3.zero;
    lightObj.transform.localRotation = Quaternion.identity;

    lightSource = lightObj.AddComponent<Light>();
    lightSource.type = LightType.Spot;
    lightSource.range = range;
    lightSource.spotAngle = spotAngle;
    lightSource.intensity = intensity;
    lightSource.enabled = false;
  }

  public bool IsOn()
  {
    return isOn;
  }

  public void Toggle()
  {
    isOn = !isOn;
    if (IsOn())
    {
      On();
    }
    else
    {
      Off();
    }
  }

  private void On()
  {
    Debug.Log("Flashlight On!");
    if (lightSource != null)
      lightSource.enabled = true;
  }

  private void Off()
  {
    Debug.Log("Flashlight Off!");
    if (lightSource != null)
      lightSource.enabled = false;
  }
}