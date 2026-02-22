using UnityEngine;

public class EndDoor : MonoBehaviour
{

  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      if (Sibling.Instance.IsHiding)
      {
        GameUI.Instance.ShowBanner("\"I need to find my little sibling first!\"");
      }
      else
      {
        LevelMaster.Instance.PlayNextLevel();
      }
    }
  }
}