using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Echo : MonoBehaviour
{
  private AudioSource audioSrc;
  private float delay = 1f;

  void Awake()
  {
    audioSrc = GetComponent<AudioSource>();
  }

  public void OnUse(InputValue value)
  {
    StartCoroutine(CallForSibling());
  }

  private IEnumerator CallForSibling()
  {
    audioSrc.Play();
    print("Calling!");

    // Wait until audio finishes
    yield return new WaitWhile(() => audioSrc.isPlaying);
    yield return new WaitForSeconds(delay);

    // TODO: add logic to only respond if in a range?
    Sibling.Instance.Respond();
  }
}