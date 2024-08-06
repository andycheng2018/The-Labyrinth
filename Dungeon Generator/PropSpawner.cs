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
        public bool useSpawnPoints;
        public Transform[] spawnPoints;
        public float spawnProb = 0.1f;

        private void Start()
        {
            if (useSpawnPoints) {
                SpawnPoint();
            } else {
                SpawnProps();
            }
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

                var obj = Instantiate(selectedProp, transform.position + new Vector3(-7, 0, 7f) + randomPosition, randomRotation);
                obj.transform.SetParent(transform);
            }
        }

        private void SpawnPoint() {
            if (Random.value <= spawnProb)
            {
                GameObject selectedProp = propPrefabs[Random.Range(0, propPrefabs.Length)];
                Transform spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var obj = Instantiate(selectedProp, spawnPos.position, spawnPos.rotation);
                obj.transform.SetParent(transform);
            }
        }
    }
}
