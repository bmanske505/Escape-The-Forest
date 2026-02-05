using UnityEngine;

[RequireComponent(typeof(Light))]
public class SunProgression : MonoBehaviour
{
    [Header("Level Mapping")]
    [Tooltip("How many levels total affect the sun (1–5)")]
    public int maxSunLevels = 5;

    [Header("Sun Rotation (X Axis)")]
    public float noonAngle = 50f;
    public float nightAngle = -10f;

    [Header("Light Intensity")]
    public float maxIntensity = 1.2f;
    public float minIntensity = 0.15f;

    [Header("Sun Colors")]
    public Color dayColor = new Color(1f, 0.95f, 0.85f);
    public Color sunsetColor = new Color(1f, 0.6f, 0.3f);
    public Color nightColor = new Color(0.4f, 0.45f, 0.6f);

    [Header("Transition")]
    public float transitionSpeed = 1.5f;

    private Light sunLight;
    private int cachedLevel = -1;

    private void Awake()
    {
        sunLight = GetComponent<Light>();
        sunLight.type = LightType.Directional;
    }

    private void Update()
    {
        if (LevelMaster.Instance == null)
            return;

        // Convert scene index → sun level (1–5)
        int levelIndex = GetSunLevelFromLevelMaster();

        if (levelIndex != cachedLevel)
        {
            cachedLevel = levelIndex;
        }

        ApplySunSettings(levelIndex);
    }

    int GetSunLevelFromLevelMaster()
    {
        // index is 0-based → convert to 1–5 range
        int level = Mathf.Clamp(LevelMaster.Instance.GetLevelIndex() + 1, 1, maxSunLevels);
        return level;
    }

    void ApplySunSettings(int level)
    {
        float t = Mathf.InverseLerp(1, maxSunLevels, level);

        // Rotation
        float targetAngle = Mathf.Lerp(noonAngle, nightAngle, t);
        Quaternion targetRotation = Quaternion.Euler(targetAngle, 170f, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * transitionSpeed
        );

        // Intensity
        float targetIntensity = Mathf.Lerp(maxIntensity, minIntensity, t);
        sunLight.intensity = Mathf.Lerp(
            sunLight.intensity,
            targetIntensity,
            Time.deltaTime * transitionSpeed
        );

        // Color
        Color mid = Color.Lerp(dayColor, sunsetColor, t);
        Color final = Color.Lerp(mid, nightColor, t * t);
        sunLight.color = Color.Lerp(
            sunLight.color,
            final,
            Time.deltaTime * transitionSpeed
        );
    }
}
