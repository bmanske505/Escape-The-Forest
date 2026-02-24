using System.Runtime.InteropServices;

namespace Scripts.FirebaseScripts
{
  public static class FirebaseAnalytics
  {

    [DllImport("__Internal")]
    private static extern void LogDocumentToFirebase(string collectionName, string jsonData);
#if UNITY_WEBGL && !UNITY_EDITOR

    public static void LogDocument(string collectionName, object jsonData)
    {
      // Convert original object to JSON string first
      string originalJson = JsonUtility.ToJson(jsonData);

      // Deserialize to a Dictionary for flexibility
      var dict = JsonUtility.FromJson<SerializableDictionary>(originalJson).ToDictionary();

      // Add extra fields
      dict["userId"] = PlayerPrefs.GetString("id", "unregistered");
      dict["version"] = Application.version;
      dict["platform"] = Application.platform.ToString();
      dict["domain"] = Application.absoluteURL;
      dict["sensitivity_x"] = PlayerPrefs.GetFloat("sensitivity_x", 100f);
      dict["sensitivity_y"] = PlayerPrefs.GetFloat("sensitivity_y", 100f);

      // Convert back to JSON
      string enrichedJson = JsonUtility.ToJson(new SerializableDictionary(dict));

      // Send to Firebase
      LogDocumentToFirebase(collectionName, enrichedJson);
    }

#else
    public static void LogDocument(string collectionName, object jsonData) { }
#endif
  }
}