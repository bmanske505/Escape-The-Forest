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
      } else
      {
        //Player.Instance.gameObject.SetActive(false);
        //Player.Instance.GetComponent<PlayerInput>().actions["Sprint"].Disable();
        Player.Instance.GetComponent<PlayerMovement>().SetSprinting(false);
        Player.Instance.GetComponent<PlayerMovement>().SetSpeed();
        LevelMaster.Instance.PlayNextLevel();
        //Player.Instance.GetComponent<PlayerInput>().actions["Sprint"].Enable();
        Player.Instance.GetComponent<PlayerMovement>().SetSprinting(false);
        Player.Instance.GetComponent<PlayerMovement>().SetSpeed();
      }
    }
  }
}