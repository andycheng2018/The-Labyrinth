using UnityEngine;
using Random = UnityEngine.Random;
using System;

namespace AC {
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class IslandGenerator : MonoBehaviour {
        public const int mapChunkSize = 241;
        [Range(0,6)] public int levelOfDetail;
        public float noiseScale;
        public int octaves;
        [Range(0,1)] public float persistance;
        public float lacunarity;
        public int seed;
        public Vector2 offset;
        public float meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;
        public bool autoUpdate;
        public bool useFalloff;
        float[,] falloffMap;

        public Vegetation[] vegetations;
        public float spawnRadius = 50f;
        public Vector2 spawnHeight = new Vector2(0, 20);

         public void Start() {
            falloffMap = FalloffGenerator.GenerateFalloffMap (mapChunkSize);
            seed = Random.Range(0, 100000);
            
            GenerateIsland();
        }

        public void GenerateIsland() {
            Random.InitState(seed);
            GenerateLand();
            ClearVegetation();
            GenerateVegetation();
        }

        public void GenerateLand() {
            float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
            for (int y = 0; y < mapChunkSize; y++) {
                for (int x = 0; x < mapChunkSize; x++) {
                    if (useFalloff) {
                        noiseMap [x, y] = Mathf.Clamp01(noiseMap [x, y] - falloffMap [x, y]);
                    }
                }
            }
            GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh = MeshGenerator.GenerateTerrainMesh (noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail).CreateMesh();
        }

        public void ClearVegetation() {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        public void GenerateVegetation() {
            foreach (Vegetation veg in vegetations) {
                SpawnNatureObjects(veg.vegetationPrefab, veg.numberOfVegetations);
            }
        }

        private void OnValidate() {
            if (lacunarity < 1) {
                lacunarity = 1;
            }
            if (octaves < 0) {
                octaves = 0;
            }

            falloffMap = FalloffGenerator.GenerateFalloffMap (mapChunkSize);
        }

        private void SpawnNatureObjects(GameObject prefab, int count)
        {
            if (prefab == null) { return; }

            int vegNum = 0;
            while (vegNum <= count)
            {
                Vector3 randomSurfacePoint = GetRandomSurfacePoint();
                if (randomSurfacePoint != Vector3.zero) {
                    var vegetation = Instantiate(prefab, randomSurfacePoint, Quaternion.identity);
                    vegetation.transform.eulerAngles = new Vector3(Random.Range(0, 8f), Random.Range(0, 360f), Random.Range(0, 8f));
                    vegetation.transform.SetParent(transform);
                    vegNum++;
                }
            }
        }

        private Vector3 GetRandomSurfacePoint()
        {
            Vector3 randomDirection = Random.onUnitSphere * spawnRadius;
            randomDirection.y = 0;
            Vector3 randomPoint = transform.position + randomDirection;

            RaycastHit hit;
            if (Physics.Raycast(randomPoint + Vector3.up * 100f, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                float height = hit.point.y;
                if (height >= spawnHeight.x && height <= spawnHeight.y)
                {
                    return hit.point;
                }
            }

            return Vector3.zero;
        }
    }

    [System.Serializable]
    public struct Vegetation
    {
        public String vegetationName;
        public GameObject vegetationPrefab;
        public int numberOfVegetations;
    }
}
