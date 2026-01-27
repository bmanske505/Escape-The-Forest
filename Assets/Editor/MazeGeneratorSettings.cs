using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[FilePath("Settings/MazeGeneratorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
[CreateAssetMenu]
public class MazeGeneratorSettings : ScriptableSingleton<MazeGeneratorSettings>
{
  [System.Serializable]
  public class CharacterPrefabPair
  {
    public char character; // single char
    public GameObject prefab;
  }

  [Header("Template Scene")]
  public SceneAsset templateScene;

  [Header("Character Map")]
  public List<CharacterPrefabPair> characterMap = new();

  public GameObject GetPrefab(char c)
  {
    foreach (var entry in characterMap)
      if (entry.character == c) return entry.prefab;
    return null;
  }

  public void SaveSettings() => Save(true);
}