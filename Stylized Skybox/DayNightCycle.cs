using UnityEngine;

public class DayNightCycle : MonoBehaviour {
        public Material grassMaterial;
        public Light sun;
        public float dayDuration = 60f;
        [Range(0, 1)] public float currentTime = 0f;
        public AudioClip dayAmbience;
        public AudioClip nightAmbience;
        private AudioSource audioSource;

        private void Start() {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            UpdateTime();
            UpdateSunPosition();
            UpdateAmbience();
            UpdateGrass();
        }

        private void UpdateTime()
        {
            currentTime += Time.deltaTime / dayDuration;

            if (currentTime >= 1f)
            {
                currentTime = 0f;
            }
        }

        private void UpdateSunPosition() {
            float angle = currentTime * 360f;
            sun.transform.rotation = Quaternion.Euler(new Vector3(angle, 0, 0));
        }

        private void UpdateAmbience() {
            if (currentTime <= 0.5f) {
                audioSource.clip = dayAmbience;
            } else {
                audioSource.clip = nightAmbience;
            }
            if (!audioSource.isPlaying)
                audioSource.Play();
        }

        private void UpdateGrass() {
            grassMaterial.SetFloat("_ShadowPower",  -2 * currentTime);
        }
}