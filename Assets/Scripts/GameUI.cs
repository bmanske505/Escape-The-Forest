using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
  [Header("Banner UI")]
  [SerializeField] private CanvasGroup bannerGroup;
  [SerializeField] private TMP_Text bannerText;
  [SerializeField] private float fadeDuration = 0.25f;

  [Header("Stat Bars")]
  [SerializeField] private Slider flashlightBar;
  [SerializeField] private Slider staminaBar;
  [SerializeField] private TextMeshProUGUI levelText;

  [Header("Popup UI")]
  [SerializeField] private CanvasGroup tutorialPopupGroup;
  [SerializeField] private TMP_Text tutorialPopupTitle;
  [SerializeField] private TMP_Text tutorialPopupMessage;
  [SerializeField] private CanvasGroup diedPopupGroup;

  private Coroutine activeRoutine;
  public static GameUI Instance;

  void Awake()
  {
    Instance = this;

    // Ensure banner starts hidden
    if (bannerGroup != null)
    {
      bannerGroup.alpha = 0f;
      bannerGroup.gameObject.SetActive(false);
    }
  }

  void Start()
  {
    UpdateLevelBar();
  }

  public void UpdateLevelBar()
  {
    levelText.text = "Progress: " + LevelMaster.Instance.GetProgress() * 100 + "%";
  }

  public void UpdateStaminaBar(float value)
  {
    if (value < 0)
    {
      staminaBar.gameObject.SetActive(false);
    }
    else
    {
      staminaBar.gameObject.SetActive(true);
      staminaBar.value = value;
    }
  }

  public void UpdateFlashlightBar(float value)
  {
    if (value < 0)
    {
      flashlightBar.gameObject.SetActive(false);
    }
    else
    {
      flashlightBar.gameObject.SetActive(true);
      flashlightBar.value = value;
    }
  }

  /// <summary>
  /// Shows a banner message temporarily.
  /// </summary>
  public void ShowBanner(string message, float duration = 2f)
  {
    if (activeRoutine != null)
      StopCoroutine(activeRoutine);

    activeRoutine = StartCoroutine(BannerRoutine(message, duration));
  }

  public void ShowTutorialPopup(string title, string message)
  {
    if (activeRoutine != null)
      StopCoroutine(activeRoutine);

    Time.timeScale = 0f;
    tutorialPopupTitle.text = title;
    tutorialPopupMessage.text = message;
    tutorialPopupGroup.gameObject.SetActive(true);

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void ShowDeathPopup()
  {
    Time.timeScale = 0f;
    diedPopupGroup.gameObject.SetActive(true);

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void HideTutorialPopup()
  {
    tutorialPopupGroup.gameObject.SetActive(false);

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    Time.timeScale = 1f;
  }


  private IEnumerator BannerRoutine(string message, float duration)
  {
    bannerText.text = message;
    bannerGroup.gameObject.SetActive(true);

    // Fade in
    yield return Fade(0f, 1f);

    // Stay visible
    yield return new WaitForSeconds(duration);

    // Fade out
    yield return Fade(1f, 0f);

    bannerGroup.gameObject.SetActive(false);
    activeRoutine = null;
  }

  private IEnumerator Fade(float from, float to)
  {
    float elapsed = 0f;

    while (elapsed < fadeDuration)
    {
      elapsed += Time.deltaTime;
      bannerGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
      yield return null;
    }

    bannerGroup.alpha = to;
  }

  public void ReplayLevel()
  {
    LevelMaster.Instance.ReplayLevel();
  }
}
