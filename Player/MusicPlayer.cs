using UnityEngine;
using System.Collections;

namespace AC
{
    public class MusicPlayer : MonoBehaviour
    {
        public bool playOnStart;
        public bool randomiseStartPosition;

        public AudioSource bed;
        public AudioSource melody;
        public AudioSource perc;
        public AudioSource fx;

        public AudioClip[] melodyArray;
        public AudioClip[] percArray;
        public AudioClip[] fxArray;

        public int bpm = 95;
        public int beatsPerBar = 4;
        public int barsPerTrigger = 2;

        [Range(0, 100)] public int melodyChance = 60;
        [Range(0, 100)] public int percChance = 40;
        [Range(0, 100)] public int fxChance = 20;

        [Range(0, 1)] public float panVariation;

        private float timer;
        private float triggerTime;
        private bool musicPlaying;

        private void Start()
        {
            triggerTime = 60f / bpm * beatsPerBar * barsPerTrigger;
            if (playOnStart == true)
                StartMusic();
        }

        private void Update()
        {
            if (musicPlaying == true)
            {
                timer += Time.deltaTime;
                if (timer > triggerTime)
                {
                    PlayClips();
                }
            }
        }

        public void StartMusic()
        {
            if (musicPlaying == false)
            {
                StopAllCoroutines();
                timer = 0f;
                if (randomiseStartPosition)
                {
                    bed.time = Random.Range(0, bed.clip.length);
                }
                bed.Play();
                musicPlaying = true;
            }
        }

        public void StopMusic()
        {
            StartCoroutine(FadeAndStopMusic());
        }

        private void PlayClips()
        {
            timer = 0f;
            int melodyRoll = Random.Range(0, 100);
            int percRoll = Random.Range(0, 100);
            int fxRoll = Random.Range(0, 100);

            if (melodyChance > melodyRoll && !melody.isPlaying)
            {
                int melodyIndex = Random.Range(0, melodyArray.Length);
                melody.panStereo = RandomPan();
                melody.clip = melodyArray[melodyIndex];
                melody.Play();
            }

            if (percChance > percRoll && !perc.isPlaying)
            {
                int percIndex = Random.Range(0, percArray.Length);
                perc.panStereo = RandomPan();
                perc.clip = percArray[percIndex];
                perc.Play();
            }

            if (fxChance > fxRoll && !fx.isPlaying)
            {
                int fxIndex = Random.Range(0, fxArray.Length);
                fx.panStereo = RandomPan();
                fx.clip = fxArray[fxIndex];
                fx.Play();
            }
        }

        private float RandomPan()
        {
            float panValue = panVariation;

            if (panVariation != 0)
            {
                panValue = Random.Range(-panVariation, panVariation);
            }

            return panValue;
        }

        private IEnumerator FadeAndStopMusic()
        {
            yield return null;
            bed.Stop();
            melody.Stop();
            perc.Stop();
            fx.Stop();
            musicPlaying = false;
            StopAllCoroutines();
        }
    }
}