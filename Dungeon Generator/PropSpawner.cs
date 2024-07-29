using Unity.Netcode;
using UnityEngine;

namespace AC
{
    public class PropSpawner : MonoBehaviour
    {
        public GameObject[] propPrefabs;
        public int numberOfProps;
        public float spawnAreaWidth;
        public float spawnAreaLength;

        private void Start()
        {
            SpawnProps();
        }

        private void SpawnProps()
        {
            for (int i = 0; i < numberOfProps; i++)
            {
                GameObject selectedProp = propPrefabs[Random.Range(0, propPrefabs.Length)];
                Vector3 randomPosition = new Vector3(
                     + Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f),
                    0f,
                    Random.Range(-spawnAreaLength / 2f, spawnAreaLength / 2f)
                );
                Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                Instantiate(selectedProp, transform.position + new Vector3(-7, 0, 7f) + randomPosition, randomRotation);   
            }
        }
    }
}
