using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class Utilities
{

  private static float CalculatePathLength(NavMeshPath path)
  {
    float length = 0f;
    Vector3[] corners = path.corners;

    for (int i = 1; i < corners.Length; i++)
      length += Vector3.Distance(corners[i - 1], corners[i]);

    return length;
  }

  public static Vector3 GetNavTarget(
    Vector3 origin,
    Vector2 pathRange,
    NavMeshAgent agent,
    int maxAttempts = 200)
  {
    NavMeshPath path = new NavMeshPath();

    for (int i = 0; i < maxAttempts; i++)
    {
      Debug.LogWarning("Finding new target: Iteration #" + i);
      // Pick a direction and overshoot slightly to help in mazes
      Vector3 dir = Random.insideUnitSphere;
      dir.y = 0f;
      dir.Normalize();

      float approxDist = Random.Range(pathRange.x, pathRange.y);
      Vector3 candidate = origin + dir * approxDist;

      if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        continue;

      if (!agent.CalculatePath(hit.position, path))
        continue;

      if (path.status != NavMeshPathStatus.PathComplete)
        continue;

      float length = CalculatePathLength(path);

      if (length >= pathRange.x && length <= pathRange.y)
        return hit.position;
    }

    Debug.LogWarning("Failed to find NavMesh point by path distance.");
    return origin;
  }


  public static void EnsureFolderPath(string path)
  {
    string[] parts = path.Split('/');
    string current = parts[0]; // "Assets"

    for (int i = 1; i < parts.Length; i++)
    {
      string next = $"{current}/{parts[i]}";

      if (!AssetDatabase.IsValidFolder(next))
      {
        AssetDatabase.CreateFolder(current, parts[i]);
      }

      current = next;
    }
  }


}