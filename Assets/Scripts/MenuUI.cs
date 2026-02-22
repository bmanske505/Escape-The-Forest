using UnityEngine;

public class MenuUI : MonoBehaviour
{
  public void Toggle(GameObject obj)
  {
    obj.SetActive(!obj.activeSelf);
  }

  public void Restart()
  {
    LevelMaster.Instance.Restart();
  }

  public void Lobby()
  {
    LevelMaster.Instance.LoadScene("Lobby");
  }
}