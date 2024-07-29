using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace AC
{
    public class LobbySaver : NetworkBehaviour
    {
        public Lobby? currentLobby;
        public static LobbySaver instance;
        public TMP_InputField seedInputField;
        public NetworkVariable<int> networkSeed = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SeedServerRpc()
        {
            SeedClientRpc();
        }

        [ClientRpc]
        public void SeedClientRpc()
        {
            if (!IsServer) { return; }
            
            if (seedInputField.text == "")
            {
                networkSeed.Value = Random.Range(0, 1000000);
            }
            else
            {
                networkSeed.Value = int.Parse(seedInputField.text);
            }
        }
    }
}
