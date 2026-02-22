using UnityEngine;
using UnityEditor;
using System.IO;

public class CameraCapture : EditorWindow
{
  public Camera captureCamera;
  public int width = 1920;
  public int height = 1080;

  [MenuItem("Tools/Camera Capture")]
  public static void ShowWindow()
  {
    GetWindow<CameraCapture>("Camera Capture");
  }

  void OnGUI()
  {
    GUILayout.Label("Capture Camera to PNG", EditorStyles.boldLabel);
    captureCamera = (Camera)EditorGUILayout.ObjectField("Camera", captureCamera, typeof(Camera), true);
    width = EditorGUILayout.IntField("Width", width);
    height = EditorGUILayout.IntField("Height", height);

    if (captureCamera != null && GUILayout.Button("Export PNG"))
    {
      ExportCameraToPNG(captureCamera, width, height);
    }
  }

  void ExportCameraToPNG(Camera cam, int w, int h)
  {
    // Create a high-precision RenderTexture (supports HDR)
    RenderTexture rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGBHalf);
    rt.antiAliasing = 4; // smooth edges
    rt.useMipMap = false;
    rt.autoGenerateMips = false;

    cam.targetTexture = rt;
    cam.Render(); // renders the camera with lighting & post-processing

    // Read pixels into Texture2D
    RenderTexture.active = rt;
    Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true); // linear color
    tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
    tex.Apply();

    // Reset camera
    cam.targetTexture = null;
    RenderTexture.active = null;
    DestroyImmediate(rt);

    // Apply gamma correction to match Game view
    Color[] pixels = tex.GetPixels();
    for (int i = 0; i < pixels.Length; i++)
    {
      pixels[i] = pixels[i].gamma;
    }
    tex.SetPixels(pixels);
    tex.Apply();

    // Save to PNG
    string path = EditorUtility.SaveFilePanel("Save Camera PNG", "", cam.name + ".png", "png");
    if (!string.IsNullOrEmpty(path))
    {
      File.WriteAllBytes(path, tex.EncodeToPNG());
      AssetDatabase.Refresh();
      Debug.Log("Saved PNG to: " + path);
    }

    DestroyImmediate(tex);
  }
}