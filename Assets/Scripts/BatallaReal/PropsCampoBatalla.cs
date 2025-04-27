using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsCampoBatalla : MonoBehaviour
{
    [Header("Plano sobre el que instanciar")]
    public GameObject plane;

    [Header("Prefabs a colocar aleatoriamente")]
    public GameObject[] objectPrefabs;

    [Header("Número de objetos a instanciar")]
    public int numberOfObjects = 20;

    void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        if (plane == null || objectPrefabs.Length == 0)
        {
            Debug.LogWarning("Asignar el plano y al menos un prefab.");
            return;
        }

        // Obtener los límites del plano
        Renderer planeRenderer = plane.GetComponent<Renderer>();
        Vector3 planeSize = planeRenderer.bounds.size;
        Vector3 planePosition = plane.transform.position - new Vector3(0,20,0);

        GameObject elPrefab;
        for (int i = 0; i < numberOfObjects; i++)
        {
            // Seleccionar aleatoriamente un prefab
            GameObject prefabToSpawn;
            //Sobre todo pongo árboles
            if( Random.Range(0,100) > 5)
                prefabToSpawn = objectPrefabs[Random.Range(0, 2)];
            else
                prefabToSpawn = objectPrefabs[Random.Range(2, objectPrefabs.Length)];

            //Pares o impares
            /*
            if( i % 2 == 0)
                prefabToSpawn = objectPrefabs[0];
            else    
                prefabToSpawn = objectPrefabs[1];*/

            // Generar posición aleatoria dentro de los límites del plano
            float randomX = Random.Range(-planeSize.x / 2f, planeSize.x / 2f);
            float randomZ = Random.Range(-planeSize.z / 2f, planeSize.z / 2f);
            Vector3 spawnPosition = new Vector3(planePosition.x + randomX, planePosition.y, planePosition.z + randomZ);

            // Comprobar con un raycast hacia abajo si lo vamos a poner en el río
            float raycastHeight = 5f;
            Vector3 rayOrigin = new Vector3(spawnPosition.x, spawnPosition.y + raycastHeight, spawnPosition.z);
            Ray ray = new Ray(rayOrigin, Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * raycastHeight * 2, Color.red, 2f);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, raycastHeight * 2))
            {
//                Debug.Log("El raycast: he impactado con "+hitInfo.collider.gameObject.name);
                //Sólo pongo props en el Terrain
                if ( hitInfo.collider.CompareTag("Terrain"))
                {
                    // Instanciar el objeto
                    elPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.Euler(0f,Random.Range(0f,360f),0f));
                    //elPrefab = Instantiate(objectPrefabs[0], spawnPosition, Quaternion.identity);
                    //elPrefab.transform.localScale = new Vector3(0.75f,0.75f,0.75f);
                    //Para que conserve su escala y no le afecte la del padre
                    elPrefab.transform.SetParent(plane.transform,worldPositionStays: true);
                }
            }
        }
    }
}
