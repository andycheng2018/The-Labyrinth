using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace AC
{
    public class OrbProjectile : NetworkBehaviour
    {
        [Header("Orb Projectile Settings")]
        public float speed = 5f;
        public float rotationSpeed = 5f;
        public Transform target;

        public override void OnNetworkSpawn() {
            GetComponent<AudioSync>().PlaySound(0);
            StartCoroutine(destroy());
        }

        private void Update()
        {
            FindClosestPlayer();

            if (target == null) { return; }

            RotateTowardsPlayer();
            MoveTowardsPlayer();
        }

        private void FindClosestPlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float closestDistance = Mathf.Infinity;

            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = player.transform;
                }
            }
        }

        private void RotateTowardsPlayer()
        {
            Vector3 direction = (target.position - transform.position + new Vector3(0, Random.Range(0, 2), 0)).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        private void MoveTowardsPlayer()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private IEnumerator destroy() {
            yield return new WaitForSeconds(4);
            if (gameObject.GetComponent<NetworkObject>().IsSpawned && IsServer)
                gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}