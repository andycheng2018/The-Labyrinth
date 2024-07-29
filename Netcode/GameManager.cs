using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AC {
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public Dictionary<ulong, GameObject> playerInfo = new Dictionary<ulong, GameObject>();

        [Header("Manager References")]
        public GameObject playerCardPrefab;
        public GameObject playerFieldBox;
        public GameObject hostGame;
        public GameObject joinLobby;
        public GameObject readyButton;
        public GameObject startButton;
        public TMP_InputField lobbyIDInputField;
        public TMP_Text readyText;
        public TMP_Text lobbyID;

        [Header("Manager Flags")]
        public ulong myClientId;
        public bool connected;
        public bool inGame;
        public bool isHost;
        public bool isReady;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        public void HostCreated()
        {
            isHost = true;
            connected = true;
        }

        public void ConnectedAsClient()
        {
            isHost = false;
            connected = true;
        }

        public void Disconnected()
        {
            playerInfo.Clear();

            GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
            foreach(GameObject card in playercards)
            {
                Destroy(card);
            }

            NetworkTransmission.instance.IsTheClientReadyServerRPC(false, myClientId);
            NetworkTransmission.instance.RemoveMeFromDictionaryServerRPC(myClientId);
            readyText.text = "Unready";
            readyButton.GetComponent<Image>().color = Color.red;
            startButton.SetActive(false);
            
            isReady = false;
            isHost = false;
            connected = false;
        }

        public void AddPlayerToDictionary(ulong _cliendId, string _steamName, ulong _steamId)
        {
            if (!playerInfo.ContainsKey(_cliendId))
            {
                PlayerInfo _pi = Instantiate(playerCardPrefab, playerFieldBox.transform).GetComponent<PlayerInfo>();
                _pi.steamId = _steamId;
                _pi.steamName = _steamName;
                playerInfo.Add(_cliendId, _pi.gameObject);
            }
        }

        public void UpdateClients()
        {
            foreach(KeyValuePair<ulong,GameObject> _player in playerInfo)
            {
                ulong _steamId = _player.Value.GetComponent<PlayerInfo>().steamId;
                string _steamName = _player.Value.GetComponent<PlayerInfo>().steamName;
                ulong _clientId = _player.Key;

                NetworkTransmission.instance.UpdateClientsPlayerInfoClientRPC(_steamId, _steamName, _clientId);
            }
        }

        public void RemovePlayerFromDictionary(ulong _steamId)
        {
            GameObject _value = null;
            ulong _key = 100;
            foreach(KeyValuePair<ulong,GameObject> _player in playerInfo)
            {
                if(_player.Value.GetComponent<PlayerInfo>().steamId == _steamId)
                {
                    _value = _player.Value;
                    _key = _player.Key;
                }
            }
            if(_key != 100)
            {
                playerInfo.Remove(_key);
            }
            if(_value!= null)
            {
                Destroy(_value);
            }
        }

        public void ReadyButton()
        {
            isReady = !isReady;

            if (isReady) {
                readyButton.GetComponent<Image>().color = Color.green;
                readyText.text = "Ready";
            } else {
                readyButton.GetComponent<Image>().color = Color.red;
                readyText.text = "Unready";
            }

            NetworkTransmission.instance.IsTheClientReadyServerRPC(isReady, myClientId);
        }

        public bool CheckIfPlayersAreReady()
        {
            bool _ready = false;

            foreach(KeyValuePair<ulong,GameObject> _player in playerInfo)
            {
                if (!_player.Value.GetComponent<PlayerInfo>().isReady)
                {
                    startButton.SetActive(false);
                    return false;
                }
                else
                {
                    startButton.SetActive(true);
                    _ready = true;
                }
            }

            return _ready;
        }
    }
}

