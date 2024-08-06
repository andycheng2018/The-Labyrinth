using UnityEngine;

namespace AC
{
    public class PlatformTrap : MonoBehaviour
    {
        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                animator.SetTrigger("Fall");
            }
        }

        public void Reset() {
            animator.SetTrigger("Idle");
        }
    }
}
