using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace AC {
    public class NetworkTransmission : NetworkBehaviour
    {
        public static NetworkTransmission instance;

        private void Awake()
        {
            if(instance != null)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMeToDictionaryServerRPC(ulong _steamId,string _steamName, ulong _clientId)
        {
            GameManager.instance.AddPlayerToDictionary(_clientId, _steamName, _steamId);
            GameManager.instance.UpdateClients();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveMeFromDictionaryServerRPC(ulong _steamId)
        {
            RemovePlayerFromDictionaryClientRPC(_steamId);
        }

        [ClientRpc]
        private void RemovePlayerFromDictionaryClientRPC(ulong _steamId)
        {
            Debug.Log("Removing client");
            GameManager.instance.RemovePlayerFromDictionary(_steamId);
        }

        [ClientRpc]
        public void UpdateClientsPlayerInfoClientRPC(ulong _steamId,string _steamName, ulong _clientId)
        {
            GameManager.instance.AddPlayerToDictionary(_clientId, _steamName, _steamId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void IsTheClientReadyServerRPC(bool _ready, ulong _clientId)
        {
            AClientMightBeReadyClientRPC(_ready, _clientId);
        }

        [ClientRpc]
        private void AClientMightBeReadyClientRPC(bool _ready, ulong _clientId)
        {
            foreach(KeyValuePair<ulong,GameObject> player in GameManager.instance.playerInfo)
            {
                if(player.Key == _clientId)
                {
                    player.Value.GetComponent<PlayerInfo>().isReady = _ready;
                    if (_ready) {
                        player.Value.GetComponent<PlayerInfo>().readyImage.color = Color.green;
                    } else {
                        player.Value.GetComponent<PlayerInfo>().readyImage.color = Color.red;
                    }
                    
                    if (NetworkManager.Singleton.IsHost)
                    {
                        Debug.Log(GameManager.instance.CheckIfPlayersAreReady());
                    }
                }
            }
        }
    }
}