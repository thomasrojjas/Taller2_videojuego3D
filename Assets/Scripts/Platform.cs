using UnityEngine;

// Controla cada pedazo de calle (plataforma).
// Cuando se crea, pone aleatoriamente monedas, zombies u obstaculos en los carriles.
// Cuando la plataforma se borra, sus hijos se borran junto con ella.
public class Platform : MonoBehaviour
{
    [Header("Carriles")]
    public float[] laneXs = new float[] { -3f, 0f, 3f }; // posicion X de los 3 carriles
    public float spawnZOffset = 0f; // donde aparecen las cosas a lo largo de la calle

    [Header("Cosas que puede generar")]
    public GameObject coinPrefab;
    public GameObject zombiePrefab;
    public GameObject[] vehiclePrefabs;
    public GameObject barricadePrefab;

    [Header("Probabilidades")]
    [Range(0f, 1f)]
    public float zombieChance = 0.35f;
    [Range(0f, 1f)]
    public float vehicleChance = 0.35f;
    [Range(0f, 1f)]
    public float barricadeChance = 0.15f;

    private void Start()
    {
        SpawnRandomElements();
    }

    // genera las cosas al azar en los carriles
    private void SpawnRandomElements()
    {
        // decidimos cuantas cosas poner (1 o 2)
        int elementsCount = Random.Range(1, 3);

        // para no poner dos cosas en el mismo carril
        bool[] laneOccupied = new bool[3];

        for (int i = 0; i < elementsCount; i++)
        {
            // elegimos un carril libre
            int laneIndex = GetRandomUnoccupiedLane(laneOccupied);
            if (laneIndex == -1) break; // ya no quedan carriles libres

            laneOccupied[laneIndex] = true;
            float spawnX = laneXs[laneIndex];

            // decidimos que cosa va a aparecer
            float randVal = Random.value;
            GameObject prefabToSpawn = null;
            Vector3 spawnLocalPos = new Vector3(spawnX, 0f, spawnZOffset);

            if (randVal < zombieChance)
            {
                prefabToSpawn = zombiePrefab;
                spawnLocalPos.y = 0f; // el zombie va en el piso
            }
            else if (randVal < zombieChance + vehicleChance)
            {
                if (vehiclePrefabs != null && vehiclePrefabs.Length > 0)
                {
                    prefabToSpawn = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
                }
            }
            else if (randVal < zombieChance + vehicleChance + barricadeChance)
            {
                prefabToSpawn = barricadePrefab;
            }
            else
            {
                prefabToSpawn = coinPrefab;
                spawnLocalPos.y = 0.8f; // la moneda flota un poco arriba
            }

            if (prefabToSpawn != null)
            {
                // lo creamos en la posicion del carril
                Vector3 worldPos = transform.TransformPoint(spawnLocalPos);

                // si es zombie lo giramos para que mire al jugador
                Quaternion rotation = prefabToSpawn == zombiePrefab ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;

                GameObject spawnedObj = Instantiate(prefabToSpawn, worldPos, rotation);

                // lo hacemos hijo de la plataforma para que se borre junto con ella
                spawnedObj.transform.SetParent(this.transform);
            }
        }
    }

    // busca un carril que todavia no este ocupado
    private int GetRandomUnoccupiedLane(bool[] laneOccupied)
    {
        int startIndex = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            int checkIndex = (startIndex + i) % 3;
            if (!laneOccupied[checkIndex])
            {
                return checkIndex;
            }
        }
        return -1; // no hay carriles libres
    }
}
