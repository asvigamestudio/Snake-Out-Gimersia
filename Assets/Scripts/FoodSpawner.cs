using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food Settings")]
    public FoodData[] foodTypes;
    public int maxFoodCount = 10;
    public Vector3 spawnArea = new Vector3(50, 0, 50);
    public float spawnHeight = 0.5f;

    [Header("Respawn Settings")]
    public float checkInterval = 2f;
    public float respawnDelay = 0.1f;

    [Header("Debug Settings")]
    public bool showDebugArea = true;
    public Color debugColor = new Color(0f, 1f, 0f, 0.15f);

    private List<GameObject> activeFoods = new List<GameObject>();
    private Coroutine checkRoutine;
    private bool isRunning = false;

    // 🔥 NEW — Flag yang dicek FoodRespawnListener untuk mencegah respawn saat scene unload
    [HideInInspector] public bool isShuttingDown = false;

    // =========================== INIT ===========================
    private void Start()
    {
        SpawnInitialBatch();
        isRunning = true;
        checkRoutine = StartCoroutine(CheckFoodRoutine());
    }

    private void OnDisable()
    {
        isShuttingDown = true;
        StopSpawner();
    }

    private void OnDestroy()
    {
        isShuttingDown = true;
        StopSpawner();
    }

    private void StopSpawner()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }

        isRunning = false;
        activeFoods.RemoveAll(f => f == null);
    }

    // =========================== SPAWN AWAL ===========================
    private void SpawnInitialBatch()
    {
        for (int i = 0; i < maxFoodCount; i++)
            SpawnSingleFood(Vector3.zero, true);
    }

    // =========================== LOOP CEK ===========================
    private IEnumerator CheckFoodRoutine()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(checkInterval);

            activeFoods.RemoveAll(f => f == null);

            int currentCount = activeFoods.Count;
            if (currentCount < maxFoodCount)
            {
                int toSpawn = maxFoodCount - currentCount;
                for (int i = 0; i < toSpawn && isRunning; i++)
                {
                    SpawnSingleFood(Vector3.zero, true);
                    yield return new WaitForSeconds(respawnDelay);
                }
            }
        }
    }

    // =========================== SPAWN SATU MAKANAN ===========================
    public void SpawnSingleFood(Vector3 nearPos, bool randomGlobal = false)
    {
        if (isShuttingDown) return; // prevent respawn on shutdown

        FoodData chosenFood = ChooseRandomFood();
        if (chosenFood == null || chosenFood.prefab == null) return;

        Vector3 spawnPos = randomGlobal
            ? new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                0f,
                Random.Range(-spawnArea.z, spawnArea.z)
              ) + transform.position
            : nearPos + new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10));

        spawnPos.y = GetGroundHeight(spawnPos) + spawnHeight;

        GameObject foodObj = Instantiate(chosenFood.prefab, spawnPos, Quaternion.identity);

        // Tambahkan FoodPickup otomatis
        FoodPickup pickup = foodObj.GetComponent<FoodPickup>();
        if (pickup == null)
            pickup = foodObj.AddComponent<FoodPickup>();
        pickup.data = chosenFood;

        // Tambah listener agar spawner tahu kapan dimakan
        FoodRespawnListener listener = foodObj.AddComponent<FoodRespawnListener>();
        listener.spawner = this;

        activeFoods.Add(foodObj);
    }

    // =========================== DETEKSI TINGGI TANAH ===========================
    private float GetGroundHeight(Vector3 pos)
    {
        float height = 0f;
        bool found = false;

        if (Terrain.activeTerrain != null)
        {
            height = Terrain.activeTerrain.SampleHeight(pos);
            found = true;
        }
        else if (Terrain.activeTerrains.Length > 0)
        {
            foreach (var t in Terrain.activeTerrains)
            {
                if (pos.x >= t.transform.position.x && pos.x <= t.transform.position.x + t.terrainData.size.x &&
                    pos.z >= t.transform.position.z && pos.z <= t.transform.position.z + t.terrainData.size.z)
                {
                    height = t.SampleHeight(pos);
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            Ray ray = new Ray(pos + Vector3.up * 100f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
                height = hit.point.y;
        }

        return height;
    }

    // =========================== PILIH MAKANAN RANDOM ===========================
    private FoodData ChooseRandomFood()
    {
        if (foodTypes == null || foodTypes.Length == 0)
            return null;

        float total = 0;
        foreach (var f in foodTypes) total += f.spawnChance;

        float rand = Random.value * total;
        float acc = 0;
        foreach (var f in foodTypes)
        {
            acc += f.spawnChance;
            if (rand <= acc) return f;
        }

        return foodTypes[foodTypes.Length - 1];
    }

    // =========================== DEBUG GIZMOS ===========================
    private void OnDrawGizmos()
    {
        if (!showDebugArea) return;
        Gizmos.color = debugColor;
        Vector3 center = transform.position + new Vector3(0, spawnHeight, 0);
        Vector3 size = new Vector3(spawnArea.x * 2, 0.1f, spawnArea.z * 2);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }

    // =========================== FOOD DIMAKAN ===========================
    public void NotifyFoodEaten(GameObject food)
    {
        if (isShuttingDown) return; // prevent respawn on shutdown

        activeFoods.Remove(food);

        if (isRunning)
            SpawnSingleFood(Vector3.zero, true);
    }
}
