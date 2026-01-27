using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{

  [Header("Actions")]
  public UnityEvent onCollect;   // Action when clicked

  private Collider col;

  void Awake()
  {
    col = GetComponent<Collider>();
  }

  void OnTriggerStay(Collider other)
  {
    Debug.Log(other);
    if (other.CompareTag("Player"))
    {
      onCollect.Invoke();
    }
  }

  public void SetActive(bool value)
  {
    col.enabled = value;
  }
}
