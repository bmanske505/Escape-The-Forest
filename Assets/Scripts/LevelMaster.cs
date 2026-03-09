using System.Collections;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelMaster : Singleton<LevelMaster>
{
  [Header("Level Flow")]

#if UNITY_EDITOR
  [SerializeField] private SceneAsset[] levels;
#endif

  // Runtime-safe scene names
  [SerializeField, HideInInspector]
  private string[] scenes;

  private bool isLoading = false;
  private float levelTime = 0f; // for timing each level
  private Vector2 weightedSensitivitySum = Vector2.zero;
  private float sensitivityTime = 0f;

  // Actions
  private InputAction ABAudioAction;
  private InputAction ABVisualAction;

  protected override void Awake()
  {
    base.Awake();
    CacheSceneNames();

    // configure user settings firebase analytics logging
    if (!PlayerPrefs.HasKey("id"))
    {
      PlayerPrefs.SetString("id", System.Guid.NewGuid().ToString());
    }

    if (!PlayerPrefs.HasKey("ab_group"))
    {
      int group = Random.Range(0, 2);
      PlayerPrefs.SetInt("ab_group", group); // 0 = audio, 1 = visual
      Debug.Log($"Player assigned to group {group}");
    }
    PlayerPrefs.Save();

    ABAudioAction = InputSystem.actions.FindAction("ABAudio");
    ABVisualAction = InputSystem.actions.FindAction("ABVisual");
  }

  void OnEnable()
  {
    if (ABAudioAction != null)
    {
      ABAudioAction.performed += OnABAudio;
    }
    if (ABVisualAction != null)
    {
      ABVisualAction.performed += OnABVisual;
    }
  }

  void OnDisable()
  {
    if (ABAudioAction != null)
    {
      ABAudioAction.performed -= OnABAudio;
    }
    if (ABVisualAction != null)
    {
      ABVisualAction.performed -= OnABVisual;
    }
  }

  void OnABAudio(CallbackContext context)
  {
    PlayerPrefs.SetInt("ab_group", 0); // 0 represents audio A/B pipeline
    Debug.Log("Set to Audio A/B pipeline");
  }

  void OnABVisual(CallbackContext context)
  {
    PlayerPrefs.SetInt("ab_group", 1); // 1 represents visual A/B pipeline
    Debug.Log("Set to Visual A/B pipeline");

  }

  void Update()
  {
    if (Time.timeScale != 0f && Player.Instance) // player is in a game and the game is not paused
    {
      levelTime += Time.deltaTime;
      sensitivityTime += Time.deltaTime;
    }
  }

  /* =======================
   * Public API
   * ======================= */

  public void CacheSensitivityUse()
  {
    weightedSensitivitySum += new Vector2(PlayerPrefs.GetFloat("sensitivity_x"), PlayerPrefs.GetFloat("sensitivity_y")) * sensitivityTime;
    sensitivityTime = 0f;
  }

  public void NewGame()
  {
    PlayerPrefs.DeleteKey("inventory");
    PlayerPrefs.DeleteKey("flashlight_charge");
    PlayerPrefs.SetInt("playthrough", PlayerPrefs.GetInt("playthrough", 0) + 1);
    LoadLevel(0);
  }

  public void PlayCurrentLevel()
  {
    LoadLevel(GetLevel());
  }

  public void PlayNextLevel()
  {

    if (isLoading) return;

    // log the current level's data
    CacheSensitivityUse();

    float flashlightRatio = Flashlight.Instance ? Flashlight.Instance.GetUseRatio() : 0f;

    FirebaseAnalytics.LogDocument("level_complete", new { time_spent = levelTime, flashlight_pct_on = flashlightRatio }); //, sensitivity_x_avg = weightedSensitivitySum[0] / levelTime, sensitivity_y_avg = weightedSensitivitySum[1] / levelTime });
    levelTime = 0f; // reset the counter
    sensitivityTime = 0f;
    weightedSensitivitySum = Vector2.zero;

    int nextIndex = GetLevel() + 1;
    if (nextIndex >= scenes.Length)
    {
      LoadScene("Win");
      return;
    }

    LoadLevel(nextIndex);
  }

  public void LoadLevel(int index)
  {
    if (isLoading) return;

    if (index < 0 || index >= scenes.Length)
    {
      Debug.LogError($"GameMaster: Invalid level index {index}");
      return;
    }

    PlayerPrefs.SetInt("level_index", index);

    LoadScene(scenes[index]);
  }

  public void LoadScene(string name)
  {
    if (isLoading) return;
    StartCoroutine(LoadSceneAsync(name));
  }

  public float GetProgress()
  {
    return (float)GetLevel() / scenes.Length;
  }

  public int GetLevel()
  {
    return PlayerPrefs.GetInt("level_index", 0);
  }

  public int GetTotalLevels()
  {
    return scenes.Length;
  }

  /* =======================
   * Async Loading
   * ======================= */

  private IEnumerator LoadSceneAsync(string name)
  {
    isLoading = true;

    AsyncOperation loadOp = SceneManager.LoadSceneAsync(name);

    while (!loadOp.isDone)
      yield return null;

    isLoading = false;
  }

  /* =======================
   * SceneAsset → Name
   * ======================= */

  private void CacheSceneNames()
  {
#if UNITY_EDITOR
    if (levels == null || levels.Length == 0)
      return;

    scenes = new string[levels.Length];

    for (int i = 0; i < levels.Length; i++)
    {
      if (levels[i] == null)
      {
        scenes[i] = string.Empty;
        continue;
      }

      scenes[i] = levels[i].name;
    }
#endif
  }

#if UNITY_EDITOR
  // Keep names synced automatically in editor
  private void OnValidate()
  {
    CacheSceneNames();
  }
#endif
}
