using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class Utilities
{
  public static Vector3 RandomNavSphere(Vector3 origin, Vector2 distRange, int layermask = NavMesh.AllAreas, int maxAttempts = 30)
  {
    if (distRange.y < distRange.x)
    {
      Debug.LogWarning("RandomNavSphere: Max distance is smaller than min distance. Swapping values.");
      distRange = new Vector2(distRange.y, distRange.x);
    }

    for (int i = 0; i < maxAttempts; i++)
    {
      // Pick a random direction
      Vector3 randDirection = Random.insideUnitSphere.normalized * Random.Range(distRange.x, distRange.y);
      randDirection += origin;

      // Sample NavMesh
      if (NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, distRange.y, layermask))
      {
        float distance = Vector3.Distance(origin, navHit.position);

        // Check if it is within the min-max range
        if (distance >= distRange.x && distance <= distRange.y)
        {
          return navHit.position;
        }
      }
    }

    // If all attempts fail, return origin as fallback
    Debug.LogWarning("RandomNavSphere: Could not find valid point within range. Returning origin.");
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