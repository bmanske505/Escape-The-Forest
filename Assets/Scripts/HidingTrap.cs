using Unity.VisualScripting;
using UnityEngine;

public class HidingTrap : MonoBehaviour
{
  [SerializeField] Transform hidingSpot;
  bool triggered = false;

  void OnTriggerEnter(Collider other)
  {
    if (!triggered && other.CompareTag("Player") && Sibling.Instance.state != Sibling.State.Hiding)
    {
      triggered = true;
      Sibling.Instance.Hide("trigger", hidingSpot.position);
    }
  }
}