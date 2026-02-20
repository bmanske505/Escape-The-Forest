using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using TMPro;

public class SettingsManager : Singleton<SettingsManager>
{
  public float sensitivityX = 100f;
  public float sensitivityY = 100f;

  [SerializeField] private TMP_Text sensitivityXtext;
  [SerializeField] private TMP_Text sensitivityYtext;
  [SerializeField] private GameObject settingsMenu;

  private InputAction settingsAction;

  protected override void Awake()
  {
    base.Awake();
    settingsAction = InputSystem.actions.FindAction("ToggleSettings");

    sensitivityXtext.text = $"Look X Sensitivity: {sensitivityX}";
    sensitivityYtext.text = $"Look Y Sensitivity: {sensitivityY}";
  }

  void OnEnable()
  {
    settingsAction.performed += OnToggleSettings;
  }

  void OnDisable()
  {
    settingsAction.performed -= OnToggleSettings;
  }

  public void SetMouseSensitivityX(float value)
  {
    sensitivityX = value;
    sensitivityXtext.text = $"Look X Sensitivity: {sensitivityX}";
  }

  public void SetMouseSensitivityY(float value)
  {
    sensitivityY = value;
    sensitivityYtext.text = $"Look Y Sensitivity: {sensitivityY}";
  }

  public void OnToggleSettings(CallbackContext context)
  {
    Debug.Log("Trying to toggle!");
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
  }
}
