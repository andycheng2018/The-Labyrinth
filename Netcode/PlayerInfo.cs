using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace AC {
    public class PlayerInfo : MonoBehaviour
    {
        public TMP_Text playerName;
        public Image readyImage;
        public string steamName;
        public ulong steamId;
        public bool isReady;

        private void Start()
        {
            playerName.text = steamName;
        }
    }
}
