using UnityEngine;
using UnityEngine.InputSystem;

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
        Player.Instance.GetComponentInChildren<PlayerMovement>().SetSprinting(false);
        InputSystem.actions.Disable();
        Player.Instance.gameObject.SetActive(false);
        LevelMaster.Instance.PlayNextLevel();
      }
    }
  }
}