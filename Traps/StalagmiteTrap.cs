using UnityEngine;

namespace AC
{
    public class StalagmiteTrap : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                other.GetComponent<Player>().SpawnStalagmites(gameObject);
            }
        }
    }
}
