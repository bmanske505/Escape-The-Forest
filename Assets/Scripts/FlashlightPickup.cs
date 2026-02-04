using Unity.VisualScripting;

public class FlashlightPickup : Collectible
{
  public void OnCollect()
  {
    Player.Instance.AddComponent<Flashlight>();
    // eventually replace with a popup window instead of banner
    GameUI.Instance.ShowBanner("Left mouse to use the flashlight.");
  }
}