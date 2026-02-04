using UnityEngine;

public class Door : MonoBehaviour
{
  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      Sibling sib = FindFirstObjectByType<Sibling>();
      if (sib.IsHiding)
      {
        UIMaster.Instance.ShowBanner("\"I need to find my little sibling first!\"");
      } else
      {
        LevelMaster.Instance.PlayNextLevel();
      }
    }
  }
}