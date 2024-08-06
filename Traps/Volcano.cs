using UnityEngine;

namespace AC
{
    public class Volcano : MonoBehaviour
    {
        public GameObject lavaProjectile;
        public Transform[] spawnPoints;
        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            InvokeRepeating(nameof(Erupt), Random.Range(5, 10), Random.Range(5, 15));
        }

        private void Erupt() {
            foreach(Transform transforms in spawnPoints) {
                Instantiate(lavaProjectile, transforms.position, Quaternion.identity);
                audioSource.Play();
            }
        }
    }
}
