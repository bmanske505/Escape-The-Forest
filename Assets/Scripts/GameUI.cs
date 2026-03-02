using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameUI : MonoBehaviour
{
  [Header("Banner UI")]
  [SerializeField] private CanvasGroup bannerGroup;
  [SerializeField] private TMP_Text bannerText;
  [SerializeField] private float fadeDuration = 0.25f;

  [Header("Stat Bars")]
  [SerializeField] private Slider flashlightBar;
  [SerializeField] private TextMeshProUGUI levelText;

  [Header("Popup UI")]
  [SerializeField] private CanvasGroup tutorialPopupGroup;
  [SerializeField] private TMP_Text tutorialPopupTitle;
  [SerializeField] private TMP_Text tutorialPopupMessage;
  [SerializeField] private CanvasGroup diedPopupGroup;
  [SerializeField] private TMP_Text diedTipText;

  private Coroutine bannerRoutine;
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

  void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
  }

  void Start()
  {
    UpdateLevelBar();
  }

  // used by the death popup
  public void Retry()
  {
    LevelMaster.Instance.PlayCurrentLevel();
  }

  public void UpdateLevelBar()
  {
    // recall we're 0-indexed
    levelText.text = string.Format("Progress: {0} / {1}", LevelMaster.Instance.GetLevel() + 1, LevelMaster.Instance.GetTotalLevels());
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

  public void SetFlashlightBarOpacity(float alpha)
  {
    flashlightBar.GetComponent<CanvasGroup>().alpha = alpha;
  }

  /// <summary>
  /// Shows a banner message temporarily.
  /// </summary>
  public void ShowBanner(string message, float duration = 2f)
  {
    if (bannerRoutine != null)
      StopCoroutine(bannerRoutine);

    bannerRoutine = StartCoroutine(BannerRoutine(message, duration));
  }

  public void ShowTutorialPopup(string title, string message)
  {

    InputSystem.actions.FindActionMap("Player", true).Disable();

    Time.timeScale = 0f;
    tutorialPopupTitle.text = title;
    tutorialPopupMessage.text = message;
    tutorialPopupGroup.gameObject.SetActive(true);

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void ShowDeathPopup(string tip)
  {
    Time.timeScale = 0f;
    diedTipText.text = $"TIP: {tip}";
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
    InputSystem.actions.FindActionMap("Player", true).Enable();
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
    bannerRoutine = null;
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
}
