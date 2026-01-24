using UnityEngine;

public abstract class KeepNewSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
  public static T Instance { get; private set; }

  protected virtual void OnEnable()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(Instance.gameObject);
    }
    Instance = this as T;
  }
  protected virtual void OnDisable()
  {
    Instance = null;
  }
}