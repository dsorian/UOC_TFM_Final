using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NavMeshGenerator : MonoBehaviour
{
    public Terrain terrain; // Referencia al terreno generado por código
    public float agentRadius = 0.5f; // Radio del NavMesh Agent
    public float agentHeight = 2f; // Altura del NavMesh Agent
    public int agentMaxSlope = 30; // Pendiente máxima permitida para el NavMesh Agent

    public float agentMaxClimb = 0.6f;  //Mío para probar

    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;

    void Start()
    {
        GenerateNavMesh();
    }

    void GenerateNavMesh()
    {
        NavMesh.RemoveAllNavMeshData();

        NavMeshData tempNavMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(tempNavMeshData);
        navMeshData = tempNavMeshData;
        navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData, transform.position, transform.rotation);

        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByIndex(0);
        buildSettings.agentRadius = agentRadius;
        buildSettings.agentHeight = agentHeight;
        buildSettings.agentSlope = agentMaxSlope;
        buildSettings.agentClimb = 0.6f;  //Mío para probar

        List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();
        NavMeshBuildSource buildSource = new NavMeshBuildSource();
        buildSource.shape = NavMeshBuildSourceShape.Terrain;
        buildSource.transform = Matrix4x4.TRS(terrain.transform.position, Quaternion.identity, Vector3.one);
        buildSource.area = 0;
        buildSource.sourceObject = terrain.terrainData;
        buildSources.Add(buildSource);

        Bounds bounds = terrain.terrainData.bounds;
        NavMeshBuilder.UpdateNavMeshData(navMeshData, buildSettings, buildSources, bounds);
    }

    void OnDestroy()
    {
        if (navMeshDataInstance.valid)
            NavMesh.RemoveNavMeshData(navMeshDataInstance);
    }

}
