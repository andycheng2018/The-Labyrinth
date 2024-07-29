using Unity.Netcode;
using UnityEngine;

namespace AC
{
    public class Door : NetworkBehaviour
    {
        [Header("Door Settings")]
        public float rotationSpeed = 2f;
        public NetworkVariable<bool> isOpen = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private AudioSync audioSync;
        private Quaternion closedRotation;
        private Quaternion openRotation;

        private void Start() {
            audioSync = GetComponent<AudioSync>();
            closedRotation = transform.rotation;
            openRotation = Quaternion.Euler(0, 90, 0) * closedRotation;
        }

        private void Update() {
            Quaternion targetRotation = isOpen.Value ? openRotation : closedRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
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