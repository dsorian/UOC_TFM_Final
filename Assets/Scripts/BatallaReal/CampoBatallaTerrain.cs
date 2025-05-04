using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using Unity.VisualScripting;
using System.ComponentModel;
using System;


public class CampoBatallaTerrain : MonoBehaviour
{
    private int width = 512;              // Number of vertices along the x-axis
    private int height = 512;             // Number of vertices along the z-axis
    //public float scale = 10f;            // Scale of the terrain
    private float maxHeight = 100;        // Maximum height of the terrain
    public Texture2D grassTexture,dirtTexture, rockTexture;     // Textures for the terrain
    public GameObject treePrefab;        // Prefab for the trees
    public GameObject[] otherPrefabs;    // Array of other prefabs to place in the scene
    private GameObject[] allPrefabsTerrain;     //Array con todos los prefabs clonados
    public GameObject allPrefabsPositioned;   //Para tener organizados los árboles y demás prefabs colocados
    public string heightmapFilePath;      // File path for the heightmap text file
    public int numTrees = 20;   //Number of trees to be planted into the terrain
    public int numOtherPrefabs;
    public GameObject BasePropsCampoBatalla;
    private GameObject PropsCampoBatallaLeft,PropsCampoBatallaTop,PropsCampoBatallaRight; //Donde colocaré árboles, rocas, etc rodeando el campo de batalla
    public Texture2D[] heightmapTexture; // Reference to the heightmap textures
    private int tipoEscenario = 0;   //0=desfilader0 1=llano 2=río +3 por cada otro escenario del mismo tipo
    private int posTipoEscenario = 0;   //Posición dentro del tipo de escenario (0,1 o 2 según tenga el paso arriba, en medio o abajo para ríos y desfiladeros o número de llano)
    private int numEscenario = 0;    //Número de escenario que se va a usar(calculado a partir de tipoEscenario y posTipoEscenario
    private TerrainData terrainData;  //Datos del terreno
    //private NavMeshData navMeshData;  //Datos para los navmeshAgents
    private GameObject elTerrain;  //El terreno que generaremos
    //private NavMeshDataInstance navMeshDataInstance;

    public BatallaManager elBatallaManager;
    private GameObject river; // Referencia al río
    private GameObject bridge,bridge2,bridge3; //Referencia al puente
    public GameObject modeloLimitesCampoBatalla;  //los límite para que no escapen las unidades de combate, morirán si lo tocan
    private GameObject losLimitesCampoBatalla; //Referencia a los límites del campo de batalla

    void Start()
    {
        /*
        GameObject exampleOne = new GameObject();
        exampleOne.name = "GameObject1";
        exampleOne.AddComponent<Rigidbody>();
        
        elTerrain = new GameObject();
        elTerrain.name = "elTerrain";
        elTerrain.AddComponent<Terrain>();
*/
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetKeyDown(KeyCode.U)){
            //SmoothTerrainHeights();
        }

        if (Input.GetKeyUp(KeyCode.N)){
            Debug.Log("CampoBatallaTerrain.cs: Creando campo de batalla de terreno.");
        //    int tipoEscenario = UnityEngine.Random.Range(0,3);
        //    InicializarTerreno(tipoEscenario);
            //gameObject.GetComponent<BatallaManager>().batallaActiva=true;
            //gameObject.GetComponent<NavMeshUpdater>().GenerateNavMesh();
            //gameObject.GetComponent<NavMeshUpdater>().UpdateNavMesh();
        }
    }

    //tipoEscenario= 0 Desfiladero, 1 Llano, 2 Río
    //posTipoEscenario= 0 Paso arriba, 1 Paso enmedio, 2 Paso abajo
    public void InicializarTerreno(int tipoEscenario){
        //El tipo de escenario será uno de los que tenemos disponibles
        this.tipoEscenario = tipoEscenario;
        this.posTipoEscenario = UnityEngine.Random.Range(0,3);
//this.tipoEscenario = 0;  //Forzar escenario
//this.posTipoEscenario = 0;   //Forzar tipo
        numEscenario = this.tipoEscenario*3+this.posTipoEscenario;
        Debug.Log("tipoEscenario: "+this.tipoEscenario+" posTipoEscenario: "+ this.posTipoEscenario+" numEscenario: "+numEscenario);
        // Create a new terrain data
        terrainData = new TerrainData();
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3((float)width, maxHeight, (float)height);

        // Generate the heights for the terrain from a text file
        //float[,] heights = GenerateHeightsFromTextFile();
        // Generate the heights for the terrain from a grayscale image

        float[,] heights = GenerateTerrainFromHeightmap();

        terrainData.SetHeights(0, 0, heights);

        // Create a new terrain and assign the terrain data
        ///-GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        ///-elTerrain = terrainObject.GetComponent<Terrain>();
        elTerrain = Terrain.CreateTerrainGameObject(terrainData);///-
        elTerrain.gameObject.tag = "Terrain";


        //Ini de poner agujeros
        //Sacado de: https://discussions.unity.com/t/create-terrain-hole-at-specific-position-on-terrain-by-script/837754/3
        //Falta saber cuándo y dónde ponerlo, ahora lo pone en la esquina inferior izquierda
        var b = new bool[heights.GetLength(0),heights.GetLength(1)];
        for (var i = 0; i < heights.GetLength(0); i++ ){
            for (var j = 0; j < heights.GetLength(1); j++){
                //Debug.Log("Celda: "+i+" - "+j+" tiene: "+heights[i,j]);
                if( heights[i,j] == 0)
                    b[i, j] = false;
                else
                    b[i,j] = true;
            }
        }

        //No voy a poner agujeros que quedan mal   elTerrain.terrainData.SetHoles(0,0,b);

        //Fin de poner agujeros                    

        //elTerrain.transform.position += new Vector3(0,0,-200);  //Si lo muevo no genera el navmesh en el terrain sino en el (0,0), no sé porqué

        // Set the texture for the terrain
        /* Cargamos la textura del terreno, en la build no funcionaba, al poner un terrain oculto ya va (misterio misterioso)*/
        if (grassTexture != null)
        {
            TerrainLayer terrainLayer = new TerrainLayer();
            terrainLayer.diffuseTexture = grassTexture;
            elTerrain.GetComponent<Terrain>().terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
        }

        //Añadimos otras texturas al terreno
        ApplyTextures();

        //Smooth the terrain heights
        SmoothTerrainHeights();

        // Add trees to the terrain
        //Debug.Log("OJOOOOOOOOOOOOOOOOO: Quito AddTrees() porque da error  por no tener valod mesh renderer y no sé porqué.");
        //AddTrees();

        AddRiver();

        // Place other prefabs in the scene
        PlacePrefabs();

        gameObject.GetComponent<NavMeshUpdater>().elTerrain = elTerrain.GetComponent<Terrain>();
        gameObject.GetComponent<NavMeshUpdater>().GenerateNavMesh();

        elBatallaManager.CreateAllCombatUnitsTerrain();

        losLimitesCampoBatalla = Instantiate(modeloLimitesCampoBatalla, new Vector3(243,78,194), Quaternion.Euler(0f, 0f, 0f));
    }


    //Cargamos de un fichero csv el terreno
    //El formato es líneas de números separadas por ; que nos da la parte decimal de cada punto del mapa
    //Las alturas van:
    // De 10 -> 0.1 Lo más bajo incluyendo fondos de río. 
    // a  50 -> 0.5 La montaña más alta que será el borde del mapa seguramente.
    float[,] GenerateHeightsFromTextFile()
    {
        float[,] heights = new float[width, height];

        if (!string.IsNullOrEmpty(heightmapFilePath))
        {
            string[] lines = File.ReadAllLines(heightmapFilePath);
            Debug.Log("Número de líneas de la imagen: "+ lines.Length);
            if (lines.Length == height)
            {
                for (int y = 0; y < height; y++)
                {
                    string[] heightValues = lines[y].Split(';');
                    Debug.Log("----> Número de columnas de la imagen: "+ heightValues.Length);
                    if (heightValues.Length == width)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            //Lo divido por 100 para evitar los problemas de la , o el . para los decimales
                            Debug.Log("He  leído: "+ heightValues[x]+" parseado: "+int.Parse(heightValues[x]));
                            float heightValue = (float) int.Parse(heightValues[x])/100;
                            heights[x, y] = heightValue;
                            Debug.Log("kk:"+heightValue);
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid number of height values in line " + (y + 1) + " of the heightmap file.");
                    }
                }
            }
            else
            {
                Debug.LogError("Invalid number of lines in the heightmap file.");
            }
        }
        else
        {
            Debug.LogError("Heightmap file path is not provided.");
        }

        return heights;        
    }


    //Generado a partir de una imagen en escala de grises
    float[,] GenerateTerrainFromHeightmap(){

        // Get the size of the heightmap texture
        int width = heightmapTexture[numEscenario].width;
        int height = heightmapTexture[numEscenario].height;

        // Get the heightmap colors
        Color[] colors = heightmapTexture[numEscenario].GetPixels();

        //Rotate array 180 degrees
        //System.Array.Reverse(colors, 0, colors.Length);

        // Create an array to store the heights
        float[,] heights = new float[width, height];
        //Tomo el color del 0,0 como referencia y le doy dos valores por encima y por debajo 
        //para las alturas/llanos suaves/altos
        float colorReferencia = colors[0 * width + 0].grayscale;
        //Debug.Log("Prueba: (width,height):"+width+","+height+") heights.GetLength(0):"+heights.GetLength(0)+" - heights.GetLength(1):"+heights.GetLength(1));
        // Extract height values from the colors
        float colorActual;
        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                //Si está fuera del campo de batalla dejo la altura tal cual
                if( y < 150 || y > 375 || x < 150 || x > 375){
                    heights[y,x] = colors[y * width + x].grayscale;
                }
                else{
                    //Es del campo de batalla, suavizo las alturas
                    //LERP respecto al color de (0,0)
                    colorActual = colors[y * width + x].grayscale;
                    if( colorActual != 0){
                        colorActual = Mathf.Lerp(colorReferencia, colorActual, 0.15f);
                    }
                    heights[y,x] = colorActual;
                    
                    
                    /*
                    // Obtener la altura del píxel actual y sus vecinos
                    if( y == 0 || y >= height-1 || x == 0 || x >= width)
                        heights[y,x] = colorActual;
                    else{
                        float hCenter = colors[y * width + x].grayscale;
                        float hLeft = colors[y * width + (x - 1)].grayscale;
                        float hRight = colors[y * width + (x + 1)].grayscale;
                        float hUp = colors[(y + 1) * width + x].grayscale;
                        float hDown = colors[(y - 1) * width + x].grayscale;

                        // Aplicar suavizado tomando el promedio con los vecinos
                        heights[y, x] = (hCenter + hLeft + hRight + hUp + hDown) / 5.0f;
                    }
                    */
                }                
            }
        }
        return heights;   
    }

    void AddTrees()//Terrain terrain)
    {
        TreePrototype treePrototype = new TreePrototype();
        if(treePrefab == null)
            Debug.Log("El prefab del árbol es null.");

        treePrototype.prefab = treePrefab;
        elTerrain.GetComponent<Terrain>().terrainData.treePrototypes = new TreePrototype[] { treePrototype };

        for (int i = 0; i < numTrees; i++)
        {
            float posX = UnityEngine.Random.Range(0f, elTerrain.GetComponent<Terrain>().terrainData.size.x);
            float posZ = UnityEngine.Random.Range(0f, elTerrain.GetComponent<Terrain>().terrainData.size.z);
            float posY = elTerrain.GetComponent<Terrain>().SampleHeight(new Vector3(posX, 0f, posZ));
Debug.Log("Coloco árbol "+i+" en: "+new Vector3(posX / elTerrain.GetComponent<Terrain>().terrainData.size.x, posY / elTerrain.GetComponent<Terrain>().terrainData.size.y, posZ / elTerrain.GetComponent<Terrain>().terrainData.size.z));
            TreeInstance treeInstance = new TreeInstance();
            treeInstance.position = new Vector3(posX / elTerrain.GetComponent<Terrain>().terrainData.size.x, posY / elTerrain.GetComponent<Terrain>().terrainData.size.y, posZ / elTerrain.GetComponent<Terrain>().terrainData.size.z);
            treeInstance.prototypeIndex = 0;
            treeInstance.widthScale = 1f;
            treeInstance.heightScale = 1f;
            elTerrain.GetComponent<Terrain>().AddTreeInstance(treeInstance);
        }
        elTerrain.GetComponent<Terrain>().Flush();
    }

    bool HasHoleAtPosition(Vector3 worldPosition)
    {
        TerrainData terrainData = elTerrain.GetComponent<Terrain>().terrainData;
        Vector3 terrainPosition = worldPosition - elTerrain.transform.position;

        // Normalizar las coordenadas a la resolución del mapa de agujeros
        float normalizedX = terrainPosition.x / terrainData.size.x;
        float normalizedZ = terrainPosition.z / terrainData.size.z;

        // Obtener las coordenadas del agujero
        int holeX = Mathf.FloorToInt(normalizedX * terrainData.holesResolution);
        int holeZ = Mathf.FloorToInt(normalizedZ * terrainData.holesResolution);

        // Comprobar si las coordenadas están dentro del rango válido
        if (holeX < 0 || holeX >= terrainData.holesResolution || holeZ < 0 || holeZ >= terrainData.holesResolution)
        {
            Debug.LogWarning("La posición está fuera del rango del terreno.");
            return true; // Consideramos fuera del terreno como un agujero
        }

        // Obtener el estado del agujero (false = agujero, true = terreno)
        bool[,] holes = terrainData.GetHoles(0, 0, terrainData.holesResolution, terrainData.holesResolution);
        return !holes[holeZ, holeX]; // Devuelve true si hay un agujero
    }

    void AddRiver()
    {/*
        // Crear el objeto del río como un plano
        river = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        river.transform.localScale = new Vector3(30, 60, 2); // Escalar para que se parezca a un río
        river.transform.position = new Vector3(52, 3.5f, 50); // Colocarlo en el centro del terreno
        river.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        river.GetComponent<MeshRenderer>().material.color = Color.blue; // Pintarlo de azul para simular agua

        // Configurar el río como no transitable
        river.layer = LayerMask.NameToLayer("NoWalkable");
        var obstacle = river.AddComponent<NavMeshObstacle>();
        obstacle.carving = true; // Hacer que carve el NavMesh
    */}

//MIRAR ESTO POR SI PUDIERA SERVIR PARA PONER NAVMESHMODIFIERVOLUME AT RUNTIME
//https://discussions.unity.com/t/navmesh-modifier-or-navmesh-modifier-volume-at-run-time/673816/12



    void PlacePrefabs()
    {
        allPrefabsTerrain = new GameObject[numTrees+numOtherPrefabs];
        foreach (GameObject prefab in otherPrefabs)
        {
            //El agua será el primer elemento del array mientras no lo cambie
            if( prefab.name == "AguaRio" || prefab.name == "Lava"){
                if(tipoEscenario == 2){//Tipo río, necesito agua
                    river = Instantiate(prefab, new Vector3(240f,74.5f,245f) , Quaternion.Euler(90f, 0f, 0f));
                    river.layer = LayerMask.NameToLayer("Not Walkable");
                    var obstacle = river.AddComponent<NavMeshObstacle>();
                    obstacle.carving = true; // Hacer que carve el NavMesh
                }
            }else if( prefab.name == "Puente"){
                Debug.Log("Madre, un puente!");
                /*
                bridge = Instantiate(prefab, new Vector3(48f,8.5f,50f) , Quaternion.Euler(0f, 90f, 0f));
                bridge.layer = LayerMask.NameToLayer("Walkable");
                */
                if(tipoEscenario != 1 ){
                    float posZ =70f;
                    switch(posTipoEscenario){
                        case 0:
                            if( UnityEngine.Random.Range(0,2) == 0)
                                posZ = 240f;
                            else
                                posZ = 210f;
                            break;
                        case 1: 
                            if( UnityEngine.Random.Range(0,2) == 0)
                                posZ = 275f;
                            else
                                posZ = 210f;
                            break;
                        case 2: 
                            if( UnityEngine.Random.Range(0,2) == 0)
                                posZ = 275f;
                            else
                                posZ = 240f;
                            break;
                    }
                    bridge = Instantiate(prefab, new Vector3(248f,80f,posZ) , Quaternion.Euler(0f, 90f, 0f));
                    bridge.layer = LayerMask.NameToLayer("Walkable");
                    /* para testear los tres puentes a la vez
                    bridge = Instantiate(prefab, new Vector3(248f,80f,240) , Quaternion.Euler(0f, 90f, 0f));
                    bridge.layer = LayerMask.NameToLayer("Walkable");

                    bridge = Instantiate(prefab, new Vector3(248f,80f,210) , Quaternion.Euler(0f, 90f, 0f));
                    bridge.layer = LayerMask.NameToLayer("Walkable");

                    bridge = Instantiate(prefab, new Vector3(248f,80f,275) , Quaternion.Euler(0f, 90f, 0f));
                    bridge.layer = LayerMask.NameToLayer("Walkable");
                    */
                }
            }
            /*else if( prefab.name == "Arbol1" || prefab.name == "Arbol2"){
                //Coloco el plano con todos los objetos en lugar de los objetos sueltos.
                for (int i = 0; i < numTrees/2; i++){
                        float posX, posZ;
                        if( UnityEngine.Random.Range(0,100) > 50){ //Ponemos en un lado u otro del campo de batalla y no en el río/grieta si lo hubiera
                            posX = UnityEngine.Random.Range(180f, 240);  //Para poner sólo en la zona de combate
                            posZ = UnityEngine.Random.Range(180f, 300f);
                        }else{
                            posX = UnityEngine.Random.Range(280f, 310f);  //Para poner sólo en la zona de combate
                            posZ = UnityEngine.Random.Range(180f, 300f);
                        }
                        Vector3 position = new Vector3(posX, 76f+UnityEngine.Random.Range(0f,1.5f), posZ);
                        //No ponemos prefab en un agujero del terreno
                        if( ! HasHoleAtPosition(position)){
                            Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                            allPrefabsTerrain[i] = Instantiate(prefab, position , rotation);
                            allPrefabsTerrain[i].transform.localScale += new Vector3(0.25f,0.25f,0.25f);
                            allPrefabsTerrain[i].transform.SetParent(allPrefabsPositioned.transform, true);
                        }
                }
            }else{//Resto de prefabs no contemplados

                for (int i = 0; i < numOtherPrefabs; i++){
                        float posX, posZ;
                        if( UnityEngine.Random.Range(0,100) > 50){ //Ponemos en un lado u otro del campo de batalla y no en el río/grieta si lo hubiera
                            posX = UnityEngine.Random.Range(180f, 240);  //Para poner sólo en la zona de combate
                            posZ = UnityEngine.Random.Range(180f, 300f);
                        }else{
                            posX = UnityEngine.Random.Range(280f, 310f);  //Para poner sólo en la zona de combate
                            posZ = UnityEngine.Random.Range(180f, 300f);
                        }
                        Vector3 position = new Vector3(posX, 76f+UnityEngine.Random.Range(0f,2f), posZ);
                        //No ponemos prefab en un agujero del terreno
                        if( ! HasHoleAtPosition(position)){
                            Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                            allPrefabsTerrain[numTrees+i] = Instantiate(prefab, position , rotation);
                            allPrefabsTerrain[numTrees+i].transform.SetParent(allPrefabsPositioned.transform, true);
                        }
                }
            }
            */
        }
/*
        //Pongo los árboles a mano
        for (int i = 0; i < numTrees; i++){
            float posX = Random.Range(0f, terrain.terrainData.size.x);
            float posZ = Random.Range(0f, terrain.terrainData.size.z);
            float posY = terrain.SampleHeight(new Vector3(posX, 0f, posZ));
            Vector3 position = new Vector3(posX / terrain.terrainData.size.x, posY / terrain.terrainData.size.y, posZ / terrain.terrainData.size.z);
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(treePrefab, , rotation);
        }
*/
        //Pruebas de props alrededor del campo de batalla
        //Coloco los tres PropsCampoBatalla de alrededor
        //Lado izquierdo
        PropsCampoBatallaLeft = Instantiate( BasePropsCampoBatalla, new Vector3(160,75,250), Quaternion.Euler(0f,0f,0f));
        //Lado Derecho
        PropsCampoBatallaTop = Instantiate( BasePropsCampoBatalla, new Vector3(345,75,250), Quaternion.Euler(0f,0f,0f));
        //Lado superior
        PropsCampoBatallaRight = Instantiate( BasePropsCampoBatalla, new Vector3(257,75,340), Quaternion.Euler(0f,90f,0f));

    }

    //Para pintar otras texturas
    private void ApplyTextures(){
        TerrainData terrainData = elTerrain.GetComponent<Terrain>().terrainData;

        //float randomnessFactor = 0.05f; // Proporción de zonas aleatorias (más alto = más zonas con texturas secundarias)

        Vector2[] circularAreas = new Vector2[]{new Vector2(90,100), new Vector2(130,100),new Vector2(190,100)}; // Coordenadas de las áreas circulares donde aplicar texturas específicas
        float[] circularAreaRadii = new float[]{25f,25f,25f}; // Radio de cada área circular

        Texture2D mainTexture = grassTexture;
        Texture2D secondaryTexture = dirtTexture;
        Texture2D tertiaryTexture = rockTexture;

        // Crear dinámicamente las TerrainLayers
        TerrainLayer mainLayer = CreateTerrainLayer(mainTexture, "Main Layer");
        TerrainLayer secondaryLayer = CreateTerrainLayer(secondaryTexture, "Secondary Layer");
        TerrainLayer tertiaryLayer = CreateTerrainLayer(tertiaryTexture, "Tertiary Layer");

        // Asignar las TerrainLayers al terreno
        terrainData.terrainLayers = new TerrainLayer[] { mainLayer, secondaryLayer, tertiaryLayer };

        //Obtenemos como referencia la altura del punto (0,0)
        float referenceTerrainHeight = terrainData.GetHeight(0,0);

        // Obtener dimensiones de la textura del terreno
        int splatWidth = terrainData.alphamapWidth;
        int splatHeight = terrainData.alphamapHeight;

        // Crear el mapa de splat (aplicar las texturas)
        float[,,] splatMap = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, 3];
//Coger la altura de referencia del 0,0 y si es mayor/menor que un umbral poner una u otra textura
        for (int x = 0; x < terrainData.alphamapWidth; x++)
        {
            for (int z = 0; z < terrainData.alphamapHeight; z++)
            {
                // Coordenadas del punto actual
                Vector2 currentPoint = new Vector2(x, z);

                // Pesos de texturas por defecto
                float mainWeight = 0.7f;  // Textura principal (mayoritariamente hierba)
                float secondaryWeight = 0.0f;  // Textura secundaria
                float tertiaryWeight = 0.0f;  // Textura terciaria

                // Convertir coordenadas del splatmap a coordenadas de terreno
                float worldX = (z / (float)splatWidth) * terrainData.size.x;
                float worldZ = (x / (float)splatHeight) * terrainData.size.y;

                // Obtén la altura normalizada del terreno (entre 0 y 1)
                //float currentHeight = terrainData.GetHeight(x, z);
                // Obtener la altura real en esa posición
                float currentHeight = terrainData.GetHeight(
                    Mathf.RoundToInt((worldX / terrainData.size.x) * terrainData.heightmapResolution),
                    Mathf.RoundToInt((worldZ / terrainData.size.y) * terrainData.heightmapResolution)
                );
//Debug.Log("altura del terreno en este punto("+x+","+z+"): "+terrainData.GetHeight(x, z)+" altura referencia: "+referenceTerrainHeight);
                // Aplicar zonas circulares para texturas específicas
                for (int i = 0; i < circularAreas.Length; i++)
                {
                    float distance = Vector2.Distance(currentPoint, circularAreas[i]);

                    if (distance < circularAreaRadii[i])
                    {
                        if(UnityEngine.Random.Range(0,100) < 25){
                            // Zonas circulares
                            if(i==0){ //Referencia
                                mainWeight = 0.0f;
                                secondaryWeight = 0.0f;
                                tertiaryWeight = 1.0f;
                            }else{
                                if (i % 2 == 0)
                                {
                                    // Zonas pares: roca dominante
                                    mainWeight = 0.0f;
                                    secondaryWeight = 0.4f;
                                    tertiaryWeight = 0.4f;
                                }
                                else
                                {
                                    // Zonas impares: mezcla de roca y tierra
                                    mainWeight = 1.0f;
                                    secondaryWeight = 1.0f;
                                    tertiaryWeight = 1.0f;
                                }
                            }
                        }
                    }
                }

                // Aplicar aleatoriedad fuera de zonas circulares
                if (mainWeight == 0.7f) // Solo fuera de las áreas circulares
                {/*
                    float randomValue = Random.value;
                    if (randomValue < randomnessFactor)
                    {
                        secondaryWeight = Random.Range(0.1f, 0.4f);
                        mainWeight -= secondaryWeight;
                    }
                    else if (randomValue < randomnessFactor * 2)
                    {
                        tertiaryWeight = Random.Range(0.1f, 0.4f);
                        mainWeight -= tertiaryWeight;
                    }
                    */
                    

                    if(currentHeight < referenceTerrainHeight - 0.1f){
                        //Debug.Log(" Es menor. Altura actual:"+currentHeight+" punto 0,0: "+referenceTerrainHeight);
                        mainWeight = 0.5f;
                        secondaryWeight = 1.0f;
                        tertiaryWeight = 0.0f;
                    }else if(currentHeight > referenceTerrainHeight +0.1f){
                        //Debug.Log("Es mayor. Altura actual: "+currentHeight+" punto 0,0: "+referenceTerrainHeight);
                        mainWeight = 0.5f;
                        secondaryWeight = 0.0f;
                        tertiaryWeight = 1.0f;
                    }
                }

                // Normalizar los valores
                float totalWeight = mainWeight + secondaryWeight + tertiaryWeight;

                splatMap[x, z, 0] = mainWeight / totalWeight;
                splatMap[x, z, 1] = secondaryWeight / totalWeight;
                splatMap[x, z, 2] = tertiaryWeight / totalWeight;
            }
        }

        // Asignar el splat map al terreno
        terrainData.SetAlphamaps(0, 0, splatMap);
    }

    TerrainLayer CreateTerrainLayer(Texture2D texture, string layerName)
    {
        TerrainLayer layer = new TerrainLayer
        {
            diffuseTexture = texture,
            tileSize = new Vector2(10, 10), // Tamaño del tiling de la textura
            tileOffset = new Vector2(0, 0), // Sin offset inicial
            name = layerName
        };

        return layer;
    }

    /*ESTO FUNCIONA PERO TODO QUEDA IGUAL
    void ApplyTextures()
    {
        TerrainData terrainData = elTerrain.terrainData;

        float randomnessFactor = 0.05f; // Proporción de zonas aleatorias (más alto = más zonas con texturas secundarias)

        Texture2D mainTexture = grassTexture;
        Texture2D secondaryTexture = dirtTexture;
        Texture2D tertiaryTexture = rockTexture;

        // Crear dinámicamente las TerrainLayers
        TerrainLayer mainLayer = CreateTerrainLayer(mainTexture, "Main Layer");
        TerrainLayer secondaryLayer = CreateTerrainLayer(secondaryTexture, "Secondary Layer");
        TerrainLayer tertiaryLayer = CreateTerrainLayer(tertiaryTexture, "Tertiary Layer");

        // Asignar las TerrainLayers al terreno
        terrainData.terrainLayers = new TerrainLayer[] { mainLayer, secondaryLayer, tertiaryLayer };

        // Crear el mapa de splat (aplicar las texturas)
        float[,,] splatMap = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, 3];

        for (int x = 0; x < terrainData.alphamapWidth; x++)
        {
            for (int z = 0; z < terrainData.alphamapHeight; z++)
            {
                // Valores iniciales
                float mainWeight = 1f;
                float secondaryWeight = 0f;
                float tertiaryWeight = 0f;

                // Aplicar manchas aleatorias de texturas secundarias y terciarias
                float randomValue = Random.value;
                if (randomValue < randomnessFactor)
                {
                    mainWeight = 0f;
                    secondaryWeight = 1f;
                }
                else if (randomValue < randomnessFactor * 2)
                {
                    mainWeight = 0f;
                    tertiaryWeight = 1f;
                }

                // Normalizar pesos
                float total = mainWeight + secondaryWeight + tertiaryWeight;
                splatMap[x, z, 0] = mainWeight / total;
                splatMap[x, z, 1] = secondaryWeight / total;
                splatMap[x, z, 2] = tertiaryWeight / total;
            }
        }

        // Asignar el splat map al terreno
        terrainData.SetAlphamaps(0, 0, splatMap);
    }

    TerrainLayer CreateTerrainLayer(Texture2D texture, string layerName)
    {
        TerrainLayer layer = new TerrainLayer
        {
            diffuseTexture = texture,
            tileSize = new Vector2(10, 10), // Tamaño del tiling de la textura
            tileOffset = new Vector2(0, 0), // Sin offset inicial
            name = layerName
        };

        return layer;
    }
    */

    public void DestroyTerrain(){
        for (int i = 0; i < numTrees+numOtherPrefabs; i++){
            Destroy(allPrefabsTerrain[i]);
        }
        Destroy(bridge);
        Destroy(river);
        Destroy(elTerrain);
        Destroy(PropsCampoBatallaLeft);
        Destroy(PropsCampoBatallaTop);
        Destroy(PropsCampoBatallaRight);
        Destroy(losLimitesCampoBatalla);
        NavMesh.RemoveAllNavMeshData();
    }
    //Para suavizar las alturas muy pronunciadas que no permiten pasar
    private void SmoothTerrainHeights()
    {
        int smoothIterations = 1;  //Cuántas veces se aplicará el suavizado al terreno
        float smoothFactor = 0.25f;  //Valor entre 0 (no suaviza) y 1 (suaviza completamente las alturas)

        terrainData = elTerrain.GetComponent<Terrain>().terrainData;
        float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        for (int i = 0; i < smoothIterations; i++)
        {
            for (int x = 1; x < terrainData.heightmapResolution - 1; x++)
            {
                for (int y = 1; y < terrainData.heightmapResolution - 1; y++)
                {
                    float smoothHeight = GetSmoothedHeight(heights, x, y);
                    heights[x, y] = Mathf.Lerp(heights[x, y], smoothHeight, smoothFactor);
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }

    //Quitar walkable bajo el puente
    void RemoveTerrainUnderBridge(GameObject bridge)
    {
        // Definir una región bajo el puente que NO será walkable
        NavMeshBuildSource source = new NavMeshBuildSource
        {
            shape = NavMeshBuildSourceShape.Box,
            size = new Vector3(bridge.transform.localScale.x, 5, bridge.transform.localScale.z), // Caja de exclusión bajo el puente
            transform = Matrix4x4.TRS(bridge.transform.position - new Vector3(0, 2.5f, 0), Quaternion.identity, Vector3.one), // Ajustar posición
            area = NavMesh.GetAreaFromName("Not Walkable") // Asegurar que no es navegable
        };

        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(null, LayerMask.GetMask("Default"), NavMeshCollectGeometry.RenderMeshes, 0, new List<NavMeshBuildMarkup>(), sources);

        // Agregar el área excluida bajo el puente
        sources.Add(source);

        // Regenerar el NavMesh con la nueva configuración
        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
        Bounds navBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
        //NavMeshBuilder.UpdateNavMeshData(navMeshData, buildSettings, sources, navBounds);
        gameObject.GetComponent<NavMeshUpdater>().UpdateNavMesh();
    }

    //Actualizar el navmesh
    void UpdateNavMeshWithBridge(GameObject bridge)
    {
        // Agregar el puente como parte del NavMesh
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(null, LayerMask.GetMask("Default"), NavMeshCollectGeometry.RenderMeshes, 0, new List<NavMeshBuildMarkup>(), sources);

        // Incluir la geometría del puente en el NavMesh
        MeshFilter bridgeMeshFilter = bridge.GetComponentInChildren<MeshFilter>();
        if (bridgeMeshFilter != null)
        {
            NavMeshBuildSource bridgeSource = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = bridgeMeshFilter.sharedMesh,
                transform = bridge.transform.localToWorldMatrix,
                area = 0 // Walkable
            };

            sources.Add(bridgeSource);
        }

        // Regenerar el NavMesh
        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
        Bounds navBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
        //NavMeshBuilder.UpdateNavMeshData(navMeshData, buildSettings, sources, navBounds);
        gameObject.GetComponent<NavMeshUpdater>().UpdateNavMesh();
    }

    private float GetSmoothedHeight(float[,] heights, int x, int y)
    {
        float averageHeight = (heights[x - 1, y - 1] +
                               heights[x - 1, y] +
                               heights[x - 1, y + 1] +
                               heights[x, y - 1] +
                               heights[x, y + 1] +
                               heights[x + 1, y - 1] +
                               heights[x + 1, y] +
                               heights[x + 1, y + 1]) / 8f;

        return averageHeight;
    }
}
