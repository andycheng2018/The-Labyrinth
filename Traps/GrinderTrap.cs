using System.Collections;
using UnityEngine;

namespace AC
{
    public class GrinderTrap : MonoBehaviour
    {
        private Animator animator;
        private AudioSource audioSource;

        private void Start()
        {
            animator = GetComponentInChildren<Animator>();
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
            yield return new WaitForSeconds(0.01f);
            animator.SetTrigger("GrinderTrap");
            yield return new WaitForSeconds(0.01f);
            if (audioSource.clip != null)
                audioSource.Play();
        }
    }
}
