using UnityEngine;

public class HidingTrap : MonoBehaviour
{
  bool triggered = false;

  void OnTriggerEnter(Collider other)
  {
    if (!triggered && other.CompareTag("Player"))
    {
      triggered = true;
      Sibling.Instance.Hide();
    }
  }
}