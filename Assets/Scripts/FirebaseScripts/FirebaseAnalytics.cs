using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Scripts.FirebaseScripts
{
  public static class FirebaseAnalytics
  {

    [DllImport("__Internal")]
    private static extern void LogDocumentToFirebase(string collectionName, string jsonData);
#if UNITY_WEBGL && !UNITY_EDITOR

    public static void LogDocument(string collectionName, object obj)
    {
      // Deserialize to a Dictionary for flexibility
      var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(obj));

      // Add extra fields
      dict["userId"] = PlayerPrefs.GetString("id", "unregistered");
      dict["level"] = PlayerPrefs.GetInt("level_index", -1);
      dict["playthrough"] = PlayerPrefs.GetInt("playthrough", 0);
      dict["version"] = Application.version;
      dict["platform"] = Application.platform.ToString();
      dict["domain"] = Application.absoluteURL;
      dict["sensitivity_x"] = PlayerPrefs.GetFloat("sensitivity_x", 100f);
      dict["sensitivity_y"] = PlayerPrefs.GetFloat("sensitivity_y", 100f);
      dict["unix_time"] = Time.

      // Convert back to JSON
      string enrichedJson = JsonConvert.SerializeObject(dict);

      // Send to Firebase
      LogDocumentToFirebase(collectionName, enrichedJson);
    }

#else
    public static void LogDocument(string collectionName, object jsonData) { }
#endif
  }
}