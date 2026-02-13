using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;

public class MazeGenerator : EditorWindow
{
  // ================= LEVELS (HARD-CODED) =================
  [System.Serializable]
  public class Level
  {
    public string name;        // scene name
    public string maze;        // ASCII maze
    public Color lightColor = Color.white;
    public float light = 1f; // [0,1] normalized
  }

  // Hard-coded levels
  static readonly Level[] LEVELS =
{
        new Level
        {
            name = "Level 1",
            light = 1f,
            lightColor = Color.white,
            maze =
@"
#########
#_____#_#
#_##_##_#
#__|*___#
######_##
#*______#
#E#######
"
        },

        new Level
        {
            name = "Level 2",
            light = 0.75f,
            lightColor = Color.orange,
            maze =
@"
#|###########
#*#_____#___#
#_#_###_###_#
#___#___#___#
#_#####.###_#
#___###_____#
#######_#####
#__________*E
#############
"
        },

        new Level
        {

            name = "Level 3",
            light = 0.5f,
            lightColor = Color.blue,
            maze =
@"
#################
|*_____#________#
######_#_###_##_#
#________###_##_#
#_##_###______###
####_############
#_##.______#___##
#____#####___#_*E
####_#___########
###____#_______##
#################
"
        },

        new Level
        {
            name = "Level 4",
            light = 0.25f,
            lightColor = Color.purple,
            maze =
@"
###################E#
|*______###______.#*#
##_####_#B__#####_#_#
##_####_###_##____#_#
#__####________#_##_#
#######_########_##_#
####____##_____#_##_#
#_##_#_###_#_#_#.##_#
#____#_____#_#_#_##_#
####_#######B#___##_#
####_____##########_#
#B___###_______.____#
#####################
"
        },

        new Level
        {
            name = "Level 5",
            light = 0f,
            lightColor = Color.black,
            maze =
@"
#########################
#____________######____*|
#_###_######_____##_###_#
#_#__________###_##___#_#
#B#_############____###_#
#_#_#_________##_######_#
#___#_#######_##_###____#
###_#_#_____#_##_###_####
###.#_#_###_#.##________#
###_#_#__B#_#_#########B#
###_#_#####_#_#____###_##
###_#_______#___##_____*E
###_#########_##_#####_##
###___________##___S___##
#########################
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
  const string LEVEL_FOLDER = "Assets/Scenes/Levels";

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

    Utilities.EnsureFolderPath(LEVEL_FOLDER);


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

  static void BuildScene(
  Level level,
  string scenePath,
  MazeGeneratorSettings settings,
  string templatePath
)
  {
    // --- Prevent overwriting an existing scene ---
    if (System.IO.File.Exists(scenePath))
    {
      Debug.LogWarning($"Scene already exists at path, skipping build:\n{scenePath}");
      return;
    }

    var scene = EditorSceneManager.OpenScene(templatePath, OpenSceneMode.Single);

    // Set lighting
    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    RenderSettings.ambientLight = level.lightColor;


    RenderSettings.fog = true;
    RenderSettings.fogMode = FogMode.ExponentialSquared;
    RenderSettings.fogDensity = 1 - level.light;
    RenderSettings.fogColor = level.lightColor;


    var sun = RenderSettings.sun;
    if (sun != null)
    {
      sun.color = level.lightColor;
    }

    // Build the maze

    var root = new GameObject("Maze");

    // Map: prefab name -> parent GameObject (e.g. "Wall" -> Walls)
    var parentMap = new Dictionary<string, GameObject>();

    var lines = level.maze
        .Replace("\r\n", "\n")
        .Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

    int width = lines[0].Length;
    int depth = lines.Length;

    // --- Instantiate prefabs ---
    for (int z = 0; z < lines.Length; z++)
    {
      for (int x = 0; x < lines[z].Length; x++)
      {
        char c = lines[z][x];
        var prefab = settings.GetPrefab(c);
        if (!prefab) continue;

        string parentName = prefab.name + "s";

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

    // --- Find and resize Ground ---
    var ground = GameObject.Find("Ground");
    if (ground != null)
    {
      var renderer = ground.GetComponent<Renderer>();
      if (renderer != null)
      {
        ground.transform.localScale =
          new Vector3(width * 0.1f, 1f, depth * 0.1f);

        ground.transform.position =
          new Vector3(
            (width - 1) * 0.5f,
            ground.transform.position.y,
            -(depth - 1) * 0.5f
          );
      }
      else
      {
        Debug.LogWarning("Ground object does not have a Renderer. Cannot size it automatically.");
      }
    }
    else
    {
      Debug.LogWarning("Ground object not found in the scene. Place a GameObject named 'Ground'.");
    }

    // --- NavMesh baking ---
    var nav = GameObject.Find("NavMesh Surface");
    if (nav != null && nav.TryGetComponent(out NavMeshSurface surface))
    {
      surface.BuildNavMesh();
    }
    else
    {
      Debug.LogWarning("NavMesh Surface not found or missing NavMeshSurface component.");
    }

    // --- Rotate doors to face nearest DoorMagnet ---

    GameObject magnetsParent = GameObject.Find("DoorMagnets");

    void RotateDoor(Transform door)
    {
      Transform closest = null;
      float minDist = float.MaxValue;

      foreach (Transform child in magnetsParent.transform)
      {
        GameObject magnet = child.gameObject;
        float d = Vector3.SqrMagnitude(
            magnet.transform.position - door.position
        );

        if (d < minDist)
        {
          minDist = d;
          closest = magnet.transform;
        }
      }

      if (closest != null)
      {
        Vector3 dir = closest.position - door.position;
        dir.y = 0f; // keep doors upright

        if (dir.sqrMagnitude > 0.001f)
        {
          door.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
      }
    }

    foreach (Transform door in GameObject.Find("StartDoors").transform)
    {
      RotateDoor(door);
    }

    foreach (Transform door in GameObject.Find("EndDoors").transform)
    {
      RotateDoor(door);
    }

    DestroyImmediate(magnetsParent);


    EditorSceneManager.SaveScene(scene, scenePath);
  }


}