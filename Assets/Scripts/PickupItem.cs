using UnityEngine;

public class PickupItem : MonoBehaviour
{
  public string onPickupMessage;
  [Tooltip("Name of component to enable on the player")]
  public string toAdd;

  void OnTriggerStay(Collider other)
  {

    if (!other.CompareTag("Player")) return;

    if (toAdd == null || string.IsNullOrEmpty(toAdd))
    {
      Debug.LogError("No component assigned to PickupItem");
      return;
    }

    Player.Instance.AddToInventory(toAdd);

    GameUI.Instance?.ShowTutorialPopup(gameObject.name, onPickupMessage); // replace with popup window later
    Destroy(gameObject);
  }
}