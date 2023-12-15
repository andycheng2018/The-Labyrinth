using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public List<AudioClip> musicTracks;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (musicTracks.Count > 0)
        {
            PlayRandomTrack();
        }
        else
        {
            Debug.LogError("No music tracks assigned!");
        }
    }

    private void PlayRandomTrack()
    {
        int randomIndex = Random.Range(0, musicTracks.Count);
        AudioClip randomClip = musicTracks[randomIndex];

        audioSource.clip = randomClip;
        audioSource.Play();

        StartCoroutine(PlayNextRandomTrack(randomClip.length));
    }

    private IEnumerator PlayNextRandomTrack(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayRandomTrack();
    }
}
