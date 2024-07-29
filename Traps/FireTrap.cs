using UnityEngine;

namespace AC
{
    public class FireTrap : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            int randomTime = Random.Range(3, 6);
            InvokeRepeating("SpawnFire", Random.Range(1, 2), randomTime);
        }

        private void SpawnFire()
        {
            gameObject.GetComponentInChildren<ParticleSystem>().Play();
            if (audioSource.clip != null)
                audioSource.Play();
        }
    }
}
