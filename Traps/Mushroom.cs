using UnityEngine;

namespace AC
{
    public class Mushroom : MonoBehaviour
    {
        [Header("Mushroom Settings")]
        [Range(1, 100)] public int spawnRange;
        public GameObject projectile;
        public GameObject[] players;
        public Transform player;

        private void Start()
        {
            InvokeRepeating("SpawnProjectile", Random.Range(5, 6), Random.Range(6, 10));
        }

        private void Update()
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            player = FindClosestPlayer();

            if (player == null) { return; }

            float distanceToTarget = Vector3.Distance(player.position, transform.position);

            if (distanceToTarget > spawnRange)
            {
                player = null;
            }
        }

        public Transform FindClosestPlayer()
        {
            Transform closestPlayer = null;
            float closestDistance = Mathf.Infinity;

            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }

            return closestPlayer;
        }

        private void SpawnProjectile()
        {
            if (player != null)
            {
                player.GetComponent<Player>().SpawnOrbs(gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRange);
        }
    }
}
