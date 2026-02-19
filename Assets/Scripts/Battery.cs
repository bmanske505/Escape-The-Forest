public class Battery : Collectible
{
  public float charge = 0.3f;
  public void OnCollect()
  {
    if (Flashlight.Instance.GetCharge() < 1f)
    { // consume the battery
      GameUI.Instance.ShowBanner("\"I found a battery! Sweet!\"");
      Flashlight.Instance.Charge(charge);
      GameUI.Instance.UpdateFlashlightBar(Flashlight.Instance.GetCharge());
      Destroy(gameObject);
    }
    else
    {
      GameUI.Instance.ShowBanner("\"Cool, a battery! I don't think I need it just yet, though.");
    }
  }
}