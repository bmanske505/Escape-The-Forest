using System.Collections;
using Scripts.FirebaseScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Newtonsoft.Json;
using System.Data.Common;



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

  protected override void Awake()
  {
    base.Awake();
    CacheSceneNames();

    // configure user settings firebase analytics logging
    if (!PlayerPrefs.HasKey("id"))
    {
      PlayerPrefs.SetString("id", Guid.NewGuid().ToString());
      PlayerPrefs.Save();
    }
    string id = PlayerPrefs.GetString("id");
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
    // log the current level's data
    CacheSensitivityUse();

    float flashlightRatio = Flashlight.Instance ? Flashlight.Instance.GetUseRatio() : 0f;

    FirebaseAnalytics.LogDocument("level_complete", JsonConvert.SerializeObject(new { level = GetLevel(), time_spent = levelTime, flashlight_pct_on = flashlightRatio, sensitivity_x_avg = weightedSensitivitySum[0] / levelTime, sensitivity_y_avg = weightedSensitivitySum[1] / levelTime }));
    levelTime = 0f; // reset the counter
    sensitivityTime = 0f;
    weightedSensitivitySum = Vector2.zero;

    if (isLoading) return;

    int nextIndex = GetLevel() + 1;
    if (nextIndex >= scenes.Length)
    {
      LoadScene("Win");
      return;
    }

    LoadLevel(nextIndex);
    PlayerPrefs.Save();
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
