using UnityEngine;

public class HidingTrap : MonoBehaviour
{
  [SerializeField] Transform hidingSpot;
  bool triggered = false;

  void OnTriggerEnter(Collider other)
  {
    Debug.Log($"I've been triggered. {other}, {Sibling.Instance.IsHiding}");
    if (!triggered && other.CompareTag("Player") && !Sibling.Instance.IsHiding)
    {
      triggered = true;
      Sibling.Instance.Hide("trigger", hidingSpot.position);
    }
  }
}