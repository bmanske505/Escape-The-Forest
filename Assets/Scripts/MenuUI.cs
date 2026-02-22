using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
  public void Start()
  {
    GameObject.Find("ContinueButton").GetComponent<Button>().interactable = LevelMaster.Instance.GetLevel() != 0;
  }

  public void Toggle(GameObject obj)
  {
    obj.SetActive(!obj.activeSelf);
  }

  public void NewGame()
  {
    LevelMaster.Instance.NewGame();
  }

  public void Continue()
  {
    LevelMaster.Instance.PlayCurrentLevel();
  }

  public void Lobby()
  {
    LevelMaster.Instance.LoadScene("Lobby");
  }
}