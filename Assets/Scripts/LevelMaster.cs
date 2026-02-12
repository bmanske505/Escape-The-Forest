using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;



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

  private int index = 0;

  private bool isLoading = false;

  protected override void Awake()
  {
    base.Awake();
    CacheSceneNames();
  }

  /* =======================
   * Public API
   * ======================= */

  public void ReplayLevel()
  {
    if (isLoading) return;
    StartCoroutine(LoadSceneAsync(index));
  }

  public void PlayNextLevel()
  {
    if (isLoading) return;

    int nextIndex = index + 1;
    if (nextIndex >= scenes.Length)
    {
      Debug.LogWarning("GameMaster: No more scenes to load.");
      return;
    }

    index = nextIndex;
    StartCoroutine(LoadSceneAsync(index));
  }

  public void LoadLevel(int newIndex)
  {
    if (isLoading) return;

    if (newIndex < 0 || newIndex >= scenes.Length)
    {
      Debug.LogError($"GameMaster: Invalid level index {newIndex}");
      return;
    }

    index = newIndex;
    StartCoroutine(LoadSceneAsync(index));
  }

  public float GetProgress()
  {
    return (float)index / levels.Length;
  }

  public int GetLevelIndex()
  {
    return index;
  }

  /* =======================
   * Async Loading
   * ======================= */

  private IEnumerator LoadSceneAsync(int index)
  {
    isLoading = true;

    AsyncOperation loadOp = SceneManager.LoadSceneAsync(scenes[index]);

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
