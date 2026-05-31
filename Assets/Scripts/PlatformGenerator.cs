using System.Collections.Generic;
using UnityEngine;

// Genera el suelo infinito. Crea varias plataformas al empezar y cuando el jugador
// deja atras una, la borra y crea una nueva al final para que el camino no se acabe.
public class PlatformGenerator : MonoBehaviour
{
    [Header("Configuracion")]
    public GameObject[] platformPrefabs;   // prefab(s) de plataforma
    public Transform playerTransform;      // el jugador, para saber por donde va
    public float platformLength = 10f;     // largo de cada plataforma en Z
    public int maxActivePlatforms = 15;    // cuantas plataformas hay a la vez
    public float recycleSafetyDistance = 15f; // distancia extra antes de borrar una

    // lista de las plataformas que estan activas ahora
    private List<GameObject> activePlatforms = new List<GameObject>();
    private float nextSpawnZ = 0f;

    private void Start()
    {
        if (platformPrefabs == null || platformPrefabs.Length == 0)
        {
            Debug.LogError("PlatformGenerator: no hay prefabs de plataforma asignados!");
            return;
        }

        // si no asignaron el jugador, lo buscamos por el tag
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        // creamos las primeras plataformas.
        // las 3 primeras van vacias para que el jugador no choque apenas empieza
        for (int i = 0; i < maxActivePlatforms; i++)
        {
            SpawnPlatform(i < 3);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // miramos la plataforma mas vieja
        if (activePlatforms.Count > 0)
        {
            GameObject oldestPlatform = activePlatforms[0];
            // si el jugador ya la paso, la reciclamos
            if (playerTransform.position.z > oldestPlatform.transform.position.z + platformLength + recycleSafetyDistance)
            {
                RecycleOldestPlatform();
            }
        }
    }

    // crea una plataforma al final del camino
    private void SpawnPlatform(bool spawnEmpty = false)
    {
        // elegimos una plataforma al azar (por si hay varias)
        int index = Random.Range(0, platformPrefabs.Length);
        GameObject prefab = platformPrefabs[index];

        // la creamos en la siguiente posicion Z
        GameObject newPlatform = Instantiate(prefab, new Vector3(0f, 0f, nextSpawnZ), Quaternion.identity);

        // si la queremos vacia, apagamos el script Platform asi no genera obstaculos
        if (spawnEmpty)
        {
            Platform platComp = newPlatform.GetComponent<Platform>();
            if (platComp != null)
            {
                platComp.enabled = false;
            }
        }

        activePlatforms.Add(newPlatform);
        nextSpawnZ += platformLength;
    }

    // borra la plataforma mas vieja y crea una nueva al final
    private void RecycleOldestPlatform()
    {
        if (activePlatforms.Count == 0) return;

        // borramos la plataforma vieja (y sus hijos se borran con ella)
        Destroy(activePlatforms[0]);
        activePlatforms.RemoveAt(0);

        // creamos una nueva al final
        SpawnPlatform(false);
    }
}
