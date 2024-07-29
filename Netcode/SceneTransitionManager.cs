using System.Collections;
using UnityEngine;

namespace AC
{
    public class SceneTransitionManager : MonoBehaviour
    {
        private void OnEnable()
        {
            StartCoroutine(Spawn());
        }

        public IEnumerator Spawn()
        {
            yield return new WaitForSeconds(0.5f);

            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                player.SpawnObjects();
            }
        }
    }
}
