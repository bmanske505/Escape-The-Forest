using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using TMPro;
using UnityEngine.UI;
using Scripts.FirebaseScripts;
using Newtonsoft.Json;

public class SettingsManager : Singleton<SettingsManager>
{

  [SerializeField] private TMP_Text sensitivityXText;
  [SerializeField] private TMP_Text sensitivityYText;
  [SerializeField] private Slider sensitivityXSlider;
  [SerializeField] private Slider sensitivityYSlider;
  [SerializeField] private GameObject settingsMenu;

  private InputAction settingsAction;

  protected override void Awake()
  {
    base.Awake();
    settingsAction = InputSystem.actions.FindAction("ToggleSettings");

    float sensitivityX = PlayerPrefs.GetFloat("sensitivity_x", 100f);
    sensitivityXText.text = $"Look X Sensitivity: {sensitivityX}";
    sensitivityXSlider.value = sensitivityX;

    float sensitivityY = PlayerPrefs.GetFloat("sensitivity_y", 100f);
    sensitivityYText.text = $"Look Y Sensitivity: {sensitivityY}";
    sensitivityYSlider.value = sensitivityY;
  }

  void OnEnable()
  {
    if (settingsAction != null)
      settingsAction.performed += OnToggleSettings;
  }

  void OnDisable()
  {
    if (settingsAction != null)
      settingsAction.performed -= OnToggleSettings;
  }

  public void SetMouseSensitivityX(float value)
  {
    PlayerPrefs.SetFloat("sensitivity_x", value);
    sensitivityXText.text = $"Look X Sensitivity: {value}";
  }

  public void SetMouseSensitivityY(float value)
  {
    PlayerPrefs.SetFloat("sensitivity_y", value);
    sensitivityYText.text = $"Look Y Sensitivity: {value}";
  }

  public void OnToggleSettings(CallbackContext context)
  {
    if (Time.timeScale == 0f && !settingsMenu.activeSelf) return; // prevent if already in another popup
    TogglePopup();
  }

  public void TogglePopup()
  {
    settingsMenu.SetActive(!settingsMenu.activeSelf);

    if (Player.Instance) // we're in the game!
    {
      if (settingsMenu.activeSelf) // we've just a popup on, enable cursor & pause game
      {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
      }
      else // resume game & disable cursor
      {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
      }
    }

    if (!settingsMenu.activeSelf) // we just closed the settings, save the settings to disk!
    {
      LevelMaster.Instance.CacheSensitivityUse();
    }
  }
}
