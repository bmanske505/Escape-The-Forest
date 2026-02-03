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
        UIMaster.Instance.ShowBanner("You need to find your sibling first!");
      } else
      {
        LevelMaster.Instance.PlayNextLevel();
      }
    }
  }
}