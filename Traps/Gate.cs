using Unity.Netcode;
using UnityEngine;

namespace AC
{
    public class Gate : NetworkBehaviour
    {
        [Header("Gate Settings")]
        public float moveSpeed = 5f;
        public NetworkVariable<bool> isOpen = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);      
        private AudioSync audioSync;
        private Vector3 topPosition;
        private Vector3 bottomPosition;

        private void Start() {
            audioSync = GetComponent<AudioSync>();
            topPosition = transform.position + new Vector3(0, 5, 0);
            bottomPosition = transform.position;
        }

        private void Update() {
            Vector3 targetPosition = isOpen.Value ? topPosition : bottomPosition;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeStateServerRpc()
        {
            ChangeStateClientRpc();
        }

        [ClientRpc]
        public void ChangeStateClientRpc()
        {
            if (!IsServer) { return; }

            isOpen.Value = !isOpen.Value;
            
            audioSync.PlaySound(0);
        }
    }
}