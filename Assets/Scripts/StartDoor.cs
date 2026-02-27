using UnityEngine;

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

    Instantiate(playerPrefab, spawnPosition, spawnRotation);
  }

  void Start()
  {
    GameUI.Instance?.ShowBanner("I need to go home...");
  }

  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      GameUI.Instance?.ShowBanner("I just came from there... I need to go home.");
    }
  }
}