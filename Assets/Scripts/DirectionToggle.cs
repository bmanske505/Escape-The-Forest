
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class DirectionToggle : MonoBehaviour
{
    public GameObject indicator; // Drag your indicator here
    public float displayDuration = 3.0f; // Seconds to show
    public Transform target;
    public Transform player;
    private Coroutine activeCoroutine;

    private InputAction directionAction;

    void Awake()
    {
        directionAction = InputSystem.actions.FindAction("Direction");
    }

    void OnEnable()
    {
        directionAction.performed += OnDirection;
    }

    void OnDisable()
    {
        directionAction.performed -= OnDirection;
    }

    void Start()
    {
        indicator.SetActive(false); // Ensure it starts hidden
    }

    private void OnDirection(CallbackContext context)
    {
        ShowIndicator();
    }

    public void ShowIndicator()
    {
        // Reset the timer if it's already showing
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        // if (target != null && player != null){
        //     // Calculate direction
        //     Vector3 direction = target.position - player.position;
        //     direction.y = 0;
            
        //     if (direction != Vector3.zero){
        //         // Rotate indicator to look in right direction
        //         Quaternion targetRotation = Quaternion.LookRotation(direction);
        //         transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        //     }
        // }
        
        // Update and rotate the image/sprite to point to the right direction
        indicator.SetActive(true);
        yield return new WaitForSeconds(displayDuration); // Pause for set time
        indicator.SetActive(false);
        activeCoroutine = null;
    }
}
