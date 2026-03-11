using UnityEngine;

public class VisualOnlyFeature : MonoBehaviour
{
  void Start()
  {
    gameObject.SetActive(PlayerPrefs.GetInt("ab_group") == 1);
  }
}