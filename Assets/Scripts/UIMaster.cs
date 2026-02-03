using System.Collections;
using UnityEngine;
using TMPro;

public class UIMaster : KeepOldSingleton<UIMaster>
{
  [Header("Banner UI")]
  [SerializeField] private CanvasGroup bannerGroup;
  [SerializeField] private TMP_Text bannerText;

  [Header("Timing")]
  [SerializeField] private float fadeDuration = 0.25f;

  private Coroutine bannerRoutine;

  protected override void Awake()
  {
    base.Awake();

    // Ensure banner starts hidden
    if (bannerGroup != null)
    {
      bannerGroup.alpha = 0f;
      bannerGroup.gameObject.SetActive(false);
    }
  }

  /* =======================
   * Public API
   * ======================= */

  /// <summary>
  /// Shows a banner message temporarily.
  /// </summary>
  public void ShowBanner(string message, float duration = 2f)
  {
    if (bannerRoutine != null)
      StopCoroutine(bannerRoutine);

    bannerRoutine = StartCoroutine(BannerRoutine(message, duration));
  }

  /* =======================
   * Coroutine Logic
   * ======================= */

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
