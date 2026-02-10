using UnityEngine;

public class HidingTrap : MonoBehaviour
{
  bool triggered = false;

  void OnTriggerEnter(Collider other)
  {
    if (!triggered && other.CompareTag("Player") && !Sibling.Instance.IsHiding)
    {
      triggered = true;
      Sibling.Instance.Hide();
    }
  }
}