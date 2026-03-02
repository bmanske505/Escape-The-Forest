using System.Collections;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class SoundEmitter: MonoBehaviour
{
  public AudioClip[] clips;
  [SerializeField] float period;
  private AudioSource src;
  public void Awake()
  {
    src = GetComponent<AudioSource>();
  }

  public void Start()
  {
    StartCoroutine(EmitNoise());
  }

  IEnumerator EmitNoise()
  {
    while (true)
    {

      AudioClip clip = clips[Random.Range(0, clips.Length)];
      src.PlayOneShot(clip);
      yield return new WaitForSeconds(period); // wait the period
    }
  }
}