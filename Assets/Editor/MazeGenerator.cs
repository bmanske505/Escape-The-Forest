using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MazeGenerator : EditorWindow
{
  // ================= LEVELS (HARD-CODED) =================
  [System.Serializable]
  public class Level
  {
    public string name;        // scene name
    public string maze;        // ASCII maze
    public float light;        // unused for now
  }

  // Hard-coded levels
  static readonly Level[] LEVELS =
{
        new Level
        {
            name = "Level 1",
            light = 1.0f,
            maze =
@"
#########
#_____#_#
#_##_##_#
#__#P___#
######_##
#_______#
#D#######
"
        },

        new Level
        {
            name = "Level 2",
            light = 0.75f,
            maze =
@"
#############
#P#_____#___#
#_#_###_###_#
#___#___#B__#
#_#####.###_#
#___###_____#
#######_#####
#___________D
#############
"
        }
    };

  // ================= WINDOW =================
  Vector2 scroll;

  [MenuItem("Tools/MazeGenerator")]
  static void Open() => GetWindow<MazeGenerator>("Maze Generator");

  void OnGUI()
  {
    var settings = MazeGeneratorSettings.instance;
    SerializedObject so = new SerializedObject(settings);

    scroll = EditorGUILayout.BeginScrollView(scroll);

    EditorGUI.BeginChangeCheck();

    // Template Scene
    EditorGUILayout.LabelField("Template Scene", EditorStyles.boldLabel);
    EditorGUILayout.PropertyField(so.FindProperty("templateScene"));

    EditorGUILayout.Space(10);

    // Character → Prefab Map
    EditorGUILayout.LabelField("Character → Prefab Mapping", EditorStyles.boldLabel);
    EditorGUILayout.PropertyField(so.FindProperty("characterMap"), true);

    EditorGUILayout.Space(10);

    if (EditorGUI.EndChangeCheck())
    {
      so.ApplyModifiedProperties();
      settings.SaveSettings();
    }

    EditorGUILayout.Space(10);

    // Hard-coded levels info
    EditorGUILayout.LabelField("Levels (hard-coded, edit in MazeGenerator.cs):", EditorStyles.boldLabel);

    EditorGUILayout.Space(20);

    if (GUILayout.Button("Generate Maze Scenes"))
    {
      GenerateScenes(settings);
    }

    EditorGUILayout.EndScrollView();
  }

  // ================= GENERATION =================
  const string LEVEL_FOLDER = "Assets/Levels";

  static bool ValidateCharacterMap(MazeGeneratorSettings settings)
  {
    var set = new HashSet<char>();
    foreach (var entry in settings.characterMap)
    {
      if (!set.Add(entry.character))
      {
        EditorUtility.DisplayDialog(
            "Duplicate Character",
            $"Character '{entry.character}' is mapped more than once.",
            "OK");
        return false;
      }
    }
    return true;
  }

  static void GenerateScenes(MazeGeneratorSettings settings)
  {
    if (!ValidateCharacterMap(settings))
      return;

    if (settings.templateScene == null)
    {
      EditorUtility.DisplayDialog("Error", "Template scene not assigned.", "OK");
      return;
    }

    string templatePath = AssetDatabase.GetAssetPath(settings.templateScene);
    if (!File.Exists(templatePath))
    {
      EditorUtility.DisplayDialog("Error", "Template scene not found.", "OK");
      return;
    }

    Directory.CreateDirectory(LEVEL_FOLDER);

    foreach (var level in LEVELS)
    {
      if (string.IsNullOrWhiteSpace(level.name))
        continue;

      string scenePath = $"{LEVEL_FOLDER}/{level.name}.unity";

      /*
      Currently just overwrite the scene regardless (who cares lol)
      if (File.Exists(scenePath))
      {
        if (!EditorUtility.DisplayDialog(
            "Overwrite Scene?",
            $"Scene '{level.name}' already exists. Overwrite?",
            "Overwrite",
            "Cancel"))
        {
          continue;
        }
      }
      */

      BuildScene(level, scenePath, settings, templatePath);
    }

    AssetDatabase.Refresh();
    EditorUtility.DisplayDialog("Done", "Maze scenes generated!", "OK");
  }

  static void BuildScene(Level level, string scenePath, MazeGeneratorSettings settings, string templatePath)
  {
    var scene = EditorSceneManager.OpenScene(templatePath, OpenSceneMode.Single);

    var root = new GameObject("Maze");

    // Map: prefab name -> parent GameObject (e.g. "Wall" -> Walls)
    var parentMap = new Dictionary<string, GameObject>();

    var lines = level.maze
        .Replace("\r\n", "\n")
        .Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

    for (int z = 0; z < lines.Length; z++)
    {
      for (int x = 0; x < lines[z].Length; x++)
      {
        char c = lines[z][x];
        var prefab = settings.GetPrefab(c);
        if (!prefab) continue;

        // Decide parent name (pluralized prefab name)
        string parentName = prefab.name + "s";

        // Get or create parent
        if (!parentMap.TryGetValue(parentName, out var parent))
        {
          parent = new GameObject(parentName);
          parent.transform.SetParent(root.transform);
          parentMap[parentName] = parent;
        }

        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.transform.position = new Vector3(x, obj.transform.position.y, -z);
        obj.transform.SetParent(parent.transform);
      }
    }

    // Future: use level.light to adjust lighting

    EditorSceneManager.SaveScene(scene, scenePath);
  }

}