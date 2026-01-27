using UnityEngine;

public class HidingTrap : MonoBehaviour
{
  void OnTriggerEnter(Collider other)
  {
    Debug.Log(other);
    if (other.CompareTag("Player"))
    {
      Sibling sib = FindFirstObjectByType<Sibling>();
      Debug.Log(sib.gameObject);
      sib.Hide();
    }
  }
}