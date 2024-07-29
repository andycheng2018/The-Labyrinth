using System.Collections;
using UnityEngine;

namespace AC
{
    public class ArrowTrap : MonoBehaviour
    {
        private IEnumerator OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                for (int i = 0; i < 3; i++) {
                    other.GetComponent<Player>().SpawnArrows(gameObject);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }
}
