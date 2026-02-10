using Unity.VisualScripting;
using UnityEngine;

public class PickupItem<T> : MonoBehaviour where T : Component
{
  public string onPickupMessage;
  private Collider col;

  void Awake()
  {
    col = GetComponent<Collider>();
  }

  void OnTriggerStay(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      GameObject item = other.gameObject.GetComponentInChildren<T>().gameObject;
      if (!item) { Debug.LogError("This item isn't on the player..."); }
      other.gameObject.GetComponentInChildren<T>().gameObject.SetActive(true);
      GameUI.Instance.ShowBanner(onPickupMessage); // replace with popup window later
    }
  }
}