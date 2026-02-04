using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Slider staminaSlider;

    void Start()
    {
        staminaSlider.maxValue = playerMovement.maxStamina;
    }

    void Update()
    {
        staminaSlider.value = playerMovement.CurrentStamina;
    }
}
