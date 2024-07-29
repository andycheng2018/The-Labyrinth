using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
    public class TestingNetcodeUI : NetworkBehaviour
    {
        public Button startHostButton;
        public Button startClientButton;
        public bool autoHost;

        private void Awake()
        {
            startHostButton.onClick.AddListener(() => {
                Debug.Log("HOST");
                NetworkManager.Singleton.StartHost();
            });
            startClientButton.onClick.AddListener(() =>
            {
                Debug.Log("CLIENT");
                NetworkManager.Singleton.StartClient();
            });
        }

        private void Start()
        {
            if (autoHost)
                NetworkManager.Singleton.StartHost();
        }
    }
}