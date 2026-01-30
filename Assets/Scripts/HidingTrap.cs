using UnityEngine;

public class HidingTrap : MonoBehaviour
{
  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      Sibling sib = FindFirstObjectByType<Sibling>();
      sib.Hide();
    }
  }
}