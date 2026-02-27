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
        // bank info from this level
        if (Flashlight.Instance)
        {
          PlayerPrefs.SetFloat("flashlight_charge", Flashlight.Instance.GetCharge());
        }
        PlayerPrefs.SetString("inventory", Player.Instance.GetInventory());
        LevelMaster.Instance.PlayNextLevel();

        PlayerPrefs.Save();
      }
    }
  }
}