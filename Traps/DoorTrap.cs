using System.Collections;
using UnityEngine;

namespace AC
{
    public class DoorTrap : MonoBehaviour
    {
        private Animator animator;
        private AudioSource audioSource;

        private void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                StartCoroutine(ActivateTrap());
            }
        }

        private IEnumerator ActivateTrap()
        {
            yield return new WaitForSeconds(0.15f);
            animator.SetTrigger("DoorTrap");
            yield return new WaitForSeconds(0.15f);
            if (audioSource.clip != null)
                audioSource.Play();
        }
    }
}
