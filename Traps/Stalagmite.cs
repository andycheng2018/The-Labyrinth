using Unity.Netcode;
using UnityEngine;
using System.Collections;

namespace AC
{
    public class Stalagmite : NetworkBehaviour
    {
        public float damage = 10f;

        public override void OnNetworkSpawn() {
            GetComponent<AudioSync>().PlaySound(0);
            StartCoroutine(destroy());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Mushroom") { return; }

            if (other.tag == "Player") {
                other.GetComponent<Player>().TakeDamage(damage);
            }

            if (gameObject.GetComponent<NetworkObject>().IsSpawned && IsServer)
                gameObject.GetComponent<NetworkObject>().Despawn();
        }

        private IEnumerator destroy() {
            yield return new WaitForSeconds(1.5f);
            if (gameObject.GetComponent<NetworkObject>().IsSpawned && IsServer)
                gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
