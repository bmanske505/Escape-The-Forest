using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Echo : MonoBehaviour
{
  private AudioSource audioSrc;
  private float delay = 1f;

  private InputAction echoAction;

  void Awake()
  {
    audioSrc = GetComponent<AudioSource>();
    echoAction = InputSystem.actions.FindAction("Echo");
  }

  void OnEnable()
  {
    echoAction.performed += OnEcho;
  }

  void OnDisable()
  {
    echoAction.performed -= OnEcho;
  }

  void OnEcho(CallbackContext context)
  {
    if (Sibling.Instance.CurrentState == Sibling.State.Following)
    {
      GameUI.Instance.ShowBanner("Gregory is right behind me! I shouldn't call out to him right now...");
    }
    else
    {
      StartCoroutine(CallForSibling());
    }
  }

  IEnumerator CallForSibling()
  {
    audioSrc.Play();

    // Wait until audio finishes
    yield return new WaitWhile(() => audioSrc.isPlaying);
    yield return new WaitForSeconds(delay);

    // TODO: add logic to only respond if in a range?
    Sibling.Instance.Respond();
  }
}