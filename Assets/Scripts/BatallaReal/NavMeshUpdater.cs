using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NavMeshUpdater : MonoBehaviour
{
    public Terrain elTerrain;
    public float agentRadius = 0.5f; // Radio del NavMesh Agent
    public float agentHeight = 2f; // Altura del NavMesh Agent
    public int agentMaxSlope = 30; // Pendiente máxima permitida para el NavMesh Agent

    public float agentMaxClimb = 0.6f;  //Mío para probar

    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;

    private void Start()
    {
        //GenerateNavMesh();
        //UpdateNavMesh();
    }

    void Update()
    {
        //Para recalcular el navmesh de nuevo
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("NavMeshUpdater: Actualizando el navmesh manualmente.");
            GenerateNavMesh();
            UpdateNavMesh();
        }
    }
    /*
     * Generate the navmesh surface so soldiers can walk on it
     */
    public void GenerateNavMesh()
    {
        navMeshData = new NavMeshData();
        navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);

        var sources = new List<NavMeshBuildSource>();
        var markups = new List<NavMeshBuildMarkup>();
        NavMeshBuilder.CollectSources(elTerrain.terrainData.bounds, 1, NavMeshCollectGeometry.RenderMeshes, 0,markups, sources);

        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByIndex(0);
        buildSettings.agentRadius = agentRadius;
        buildSettings.agentHeight = agentHeight;
        buildSettings.agentSlope = agentMaxSlope;
        buildSettings.agentClimb = agentMaxClimb;  //Mío para probar

        //NavMeshBuilder.UpdateNavMeshData(navMeshData, new NavMeshBuildSettings(), sources, elTerrain.terrainData.bounds);
        NavMeshBuilder.UpdateNavMeshData(navMeshData, buildSettings, sources, elTerrain.terrainData.bounds);
        Debug.Log("Generado navMeshData");  
    }


    public bool UpdateNavMesh()
    {
        var sources = new List<NavMeshBuildSource>();
        var markups = new List<NavMeshBuildMarkup>();
        //NavMeshBuilder.CollectSources(GetComponent<Bounds>(), 0, NavMeshCollectGeometry.RenderMeshes, 0, markups, sources);
        //NavMeshBuilder.UpdateNavMeshData(navMeshData, new NavMeshBuildSettings(), sources, GetComponent<Bounds>());
        NavMeshBuilder.CollectSources(elTerrain.terrainData.bounds, 0, NavMeshCollectGeometry.RenderMeshes, 0, markups, sources);

        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByIndex(0);
        buildSettings.agentRadius = agentRadius;
        buildSettings.agentHeight = agentHeight;
        buildSettings.agentSlope = agentMaxSlope;
        buildSettings.agentClimb = agentMaxClimb;  //Mío para probar

        //return NavMeshBuilder.UpdateNavMeshData(navMeshData, new NavMeshBuildSettings(), sources, elTerrain.terrainData.bounds);
        return NavMeshBuilder.UpdateNavMeshData(navMeshData, buildSettings, sources, elTerrain.terrainData.bounds);
    }

    private void OnDestroy()
    {
        navMeshDataInstance.Remove();
    }
}