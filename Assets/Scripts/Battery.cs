public class Battery : Collectible
{
  public float charge = 0.3f;
  public void OnCollect()
  {
    Flashlight flashlight = FindFirstObjectByType<Flashlight>();
    if (flashlight.GetCharge() < 1f) { // consume the battery
      UIMaster.Instance.ShowBanner("\"I found a battery! Sweet!\"");
      flashlight.Charge(charge);
      Destroy(gameObject);
    } else
    {
      UIMaster.Instance.ShowBanner("\"Cool, a battery! I don't think I need it just yet, though.");
    }
  }
}