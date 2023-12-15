using System;
using UnityEngine;

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
    public bool randomSeed;
    float[,] falloffMap;

    public Vegetation[] vegetations;
    public float spawnRadius = 50f;
    public Vector2 spawnHeight = new Vector2(0, 20);

    private void Awake() {
		falloffMap = FalloffGenerator.GenerateFalloffMap (mapChunkSize);

        if (randomSeed) {
            seed = UnityEngine.Random.Range(0, 100000);
            GenerateIsland();
        }
	}

    public void GenerateIsland() {
        UnityEngine.Random.InitState(seed);
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
            SpawnNatureObjects(veg.vegetationPrefabs, veg.numberOfVegetations);
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

    private void SpawnNatureObjects(GameObject[] prefabs, int count)
    {
        if (prefabs.Length <= 0) { return; }

        int vegNum = 0;
        while (vegNum <= count)
        {
            Vector3 randomSurfacePoint = GetRandomSurfacePoint();
            if (randomSurfacePoint != Vector3.zero) {
                GameObject prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
                var vegetation = Instantiate(prefab, randomSurfacePoint, Quaternion.identity);
                vegetation.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(0, 8f), UnityEngine.Random.Range(0, 360f), UnityEngine.Random.Range(0, 8f));
                vegetation.transform.SetParent(transform);
                vegNum++;
            }
        }
    }

    private Vector3 GetRandomSurfacePoint()
    {
        Vector3 randomDirection = UnityEngine.Random.onUnitSphere * spawnRadius;
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
    public GameObject[] vegetationPrefabs;
	public int numberOfVegetations;
}