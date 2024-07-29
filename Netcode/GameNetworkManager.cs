using UnityEngine;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using UnityEngine.SceneManagement;

namespace AC
{
    public class GameNetworkManager : MonoBehaviour
    {
        private void OnEnable()
        {
            SteamMatchmaking.OnLobbyCreated += LobbyCreated;
            SteamMatchmaking.OnLobbyEntered += LobbyEntered;
            SteamMatchmaking.OnLobbyMemberLeave += LobbyLeave;
            SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
        }

        private void OnDisable()
        {
            SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
            SteamMatchmaking.OnLobbyMemberLeave -= LobbyLeave;
            SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;

            if(NetworkManager.Singleton == null)
            {
                return;
            }
            NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        }

        private void OnApplicationQuit()
        {
            Disconnected();
        }

        private void LobbyCreated(Result result, Lobby lobby) {
            if (result == Result.OK) {
                lobby.SetPublic();
                lobby.SetJoinable(true);
                lobby.SetGameServer(lobby.Owner.Id);
                NetworkManager.Singleton.StartHost();
                Debug.Log(SteamClient.Name +  " created the lobby");
                NetworkTransmission.instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, SteamClient.Name, NetworkManager.Singleton.LocalClientId);
            }
        }

        private void LobbyEntered(Lobby lobby) {
            LobbySaver.instance.currentLobby = lobby;
            GameManager.instance.lobbyID.text = lobby.Id.ToString();

            if (!NetworkManager.Singleton.IsHost) {
                NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
                NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
                NetworkManager.Singleton.StartClient();
                GameManager.instance.ConnectedAsClient();
                GameManager.instance.hostGame.SetActive(true);
                GameManager.instance.joinLobby.SetActive(false);
                Debug.Log(SteamClient.Name +  " entered the lobby");   
            }
        }

        private void LobbyLeave(Lobby lobby, Friend steamId) {
            Debug.Log(steamId.Name + " has left");
            NetworkTransmission.instance.RemoveMeFromDictionaryServerRPC(steamId.Id);
        }

        private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId) {
            await lobby.Join();
        }

        public async void HostLobby() {
            await SteamMatchmaking.CreateLobbyAsync(10);
        }

        public async void JoinLobbyWithID() {
            ulong ID;
            if (!ulong.TryParse(GameManager.instance.lobbyIDInputField.text, out ID))
                return;
            
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

            foreach (Lobby lobby in lobbies) {
                if (lobby.Id == ID) {
                    await lobby.Join();
                    return;
                }
            }
        }

        public void CopyID() {
            TextEditor textEditor = new TextEditor();
            textEditor.text = GameManager.instance.lobbyID.text;
            textEditor.SelectAll();
            textEditor.Copy();
        }

        public void StartGame() {
            if (NetworkManager.Singleton.IsHost) {
                LobbySaver.instance.SeedServerRpc();
                NetworkManager.Singleton.SceneManager.LoadScene("(2) Game Scene", LoadSceneMode.Single);
            }
        }

        public void Disconnected() {
            LobbySaver.instance.currentLobby?.Leave();
            LobbySaver.instance.currentLobby = null;
            if(NetworkManager.Singleton == null)
            {
                return;
            }
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
            }
            else
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            }

            NetworkManager.Singleton.Shutdown(true);
            GameManager.instance.Disconnected();
            Debug.Log(SteamClient.Name + " left the lobby");
        }

        private void Singleton_OnClientDisconnectCallback(ulong _cliendId)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
            if(_cliendId == 0)
            {
                Disconnected();
            }
        }

        private void Singleton_OnClientConnectedCallback(ulong _cliendId)
        {
            NetworkTransmission.instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, SteamClient.Name, _cliendId);
            GameManager.instance.myClientId = _cliendId;
            NetworkTransmission.instance.IsTheClientReadyServerRPC(false, _cliendId);
        }

        private void Singleton_OnServerStarted()
        {
            Debug.Log("Host started");
            GameManager.instance.HostCreated();
        }

        public void Cleanup()
        {
            if (NetworkManager.Singleton != null)
            {
                Destroy(NetworkManager.Singleton.gameObject);
            }

            if (LobbySaver.instance != null)
            {
                Destroy(LobbySaver.instance.gameObject);
            }
        }

        public void Tutorial()
        {
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("(3) Tutorial", LoadSceneMode.Single);
        }

        public void Quit() {
            Application.Quit();
        }
    }
}