using UnityEngine;
using UnityEngine.InputSystem;

public class StartDoor : MonoBehaviour
{
  public GameObject playerPrefab;
  private float tileSize = 1f;   // distance in front of the door

  void Awake()
  {
    Vector3 spawnPosition =
      transform.position + transform.forward * tileSize;
    spawnPosition.y = 0f;
    Quaternion spawnRotation = Quaternion.LookRotation(
      transform.forward,
      Vector3.up
    );

    if (Player.Instance == null)
    {
      Instantiate(playerPrefab, spawnPosition, spawnRotation);
    }
    else
    {
      Transform player = Player.Instance.transform;
      player.position = spawnPosition;
      player.rotation = spawnRotation;
    }
  }

  void Start()
  {
    Player.Instance.gameObject.SetActive(true);
    GameUI.Instance.ShowBanner("I need to go home...");
    InputSystem.actions.Enable();
  }

  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      GameUI.Instance.ShowBanner("I just came from there... I need to go home.");
    }
  }
}