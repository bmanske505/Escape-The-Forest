using System.Collections;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{

  public AudioClip[] clips;
  [SerializeField] float period = 1f;
  [SerializeField] float volumeMultiplier = 1;

  private AudioSource src;
  public void Awake()
  {
    src = GetComponent<AudioSource>();
  }

  public void Start()
  {
    if (PlayerPrefs.GetInt("ab_group") == 0) // user is on the audio pipeline
    {
      StartCoroutine(EmitNoise());
    }
  }

  IEnumerator EmitNoise()
  {
    if (clips == null || clips.Length == 0)
    {
      Debug.LogError("No clips assigned to SoundEmitter!");
      yield break;
    }

    src.loop = false;

    while (true)
    {
      if (!gameObject.activeInHierarchy) // safety check
        yield return null;

      AudioClip clip = clips[Random.Range(0, clips.Length)];
      src.PlayOneShot(clip, volumeMultiplier);

      Debug.Log("I made a sound!");

      yield return new WaitForSeconds(period);
    }
  }
}