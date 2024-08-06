using UnityEngine;

namespace AC
{
    public class RoomBehaviour : MonoBehaviour
    {
        [Header("Room Settings")]
        public GameObject[] walls;
        public GameObject[] doors;
        public GameObject[] traps;
        private bool[] stat = new bool[4];

        public void UpdateRoom(bool[] status)
        {
            if (walls.Length > 0) {
                for (int i = 0; i < status.Length; i++)
                {
                    walls[i].SetActive(!status[i]);
                }
            }

            if (doors.Length > 0) {
                for (int i = 0; i < status.Length; i++)
                {
                    doors[i].SetActive(status[i]);
                }
            }

            if (traps.Length > 0) {
                for (int i = 0; i < traps.Length; i++)
                {
                    bool setActive = Random.Range(0, 3) == 0;
                    traps[i].SetActive(setActive);
                }
            }
        }

        public bool[] GetStatus()
        {
            for (int i = 0; i < 4; i++)
            {
                stat[i] = !walls[i].activeSelf;
            }
            return stat;
        }
    }
}