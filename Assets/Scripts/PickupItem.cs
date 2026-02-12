using UnityEditor;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
  public string onPickupMessage;
  [Tooltip("Script type to enable on the player")]
  public MonoScript componentScript;
  private Collider col;

  void Awake()
  {
    col = GetComponent<Collider>();
  }

  void OnTriggerStay(Collider other)
  {

    if (!other.CompareTag("Player")) return;

    if (componentScript == null)
    {
      Debug.LogError("No script assigned to PickupItem");
      return;
    }

    System.Type type = componentScript.GetClass();

    if (type == null || !typeof(Component).IsAssignableFrom(type))
    {
      Debug.LogError("Assigned script is not a Component");
      return;
    }

    Component comp = other.GetComponentInChildren(type, true);

    if (!comp)
    {
      Debug.LogError($"Player does not have component of type {type.Name}");
      return;
    }

    comp.gameObject.SetActive(true);
    GameUI.Instance.ShowBanner(onPickupMessage); // replace with popup window later
    Destroy(gameObject);
  }
}