using System.Collections;
using UnityEngine;

namespace AC
{
    public class SpikeTrap : MonoBehaviour
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
            if (other.tag == "Player" || other.tag == "Monster")
            {
                StartCoroutine(ActivateTrap());
            }
        }

        private IEnumerator ActivateTrap()
        {
            yield return new WaitForSeconds(0.1f);
            animator.SetTrigger("SpikeTrap");
            yield return new WaitForSeconds(0.11f);
            if (audioSource.clip != null)
                audioSource.Play();
        }
    }
}
