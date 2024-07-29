using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace AC
{
    public class Arrow : NetworkBehaviour
    {
        public float speed = 10f;

        public override void OnNetworkSpawn() {
            GetComponent<AudioSync>().PlaySound(0);
            StartCoroutine(destroy());
        }

        private void Update() {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private IEnumerator destroy() {
            yield return new WaitForSeconds(2);
            if (gameObject.GetComponent<NetworkObject>().IsSpawned && IsServer)
                gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
