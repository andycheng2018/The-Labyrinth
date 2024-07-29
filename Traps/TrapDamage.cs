using UnityEngine;

namespace AC
{
    public class TrapDamage : MonoBehaviour
    {
        [Header("Trap Damage Settings")]
        public float damage = 100;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                other.GetComponent<Player>().TakeDamage(damage);
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            if (other.tag == "Player")
            {
                other.GetComponent<Player>().TakeDamage(damage);
            }
        }
    }
}
