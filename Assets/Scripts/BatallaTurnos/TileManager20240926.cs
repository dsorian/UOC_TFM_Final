using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;
//using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.IO.Enumeration;
using System;
//using UnityEngine.Rendering.VirtualTexturing;



/*
 * Para controlar los eventos de los hexágonos.
 * Resaltarlos cuando se pase por encima de ellos, seleccionar cuando se 
 * clicke, colocar al jugador, pathfinding, ...
*/
public class TileManager20240926 : MonoBehaviour
{
    //public TileManager instance;
    //public GameObject player;
    //public GameObject[] enemies; //Enemigos a clonar
    public GameObject highlight;
    public GameObject selector;
    private List<HexTile> path;
    private Dictionary<Vector3Int, HexTile> tiles;
    public GameObject unaFrontera; //Para clonar en las fronteras de las celdas

    private bool stop = false; //Para el tick del movimiento
    public static TileManager20240926 instance;
   
    //Para gestión de los turnos
    public GameObject modeloEjercitoP1,modeloEjercitoP2;
    public int numEjercitosP1=2,numEjercitosP2=2;
    private List<GameObject> ejercitosP1,ejercitosP2;
    private int currentPlayer = 1;
    private GameObject ejercitoSeleccionado = null;
    //private int indiceEjercitoP1 = -1;
    //private int indiceEjercitoP2 = -1;
    private bool moviendoTropa = false;
    private bool atacando = false;
    private bool uniendoTropas = false;
    private bool ocupandoEstado = false;

    //Info partida
    public GameObject elMapaReino;

    //Para editar el mapa y crear los estados
    //private int numTileTemp;        
    private bool editandoMapa = false;
    private int numEstadoActual=0;
    private int numEstadoAnterior=0;  //Para saber qué estado había seleccionado antes
    private bool estadoSeleccionado = false;
   // private int modoJuego = 0; //0=Combates automáticos 1=Combates reales
    //private int tipoCeldaActual;
    public GameObject elCanvasEdicion,elCanvasUI;

    private void Awake(){
        instance = this;
        tiles = new Dictionary<Vector3Int, HexTile>();
        HexTile[] hexTiles = gameObject.GetComponentsInChildren<HexTile>();
        //Register all the tiles
        foreach( HexTile hexTile in hexTiles){
            RegisterTile(hexTile);
        }
        //Get each tiles set of neighbours
        foreach(HexTile hexTile in hexTiles){
            List<HexTile> neighbours = GetNeighbours(hexTile);
            hexTile.neighbours = neighbours;
        }
        //Put the player somewhere
        /*
        HexTile tile = hexTiles[UnityEngine.Random.Range(0,(hexTiles.Length-1))];
        player.GetComponent<Ejercito>().cubeCoordinate = tile.cubeCoordinate;
        player.transform.position = tile.transform.position + new Vector3(0,1f,0);
        player.GetComponent<Ejercito>().currentTile=tile;
        
        //Layout the enemies
        foreach(GameObject enemy in enemies){
            //Find a place for them
            HexTile[] lasTiles = tiles.Values.ToArray();
            tile = lasTiles[UnityEngine.Random.Range(0,lasTiles.Length)];
            / *No tengo tipos de tiles, de momento
            while ( tile.tileType != HexTileGenerationSettings.TileType.Standard){
                tile = hexTiles.GetRandom();
            }* /
            enemy.transform.position = tile.transform.position + new Vector3(0,1f,0);
            //Face a random tile
            enemy.transform.LookAt(lasTiles[UnityEngine.Random.Range(0,lasTiles.Length)].transform.position, Vector3.up);
            enemy.GetComponent<Ejercito>().currentTile = tile;
        }
        */
        //Creamos los ejércitos en sus casillas
        HexTile tile;
        ejercitosP1 = new List<GameObject>(); //new GameObject[numEjercitosP1];
        for(int i=0; i<numEjercitosP1;i++){
            ejercitosP1.Add(Instantiate(modeloEjercitoP1, new Vector3(0,0,0), new Quaternion()));

            HexTile[] lasTiles = tiles.Values.ToArray();
            tile = lasTiles[UnityEngine.Random.Range(0,lasTiles.Length)];
            ejercitosP1[i].GetComponent<Ejercito>().currentTile = tile;
//            ejercitosP1[i].GetComponent<Ejercito>().indiceEjercito = i;
            ejercitosP1[i].GetComponent<Ejercito>().numPlayer = 1;
        }
        ejercitosP2 = new List<GameObject>(); // new GameObject[numEjercitosP2];
        for(int i=0; i<numEjercitosP2;i++){
            //Lo creamos
            ejercitosP2.Add(Instantiate(modeloEjercitoP2, new Vector3(0,0,0), new Quaternion()));

            HexTile[] lasTiles = tiles.Values.ToArray();
            tile = lasTiles[UnityEngine.Random.Range(0,lasTiles.Length)];
            ejercitosP2[i].GetComponent<Ejercito>().currentTile = tile;
            //ejercitosP2[i].GetComponent<Ejercito>().indiceEjercito = i;
            ejercitosP2[i].GetComponent<Ejercito>().numPlayer = 2;
        }
        
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Tick());

        CargarMapaJuego();

        Vector2Int aux;
        HexTile tile;
        int posIni1 = 9;  // para ocupar siempre el mismo estado luego poner -->UnityEngine.Random.Range(1,12);
        int posIni2 = 3;  // como arriba o poner lo que interese para el juego UnityEngine.Random.Range(14,22);
        for(int i=0; i<numEjercitosP1;i++){
            aux = elMapaReino.GetComponent<MapaReino>().listaEstados[posIni1].GetCoordsCapital();
            tile = elMapaReino.GetComponent<MapaReino>().elGridMapa[aux.y*elMapaReino.GetComponent<MapaReino>().gridSize.y+aux.x].GetComponent<HexTile>();
            ejercitosP1[i].transform.position = tile.transform.position+ new Vector3(0,1f,0);
            ejercitosP1[i].transform.LookAt(new Vector3(0,0,-1));  //Mira siempre al sur
            ejercitosP1[i].GetComponent<Ejercito>().currentTile = tile;
            elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().OcuparCapital(1,ejercitosP1[i]);
            posIni1++;
        }
        for(int i=0; i<numEjercitosP2;i++){
            aux = elMapaReino.GetComponent<MapaReino>().listaEstados[posIni2].GetCoordsCapital();
            tile = elMapaReino.GetComponent<MapaReino>().elGridMapa[aux.y*elMapaReino.GetComponent<MapaReino>().gridSize.y+aux.x].GetComponent<HexTile>();
            ejercitosP2[i].transform.position = tile.transform.position+ new Vector3(0,1f,0);
            ejercitosP2[i].transform.LookAt(new Vector3(0,0,-1));  //Mira siempre al sur
            ejercitosP2[i].GetComponent<Ejercito>().currentTile = tile;
            elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().OcuparCapital(2,ejercitosP2[i]);
            posIni2++;
        }

        //player.transform.position = player.GetComponent<Ejercito>().currentTile.transform.position + new Vector3(0,1f,0);

        PintarFronteras();
        ActualizarEstadosVecinos();
        elMapaReino.GetComponent<MapaReino>().ColocarCapitales();

        numEstadoAnterior = 0;
        numEstadoActual = 0;

        elCanvasEdicion.transform.GetChild(0).GetComponent<TMP_Text>().text = "Estado Actual: "+ numEstadoActual;
        elCanvasUI.transform.GetChild(0).GetComponent<TMP_Text>().text = "Turno: Player"+currentPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E)){
            Debug.Log("tilemanager: Has pulsado E");
            mostrarInfoJuego();
            /*
            Desactivamos la edición para que el jugador no la active
            editandoMapa = !editandoMapa;
            if ( editandoMapa ){
                elCanvasEdicion.gameObject.SetActive(true);
                elCanvasUI.gameObject.SetActive(false);
            }else{
                elCanvasEdicion.gameObject.SetActive(false);
                elCanvasUI.gameObject.SetActive(true);
            }
            elCanvasEdicion.transform.GetChild(0).GetComponent<TMP_Text>().text = "Estado Actual: "+ numEstadoActual;
            */
        }
        mostrarInfoJuego();
    }

    public void RegisterTile(HexTile tile){
        tiles.Add(tile.cubeCoordinate, tile);
    }

    public List<HexTile> GetNeighbours(HexTile tile){
        List<HexTile> neighbours = new List<HexTile>();

        Vector3Int[] neighbourCoords = new Vector3Int[] {
            new Vector3Int(1,-1,0),
            new Vector3Int(1,0,-1),
            new Vector3Int(0,1,-1),
            new Vector3Int(-1,1-0),
            new Vector3Int(-1,0,1),
            new Vector3Int(0,-1,1),
        };
        foreach (Vector3Int neighbourCoord in neighbourCoords){
            Vector3Int tileCoord = tile.cubeCoordinate;

            if(tiles.TryGetValue(tileCoord + neighbourCoord, out HexTile neighbour)){
                neighbours.Add(neighbour);
            }
        }
        return neighbours;
    }

    public void OnHighlightTile(HexTile tile){
        //Mostrar el estado y el player
        elCanvasUI.transform.GetChild(0).GetComponent<TMP_Text>().text = "Turno: Player"+ currentPlayer+"\n Estado: "+tile.numEstado;

        highlight.transform.position = tile.transform.position;
        if( editandoMapa && tile.numEstado != -1){
            if( Input.GetKey(KeyCode.Z)){
                //Pintar las tiles
                if(elCanvasEdicion.transform.GetChild(7).GetComponent<Toggle>().isOn){
                //Cambiamos el material al seleccionado
                //tile.SetMaterial(tile.materiales[1]);
                elMapaReino.GetComponent<MapaReino>().listaEstados[tile.numEstado].SetMaterialCelda(tile,elCanvasEdicion.transform.GetChild(8).GetComponent<TMP_Dropdown>().value);
                //Debug.Log("pintando la celda "+tile.name+" de color: "+ elCanvasEdicion.transform.GetChild(8).GetComponent<TMP_Dropdown>().value);
                }else{
                    //Pintar estados
                    if( tile.numEstado != numEstadoActual){
                        //Cambiamos el estado a la tile clickada
                        elMapaReino.GetComponent<MapaReino>().CambiarTileDeEstado(tile,numEstadoActual);
                    }
                }
            }
            PintarFronteras();
        }else{
            //Si se ha seleccionado un estado habrá que resaltar la capital si estoy sobre un vecino suyo
        }
    }

    public void OnSelectTile(HexTile tile){
        numEstadoAnterior = numEstadoActual;
        numEstadoActual = tile.numEstado;
        //Modo edición del mapa
        if(editandoMapa && tile.numEstado != -1){
            elCanvasEdicion.transform.GetChild(0).GetComponent<TMP_Text>().text = "Estado Actual: "+ numEstadoActual;
            //Elegir capital
            if( elCanvasEdicion.transform.GetChild(6).GetComponent<Toggle>().isOn ){
                //La celda será la capital del estado
                elMapaReino.GetComponent<MapaReino>().listaEstados[numEstadoActual].SetCapital(tile);
                elMapaReino.GetComponent<MapaReino>().ColocarCapital(numEstadoActual,tile.transform.position);
                Debug.Log("Capital puesta en: "+tile.name);
            }
        }else{  //Modo Juego (movimiento de tropas)
            RealizarMovimientoJuego(tile);
        }
        //if( tile.tipoCelda <= 1) //Si es agua o neutro no la seleccionamos para moverse
        //    return;
        selector.transform.position = tile.transform.position;
    }

    public void OnDrawGizmos(){
        if (path != null){
            foreach(HexTile tile in path){
                Gizmos.DrawCube(tile.transform.position + new Vector3(0f, 0.5f, 0.5f), new Vector3(0.5f,0.5f,0.5f));
            }
        }
    }

    //      ***********
    //          ***     ***     ******  
    //          ***     ***     ***
    //          ***     ***     ***
    //          ***     ***     ******
    //Para el movimiento que modificará el gamemanager según los movimientos de los jugadores
    public IEnumerator Tick(){
        /*Mover sólo player
            moviendoTropa = player.GetComponent<Ejercito>().HandleMovement();
            yield return new WaitForSeconds(1f);
            StartCoroutine(Tick());
            */
    //Por alguna razón se hacen muchos ticks simultáneos. Averiguar
    //el por qué. Ver cómo funciona el StartCoroutine y el IEnumerator
        while(true){
            //Check if the game is over
/*Esto no vale, quitarlo si no peta. Era para los enemigos que capturaban al player en el ejemplo
            foreach( GameObject enemy in ejercitosP2 ){
                if (enemy.GetComponent<Ejercito>().currentTile == ejercitosP1[0].GetComponent<Ejercito>().currentTile
                    || enemy.GetComponent<Ejercito>().nextTile == ejercitosP1[0].GetComponent<Ejercito>().currentTile){
                    //stop = true;
                    //Debug.Log("Jugador atrapado");
                    //Canvas.Show();
                }
*/
                if (stop){
                    yield return null;
                }else{
                    /*
                    moviendoTropa = false;
                    if (currentPlayer == 1){
                        for ( int i=0; i < ejercitosP1.Count;i++ ){
                            //Si tiene path establecido lo movemos en ese path y salimos (sólo se mueve un ejército cada vez)
                            if (ejercitosP1[i].GetComponent<Ejercito>().GotCurrentPath()){
                                moviendoTropa = ejercitosP1[i].GetComponent<Ejercito>().HandleMovement();
                                if( ! moviendoTropa ){//Ha llegado a su destino
                                    Capital laCapitalDestino = elMapaReino.GetComponent<MapaReino>().capitalesEstados[numEstadoActual].GetComponent<Capital>();
                                    if( ocupandoEstado){
                                        Debug.Log("=======>En Tick: He llegado a mi destino que es ocupar el estado.");
                                        ocupandoEstado = false;
                                        laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,ejercitoSeleccionado);
                                    }
                                    if( atacando ){
                                        Debug.Log("=======>En Tick: He llegado a mi destino que es atacar al enemigo.");
                                        int vencedor = ResolverCombate(ejercitosP1[i],laCapitalDestino.ejercitoOcupante);
                                        if( vencedor == 1){
                                            Debug.Log("Ha ganado el player 1!! Eliminar unidad del player 2 pendiente. Hay que ocupar el estado.");
                                            Destroy(laCapitalDestino.ejercitoOcupante);
                                            ejercitosP2.Remove(laCapitalDestino.ejercitoOcupante);
                                            laCapitalDestino.GetComponent<Capital>().OcuparCapital(vencedor,ejercitoSeleccionado);
                                        }else{
                                            Debug.Log("Ha ganado el player 2!! Eliminar unidad del player 1 pendiente. Hay que ocupar el estado.");
                                            Destroy(ejercitosP1[i]);
                                            ejercitosP1.RemoveAt(i);
                                            laCapitalDestino.GetComponent<Capital>().OcuparCapital(vencedor,laCapitalDestino.ejercitoOcupante);
                                        }
                                        atacando = false;
                                    }
                                    if( uniendoTropas ){
                                        Debug.Log("=======>En Tick: He llegado a mi destino que es unir las tropas.");
                                        uniendoTropas = false;

                                    }
                                }
                                break;
                            }
                        }
                    }else{
                            for ( int i=0; i < ejercitosP2.Count;i++ ){
                            //Si tiene path establecido lo movemos en ese path y salimos (sólo se mueve un ejército cada vez)
                            if (ejercitosP2[i].GetComponent<Ejercito>().GotCurrentPath()){
                                moviendoTropa = ejercitosP2[i].GetComponent<Ejercito>().HandleMovement();
                                if( ! moviendoTropa ){//Ha llegado a su destino
                                    Capital laCapitalDestino = elMapaReino.GetComponent<MapaReino>().capitalesEstados[numEstadoActual].GetComponent<Capital>();
                                    if( ocupandoEstado){
                                        Debug.Log("=======>En Tick: He llegado a mi destino que es ocupar el estado.");
                                        ocupandoEstado = false;
                                        laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,ejercitoSeleccionado);
                                    }
                                    if( atacando ){
                                        Debug.Log("=======>En Tick: He llegado a mi destino que es atacar al enemigo.");
                                        int vencedor = ResolverCombate(laCapitalDestino.ejercitoOcupante,ejercitosP2[i]);
                                        if( vencedor == 1){
                                            Debug.Log("Ha ganado el player 1!! Eliminar unidad del player 2 pendiente. Hay que ocupar el estado.");
                                            Destroy(ejercitosP2[i]);
                                            ejercitosP2.Remove(ejercitosP2[i]);
                                            // El ocupante ha ganado, no hay que ocupar. laCapitalDestino.GetComponent<Capital>().OcuparCapital(vencedor,ejercitoSeleccionado);
                                        }else{
                                            Debug.Log("Ha ganado el player 2!! Eliminar unidad del player 1 pendiente. Hay que ocupar el estado.");
                                            Destroy(laCapitalDestino.ejercitoOcupante);
                                            ejercitosP1.RemoveAt(i);
                                            laCapitalDestino.GetComponent<Capital>().OcuparCapital(vencedor,ejercitoSeleccionado);
                                        }
                                        atacando = false;

                                    }
                                    if( uniendoTropas ){
                                        Debug.Log("=======>En Tick: He llegado a mi destino que es unir las tropas.");
                                        uniendoTropas = false;

                                    }
                                }
                                break;
                            }
                        }
                    }
                    //Comprobamos si se ha finalizado el turno
                    if( !moviendoTropa){
                        //Si hemos movido todas las tropas finaliza el turno
                        //Reiniciamos los movimientos de cada ejército
                        int numMovidos = 0;
                        int tamanyoArray = 0;
                        if( currentPlayer == 1){
                            tamanyoArray = ejercitosP1.Count;
                            foreach( GameObject unEjercito in ejercitosP1){
                                if( unEjercito.GetComponent<Ejercito>().haMovido){
                                    numMovidos++;
                                }
                            }
                        }
                        if( currentPlayer == 2 ){
                            tamanyoArray = ejercitosP2.Count;
                            foreach( GameObject unEjercito in ejercitosP2){
                                if( unEjercito.GetComponent<Ejercito>().haMovido){
                                    numMovidos++;
                                }
                            }
                        }
                        if( tamanyoArray == numMovidos){
                            FinalizarTurno();
                        }
                    }
                    //Fin de comprobación de fin de turno

                    yield return new WaitForSeconds(0.7f);
                    //StartCoroutine(Tick());
                    */
                }
            //}
        }
    }

    private void PintarFronteras(){
        //Pinto todas las fronteras
        HexTile[] hexTiles = gameObject.GetComponentsInChildren<HexTile>();
        /*
        Transform[] kk = transform.GetComponentsInChildren<Transform>().Where(t => t.name == "UnaFrontera(Clone)").ToArray();
        Debug.Log("kk tiene elementos: "+kk.Length);
        foreach(Transform unatransform in kk){
            Debug.Log("destruyendo: "+unatransform.name);
            Destroy(unatransform.gameObject);
        }
        / *
        //Elimino y reseteo las fronteras
        foreach(Transform child in transform){
            Destroy(child.gameObject);
        }* /
        foreach( HexTile laTile in hexTiles){
            for(int i=0; i<laTile.fronterasPintadas.Length; i++){
                Debug.Log("poniendo todo a false................");
                laTile.fronterasPintadas[i] = false;
            }
        }*/
        foreach( HexTile laTile in hexTiles){
            //Falla ahora tenemos las playas tambien laTile.PintarMisFronteras(unaFrontera);
        }
    }

    
    public void SiguienteEstado(){
        if( numEstadoActual < elMapaReino.GetComponent<MapaReino>().listaEstados.Count()-1){
            numEstadoAnterior = numEstadoActual;
            numEstadoActual++;
            //estadoTemp = elMapaReino.GetComponent<MapaReino>().listaEstados[numEstadoActual];
            elCanvasEdicion.transform.GetChild(0).GetComponent<TMP_Text>().text = "Estado Actual: "+ numEstadoActual;
        }
    }

    public void AnteriorEstado(){
        if( numEstadoActual > 0 ){
            numEstadoAnterior = numEstadoActual;
            numEstadoActual--;
            //estadoTemp = elMapaReino.GetComponent<MapaReino>().listaEstados[numEstadoActual];
            elCanvasEdicion.transform.GetChild(0).GetComponent<TMP_Text>().text = "Estado Actual: "+ numEstadoActual;
        }
    }

    

    public void GuardarMapaActual(){
        //Archivo donde guardaremos la información (texto plano, para más seguridad usaríamos binario pero así podemos verlo)
        //Se guarda en C:\Users\david\AppData\LocalLow\DefaultCompany\TFM_DSS
        string nombreArchivo = "";
        string json = "";
        int contador = 0;
        foreach(Estado elEstado in elMapaReino.GetComponent<MapaReino>().listaEstados){
            elEstado.MostrarEstado();
            if(contador>9)
                nombreArchivo = "/MapaReino_Estado_"+contador+".txt";
            else
                nombreArchivo = "/MapaReino_Estado_"+contador+".txt";
            //string json = JsonUtility.ToJson(elMapaReino.GetComponent<MapaReino>().listaEstados[numEstadoActual]);
            json = "";
            
            json = json + JsonUtility.ToJson(elEstado);
            //Guardamos la info
            if(File.Exists(Application.persistentDataPath+nombreArchivo)){
                Debug.Log("GuardarMapaActual(): El fichero json existe y lo voy a borrar para escribirlo de nuevo."+Application.persistentDataPath+nombreArchivo);
                File.Delete(Application.persistentDataPath + nombreArchivo);
            }
            File.WriteAllText(Application.persistentDataPath+nombreArchivo,json);
            contador++;
        }
        
    }

    public void CargarMapaFicheros(){
        string[] archivosJSON = Directory.GetFiles(Application.persistentDataPath,"*.txt");
        Array.Sort(archivosJSON);
        string json;
        Estado estadoAux;
        elMapaReino.GetComponent<MapaReino>().listaEstados.Clear();
        elMapaReino.GetComponent<MapaReino>().listaEstados = new List<Estado>();
        Debug.Log("count: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Count+" Capacity: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Capacity);
        for(int i=0; i<archivosJSON.Length; i++){
            elMapaReino.GetComponent<MapaReino>().listaEstados.Add(null);
        }
        //Debug.Log("Después del for. count: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Count+" Capacity: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Capacity);

        foreach(string archivo in archivosJSON){
            estadoAux = ((Estado) ScriptableObject.CreateInstance(typeof(Estado)));
            json = File.ReadAllText(archivo);
            JsonUtility.FromJsonOverwrite(json,estadoAux); //elMapaReino.GetComponent<MapaReino>().listaEstados[contador]
            Debug.Log("Quiero insertar el estado: "+estadoAux.nombreEstado+" en posicion: "+estadoAux.numEstado+" y la lista tiene "+elMapaReino.GetComponent<MapaReino>().listaEstados.Count+" y la capacity: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Capacity);
            elMapaReino.GetComponent<MapaReino>().listaEstados[estadoAux.numEstado] = estadoAux;
            Debug.Log("He leído el mapa: "+elMapaReino.GetComponent<MapaReino>().listaEstados[estadoAux.numEstado].nombreEstado);
        }

        Debug.Log("Archivos cargados, a pintar fronteras toca!");
        elMapaReino.GetComponent<MapaReino>().ActualizarEstadoDeTiles();
        PintarFronteras();
        Debug.Log("¿Faltaría actualizar el canvas?");
    }

    public void CargarMapaJuego(){       

        string[] estados=  {"{\"numEstado\":0,\"nombreEstado\":\"Estado 0\",\"tileCapital\":-1,\"coordsTiles\":[{\"x\":1,\"y\":1},{\"x\":2,\"y\":1},{\"x\":3,\"y\":1},{\"x\":4,\"y\":1},{\"x\":5,\"y\":1},{\"x\":6,\"y\":1},{\"x\":7,\"y\":1},{\"x\":8,\"y\":1},{\"x\":9,\"y\":1},{\"x\":10,\"y\":1},{\"x\":11,\"y\":1},{\"x\":12,\"y\":1},{\"x\":13,\"y\":1},{\"x\":14,\"y\":1},{\"x\":15,\"y\":1},{\"x\":16,\"y\":1},{\"x\":17,\"y\":1},{\"x\":18,\"y\":1},{\"x\":19,\"y\":1},{\"x\":20,\"y\":1},{\"x\":21,\"y\":1},{\"x\":22,\"y\":1},{\"x\":23,\"y\":1},{\"x\":24,\"y\":1},{\"x\":25,\"y\":1},{\"x\":26,\"y\":1},{\"x\":27,\"y\":1},{\"x\":28,\"y\":1},{\"x\":29,\"y\":1},{\"x\":30,\"y\":1},{\"x\":31,\"y\":1},{\"x\":32,\"y\":1},{\"x\":33,\"y\":1},{\"x\":34,\"y\":1},{\"x\":35,\"y\":1},{\"x\":36,\"y\":1},{\"x\":37,\"y\":1},{\"x\":38,\"y\":1},{\"x\":1,\"y\":2},{\"x\":2,\"y\":2},{\"x\":3,\"y\":2},{\"x\":4,\"y\":2},{\"x\":5,\"y\":2},{\"x\":6,\"y\":2},{\"x\":7,\"y\":2},{\"x\":8,\"y\":2},{\"x\":9,\"y\":2},{\"x\":10,\"y\":2},{\"x\":11,\"y\":2},{\"x\":12,\"y\":2},{\"x\":13,\"y\":2},{\"x\":14,\"y\":2},{\"x\":15,\"y\":2},{\"x\":16,\"y\":2},{\"x\":17,\"y\":2},{\"x\":18,\"y\":2},{\"x\":19,\"y\":2},{\"x\":20,\"y\":2},{\"x\":21,\"y\":2},{\"x\":22,\"y\":2},{\"x\":23,\"y\":2},{\"x\":24,\"y\":2},{\"x\":25,\"y\":2},{\"x\":26,\"y\":2},{\"x\":27,\"y\":2},{\"x\":28,\"y\":2},{\"x\":29,\"y\":2},{\"x\":30,\"y\":2},{\"x\":31,\"y\":2},{\"x\":32,\"y\":2},{\"x\":33,\"y\":2},{\"x\":34,\"y\":2},{\"x\":35,\"y\":2},{\"x\":36,\"y\":2},{\"x\":37,\"y\":2},{\"x\":38,\"y\":2},{\"x\":1,\"y\":3},{\"x\":2,\"y\":3},{\"x\":3,\"y\":3},{\"x\":4,\"y\":3},{\"x\":5,\"y\":3},{\"x\":6,\"y\":3},{\"x\":7,\"y\":3},{\"x\":8,\"y\":3},{\"x\":9,\"y\":3},{\"x\":10,\"y\":3},{\"x\":11,\"y\":3},{\"x\":12,\"y\":3},{\"x\":13,\"y\":3},{\"x\":14,\"y\":3},{\"x\":15,\"y\":3},{\"x\":16,\"y\":3},{\"x\":17,\"y\":3},{\"x\":18,\"y\":3},{\"x\":19,\"y\":3},{\"x\":20,\"y\":3},{\"x\":21,\"y\":3},{\"x\":22,\"y\":3},{\"x\":23,\"y\":3},{\"x\":24,\"y\":3},{\"x\":25,\"y\":3},{\"x\":26,\"y\":3},{\"x\":27,\"y\":3},{\"x\":28,\"y\":3},{\"x\":29,\"y\":3},{\"x\":30,\"y\":3},{\"x\":31,\"y\":3},{\"x\":32,\"y\":3},{\"x\":33,\"y\":3},{\"x\":34,\"y\":3},{\"x\":35,\"y\":3},{\"x\":36,\"y\":3},{\"x\":37,\"y\":3},{\"x\":38,\"y\":3},{\"x\":1,\"y\":4},{\"x\":2,\"y\":4},{\"x\":3,\"y\":4},{\"x\":4,\"y\":4},{\"x\":5,\"y\":4},{\"x\":6,\"y\":4},{\"x\":9,\"y\":4},{\"x\":12,\"y\":4},{\"x\":13,\"y\":4},{\"x\":14,\"y\":4},{\"x\":17,\"y\":4},{\"x\":18,\"y\":4},{\"x\":19,\"y\":4},{\"x\":21,\"y\":4},{\"x\":22,\"y\":4},{\"x\":23,\"y\":4},{\"x\":24,\"y\":4},{\"x\":25,\"y\":4},{\"x\":26,\"y\":4},{\"x\":27,\"y\":4},{\"x\":28,\"y\":4},{\"x\":29,\"y\":4},{\"x\":30,\"y\":4},{\"x\":31,\"y\":4},{\"x\":32,\"y\":4},{\"x\":34,\"y\":4},{\"x\":35,\"y\":4},{\"x\":36,\"y\":4},{\"x\":37,\"y\":4},{\"x\":38,\"y\":4},{\"x\":1,\"y\":5},{\"x\":2,\"y\":5},{\"x\":3,\"y\":5},{\"x\":7,\"y\":5},{\"x\":13,\"y\":5},{\"x\":14,\"y\":5},{\"x\":15,\"y\":5},{\"x\":17,\"y\":5},{\"x\":18,\"y\":5},{\"x\":19,\"y\":5},{\"x\":21,\"y\":5},{\"x\":22,\"y\":5},{\"x\":23,\"y\":5},{\"x\":24,\"y\":5},{\"x\":25,\"y\":5},{\"x\":26,\"y\":5},{\"x\":27,\"y\":5},{\"x\":28,\"y\":5},{\"x\":29,\"y\":5},{\"x\":30,\"y\":5},{\"x\":31,\"y\":5},{\"x\":34,\"y\":5},{\"x\":35,\"y\":5},{\"x\":36,\"y\":5},{\"x\":37,\"y\":5},{\"x\":38,\"y\":5},{\"x\":1,\"y\":6},{\"x\":2,\"y\":6},{\"x\":12,\"y\":6},{\"x\":13,\"y\":6},{\"x\":14,\"y\":6},{\"x\":21,\"y\":6},{\"x\":22,\"y\":6},{\"x\":24,\"y\":6},{\"x\":25,\"y\":6},{\"x\":26,\"y\":6},{\"x\":27,\"y\":6},{\"x\":33,\"y\":6},{\"x\":34,\"y\":6},{\"x\":35,\"y\":6},{\"x\":36,\"y\":6},{\"x\":37,\"y\":6},{\"x\":38,\"y\":6},{\"x\":1,\"y\":7},{\"x\":2,\"y\":7},{\"x\":3,\"y\":7},{\"x\":13,\"y\":7},{\"x\":14,\"y\":7},{\"x\":15,\"y\":7},{\"x\":25,\"y\":7},{\"x\":26,\"y\":7},{\"x\":34,\"y\":7},{\"x\":35,\"y\":7},{\"x\":36,\"y\":7},{\"x\":37,\"y\":7},{\"x\":38,\"y\":7},{\"x\":1,\"y\":8},{\"x\":2,\"y\":8},{\"x\":3,\"y\":8},{\"x\":11,\"y\":8},{\"x\":12,\"y\":8},{\"x\":14,\"y\":8},{\"x\":15,\"y\":8},{\"x\":33,\"y\":8},{\"x\":34,\"y\":8},{\"x\":35,\"y\":8},{\"x\":36,\"y\":8},{\"x\":37,\"y\":8},{\"x\":38,\"y\":8},{\"x\":1,\"y\":9},{\"x\":2,\"y\":9},{\"x\":3,\"y\":9},{\"x\":12,\"y\":9},{\"x\":15,\"y\":9},{\"x\":33,\"y\":9},{\"x\":34,\"y\":9},{\"x\":35,\"y\":9},{\"x\":36,\"y\":9},{\"x\":37,\"y\":9},{\"x\":38,\"y\":9},{\"x\":1,\"y\":10},{\"x\":2,\"y\":10},{\"x\":3,\"y\":10},{\"x\":4,\"y\":10},{\"x\":5,\"y\":10},{\"x\":14,\"y\":10},{\"x\":32,\"y\":10},{\"x\":33,\"y\":10},{\"x\":35,\"y\":10},{\"x\":36,\"y\":10},{\"x\":37,\"y\":10},{\"x\":38,\"y\":10},{\"x\":1,\"y\":11},{\"x\":2,\"y\":11},{\"x\":3,\"y\":11},{\"x\":4,\"y\":11},{\"x\":5,\"y\":11},{\"x\":33,\"y\":11},{\"x\":36,\"y\":11},{\"x\":37,\"y\":11},{\"x\":38,\"y\":11},{\"x\":1,\"y\":12},{\"x\":2,\"y\":12},{\"x\":37,\"y\":12},{\"x\":38,\"y\":12},{\"x\":1,\"y\":13},{\"x\":2,\"y\":13},{\"x\":37,\"y\":13},{\"x\":38,\"y\":13},{\"x\":1,\"y\":14},{\"x\":2,\"y\":14},{\"x\":35,\"y\":14},{\"x\":36,\"y\":14},{\"x\":37,\"y\":14},{\"x\":38,\"y\":14},{\"x\":1,\"y\":15},{\"x\":2,\"y\":15},{\"x\":3,\"y\":15},{\"x\":31,\"y\":15},{\"x\":35,\"y\":15},{\"x\":36,\"y\":15},{\"x\":37,\"y\":15},{\"x\":38,\"y\":15},{\"x\":1,\"y\":16},{\"x\":2,\"y\":16},{\"x\":30,\"y\":16},{\"x\":31,\"y\":16},{\"x\":32,\"y\":16},{\"x\":34,\"y\":16},{\"x\":35,\"y\":16},{\"x\":36,\"y\":16},{\"x\":37,\"y\":16},{\"x\":38,\"y\":16},{\"x\":1,\"y\":17},{\"x\":2,\"y\":17},{\"x\":30,\"y\":17},{\"x\":31,\"y\":17},{\"x\":32,\"y\":17},{\"x\":33,\"y\":17},{\"x\":34,\"y\":17},{\"x\":35,\"y\":17},{\"x\":36,\"y\":17},{\"x\":37,\"y\":17},{\"x\":38,\"y\":17},{\"x\":1,\"y\":18},{\"x\":2,\"y\":18},{\"x\":30,\"y\":18},{\"x\":31,\"y\":18},{\"x\":32,\"y\":18},{\"x\":33,\"y\":18},{\"x\":34,\"y\":18},{\"x\":35,\"y\":18},{\"x\":36,\"y\":18},{\"x\":37,\"y\":18},{\"x\":38,\"y\":18},{\"x\":1,\"y\":19},{\"x\":2,\"y\":19},{\"x\":3,\"y\":19},{\"x\":30,\"y\":19},{\"x\":31,\"y\":19},{\"x\":32,\"y\":19},{\"x\":33,\"y\":19},{\"x\":34,\"y\":19},{\"x\":35,\"y\":19},{\"x\":36,\"y\":19},{\"x\":37,\"y\":19},{\"x\":38,\"y\":19},{\"x\":1,\"y\":20},{\"x\":32,\"y\":20},{\"x\":33,\"y\":20},{\"x\":34,\"y\":20},{\"x\":35,\"y\":20},{\"x\":36,\"y\":20},{\"x\":37,\"y\":20},{\"x\":38,\"y\":20},{\"x\":1,\"y\":21},{\"x\":2,\"y\":21},{\"x\":35,\"y\":21},{\"x\":36,\"y\":21},{\"x\":37,\"y\":21},{\"x\":38,\"y\":21},{\"x\":1,\"y\":22},{\"x\":2,\"y\":22},{\"x\":34,\"y\":22},{\"x\":35,\"y\":22},{\"x\":36,\"y\":22},{\"x\":37,\"y\":22},{\"x\":38,\"y\":22},{\"x\":1,\"y\":23},{\"x\":2,\"y\":23},{\"x\":3,\"y\":23},{\"x\":36,\"y\":23},{\"x\":37,\"y\":23},{\"x\":38,\"y\":23},{\"x\":1,\"y\":24},{\"x\":2,\"y\":24},{\"x\":35,\"y\":24},{\"x\":36,\"y\":24},{\"x\":37,\"y\":24},{\"x\":38,\"y\":24},{\"x\":1,\"y\":25},{\"x\":2,\"y\":25},{\"x\":36,\"y\":25},{\"x\":37,\"y\":25},{\"x\":38,\"y\":25},{\"x\":1,\"y\":26},{\"x\":2,\"y\":26},{\"x\":37,\"y\":26},{\"x\":38,\"y\":26},{\"x\":1,\"y\":27},{\"x\":2,\"y\":27},{\"x\":38,\"y\":27},{\"x\":1,\"y\":28},{\"x\":2,\"y\":28},{\"x\":37,\"y\":28},{\"x\":38,\"y\":28},{\"x\":1,\"y\":29},{\"x\":2,\"y\":29},{\"x\":3,\"y\":29},{\"x\":36,\"y\":29},{\"x\":37,\"y\":29},{\"x\":38,\"y\":29},{\"x\":1,\"y\":30},{\"x\":2,\"y\":30},{\"x\":3,\"y\":30},{\"x\":36,\"y\":30},{\"x\":37,\"y\":30},{\"x\":38,\"y\":30},{\"x\":1,\"y\":31},{\"x\":2,\"y\":31},{\"x\":3,\"y\":31},{\"x\":4,\"y\":31},{\"x\":29,\"y\":31},{\"x\":36,\"y\":31},{\"x\":37,\"y\":31},{\"x\":38,\"y\":31},{\"x\":1,\"y\":32},{\"x\":2,\"y\":32},{\"x\":28,\"y\":32},{\"x\":35,\"y\":32},{\"x\":36,\"y\":32},{\"x\":37,\"y\":32},{\"x\":38,\"y\":32},{\"x\":1,\"y\":33},{\"x\":2,\"y\":33},{\"x\":3,\"y\":33},{\"x\":28,\"y\":33},{\"x\":29,\"y\":33},{\"x\":36,\"y\":33},{\"x\":37,\"y\":33},{\"x\":38,\"y\":33},{\"x\":1,\"y\":34},{\"x\":2,\"y\":34},{\"x\":23,\"y\":34},{\"x\":24,\"y\":34},{\"x\":28,\"y\":34},{\"x\":29,\"y\":34},{\"x\":35,\"y\":34},{\"x\":36,\"y\":34},{\"x\":37,\"y\":34},{\"x\":38,\"y\":34},{\"x\":1,\"y\":35},{\"x\":2,\"y\":35},{\"x\":23,\"y\":35},{\"x\":24,\"y\":35},{\"x\":28,\"y\":35},{\"x\":29,\"y\":35},{\"x\":30,\"y\":35},{\"x\":31,\"y\":35},{\"x\":32,\"y\":35},{\"x\":35,\"y\":35},{\"x\":36,\"y\":35},{\"x\":37,\"y\":35},{\"x\":38,\"y\":35},{\"x\":1,\"y\":36},{\"x\":8,\"y\":36},{\"x\":9,\"y\":36},{\"x\":10,\"y\":36},{\"x\":11,\"y\":36},{\"x\":12,\"y\":36},{\"x\":13,\"y\":36},{\"x\":14,\"y\":36},{\"x\":22,\"y\":36},{\"x\":23,\"y\":36},{\"x\":24,\"y\":36},{\"x\":28,\"y\":36},{\"x\":29,\"y\":36},{\"x\":30,\"y\":36},{\"x\":31,\"y\":36},{\"x\":32,\"y\":36},{\"x\":33,\"y\":36},{\"x\":34,\"y\":36},{\"x\":35,\"y\":36},{\"x\":36,\"y\":36},{\"x\":37,\"y\":36},{\"x\":38,\"y\":36},{\"x\":1,\"y\":37},{\"x\":2,\"y\":37},{\"x\":3,\"y\":37},{\"x\":4,\"y\":37},{\"x\":5,\"y\":37},{\"x\":6,\"y\":37},{\"x\":7,\"y\":37},{\"x\":9,\"y\":37},{\"x\":10,\"y\":37},{\"x\":11,\"y\":37},{\"x\":12,\"y\":37},{\"x\":13,\"y\":37},{\"x\":14,\"y\":37},{\"x\":15,\"y\":37},{\"x\":16,\"y\":37},{\"x\":19,\"y\":37},{\"x\":20,\"y\":37},{\"x\":21,\"y\":37},{\"x\":22,\"y\":37},{\"x\":23,\"y\":37},{\"x\":24,\"y\":37},{\"x\":25,\"y\":37},{\"x\":28,\"y\":37},{\"x\":29,\"y\":37},{\"x\":30,\"y\":37},{\"x\":31,\"y\":37},{\"x\":32,\"y\":37},{\"x\":33,\"y\":37},{\"x\":34,\"y\":37},{\"x\":35,\"y\":37},{\"x\":36,\"y\":37},{\"x\":37,\"y\":37},{\"x\":38,\"y\":37},{\"x\":1,\"y\":38},{\"x\":2,\"y\":38},{\"x\":3,\"y\":38},{\"x\":4,\"y\":38},{\"x\":5,\"y\":38},{\"x\":6,\"y\":38},{\"x\":8,\"y\":38},{\"x\":9,\"y\":38},{\"x\":10,\"y\":38},{\"x\":11,\"y\":38},{\"x\":12,\"y\":38},{\"x\":13,\"y\":38},{\"x\":14,\"y\":38},{\"x\":15,\"y\":38},{\"x\":16,\"y\":38},{\"x\":17,\"y\":38},{\"x\":18,\"y\":38},{\"x\":19,\"y\":38},{\"x\":20,\"y\":38},{\"x\":21,\"y\":38},{\"x\":22,\"y\":38},{\"x\":23,\"y\":38},{\"x\":24,\"y\":38},{\"x\":25,\"y\":38},{\"x\":28,\"y\":38},{\"x\":29,\"y\":38},{\"x\":30,\"y\":38},{\"x\":31,\"y\":38},{\"x\":32,\"y\":38},{\"x\":33,\"y\":38},{\"x\":34,\"y\":38},{\"x\":35,\"y\":38},{\"x\":36,\"y\":38},{\"x\":37,\"y\":38},{\"x\":38,\"y\":38},{\"x\":4,\"y\":15},{\"x\":4,\"y\":16},{\"x\":5,\"y\":15},{\"x\":4,\"y\":14},{\"x\":5,\"y\":16},{\"x\":12,\"y\":7},{\"x\":26,\"y\":38},{\"x\":27,\"y\":38},{\"x\":27,\"y\":37},{\"x\":11,\"y\":35},{\"x\":12,\"y\":35},{\"x\":7,\"y\":35},{\"x\":8,\"y\":37},{\"x\":7,\"y\":38},{\"x\":5,\"y\":36},{\"x\":6,\"y\":36},{\"x\":2,\"y\":20},{\"x\":3,\"y\":21},{\"x\":3,\"y\":22},{\"x\":4,\"y\":21},{\"x\":4,\"y\":22},{\"x\":4,\"y\":23},{\"x\":3,\"y\":27},{\"x\":4,\"y\":29},{\"x\":5,\"y\":29},{\"x\":4,\"y\":28},{\"x\":4,\"y\":5},{\"x\":3,\"y\":6},{\"x\":4,\"y\":6},{\"x\":0,\"y\":39},{\"x\":1,\"y\":39},{\"x\":2,\"y\":39},{\"x\":3,\"y\":39},{\"x\":4,\"y\":39},{\"x\":5,\"y\":39},{\"x\":6,\"y\":39},{\"x\":7,\"y\":39},{\"x\":8,\"y\":39},{\"x\":9,\"y\":39},{\"x\":2,\"y\":39},{\"x\":3,\"y\":39},{\"x\":4,\"y\":39},{\"x\":5,\"y\":39},{\"x\":6,\"y\":39},{\"x\":7,\"y\":39},{\"x\":1,\"y\":39},{\"x\":0,\"y\":38},{\"x\":0,\"y\":37},{\"x\":0,\"y\":39},{\"x\":9,\"y\":39},{\"x\":11,\"y\":39},{\"x\":12,\"y\":39},{\"x\":10,\"y\":39},{\"x\":8,\"y\":39},{\"x\":13,\"y\":39},{\"x\":14,\"y\":39},{\"x\":15,\"y\":39},{\"x\":16,\"y\":39},{\"x\":17,\"y\":39},{\"x\":18,\"y\":39},{\"x\":19,\"y\":39},{\"x\":20,\"y\":39},{\"x\":21,\"y\":39},{\"x\":22,\"y\":39},{\"x\":23,\"y\":39},{\"x\":24,\"y\":39},{\"x\":25,\"y\":39},{\"x\":26,\"y\":39},{\"x\":27,\"y\":39},{\"x\":28,\"y\":39},{\"x\":29,\"y\":39},{\"x\":30,\"y\":39},{\"x\":31,\"y\":39},{\"x\":32,\"y\":39},{\"x\":33,\"y\":39},{\"x\":34,\"y\":39},{\"x\":35,\"y\":39},{\"x\":36,\"y\":39},{\"x\":37,\"y\":39},{\"x\":38,\"y\":39},{\"x\":39,\"y\":39},{\"x\":39,\"y\":38},{\"x\":39,\"y\":37},{\"x\":39,\"y\":36},{\"x\":39,\"y\":35},{\"x\":39,\"y\":34},{\"x\":39,\"y\":33},{\"x\":39,\"y\":32},{\"x\":39,\"y\":31},{\"x\":39,\"y\":30},{\"x\":39,\"y\":29},{\"x\":39,\"y\":28},{\"x\":39,\"y\":27},{\"x\":39,\"y\":25},{\"x\":39,\"y\":26},{\"x\":39,\"y\":24},{\"x\":39,\"y\":23},{\"x\":39,\"y\":21},{\"x\":39,\"y\":22},{\"x\":39,\"y\":20},{\"x\":39,\"y\":19},{\"x\":39,\"y\":18},{\"x\":39,\"y\":17},{\"x\":39,\"y\":16},{\"x\":39,\"y\":15},{\"x\":39,\"y\":14},{\"x\":39,\"y\":13},{\"x\":39,\"y\":12},{\"x\":39,\"y\":11},{\"x\":39,\"y\":10},{\"x\":39,\"y\":9},{\"x\":39,\"y\":8},{\"x\":39,\"y\":7},{\"x\":39,\"y\":6},{\"x\":39,\"y\":5},{\"x\":39,\"y\":4},{\"x\":39,\"y\":3},{\"x\":39,\"y\":2},{\"x\":39,\"y\":1},{\"x\":39,\"y\":0},{\"x\":38,\"y\":2},{\"x\":38,\"y\":1},{\"x\":38,\"y\":0},{\"x\":36,\"y\":0},{\"x\":35,\"y\":0},{\"x\":34,\"y\":0},{\"x\":33,\"y\":0},{\"x\":32,\"y\":0},{\"x\":31,\"y\":0},{\"x\":29,\"y\":0},{\"x\":37,\"y\":0},{\"x\":30,\"y\":0},{\"x\":28,\"y\":0},{\"x\":27,\"y\":0},{\"x\":26,\"y\":0},{\"x\":25,\"y\":0},{\"x\":24,\"y\":0},{\"x\":22,\"y\":0},{\"x\":19,\"y\":0},{\"x\":18,\"y\":0},{\"x\":20,\"y\":0},{\"x\":21,\"y\":0},{\"x\":23,\"y\":0},{\"x\":15,\"y\":0},{\"x\":17,\"y\":0},{\"x\":16,\"y\":0},{\"x\":14,\"y\":0},{\"x\":13,\"y\":0},{\"x\":12,\"y\":0},{\"x\":11,\"y\":0},{\"x\":10,\"y\":0},{\"x\":9,\"y\":0},{\"x\":8,\"y\":0},{\"x\":7,\"y\":0},{\"x\":6,\"y\":0},{\"x\":5,\"y\":0},{\"x\":4,\"y\":0},{\"x\":3,\"y\":0},{\"x\":2,\"y\":0},{\"x\":1,\"y\":0},{\"x\":0,\"y\":0},{\"x\":0,\"y\":1},{\"x\":0,\"y\":2},{\"x\":0,\"y\":4},{\"x\":0,\"y\":5},{\"x\":0,\"y\":3},{\"x\":0,\"y\":7},{\"x\":0,\"y\":8},{\"x\":0,\"y\":6},{\"x\":0,\"y\":9},{\"x\":0,\"y\":10},{\"x\":0,\"y\":12},{\"x\":0,\"y\":14},{\"x\":0,\"y\":11},{\"x\":0,\"y\":16},{\"x\":0,\"y\":18},{\"x\":0,\"y\":22},{\"x\":0,\"y\":24},{\"x\":0,\"y\":26},{\"x\":0,\"y\":30},{\"x\":0,\"y\":32},{\"x\":0,\"y\":33},{\"x\":0,\"y\":34},{\"x\":0,\"y\":36},{\"x\":0,\"y\":29},{\"x\":0,\"y\":28},{\"x\":0,\"y\":27},{\"x\":0,\"y\":25},{\"x\":0,\"y\":19},{\"x\":0,\"y\":20},{\"x\":0,\"y\":17},{\"x\":0,\"y\":35},{\"x\":0,\"y\":31},{\"x\":0,\"y\":23},{\"x\":0,\"y\":21},{\"x\":0,\"y\":15},{\"x\":0,\"y\":13}],\"tipoCeldas\":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,4,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,4,4,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,4,4,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]}",
                            "{\"numEstado\":1,\"nombreEstado\":\"Estado 1\",\"tileCapital\":19,\"coordsTiles\":[{\"x\":11,\"y\":10},{\"x\":12,\"y\":11},{\"x\":12,\"y\":12},{\"x\":13,\"y\":13},{\"x\":14,\"y\":15},{\"x\":15,\"y\":15},{\"x\":15,\"y\":14},{\"x\":15,\"y\":13},{\"x\":14,\"y\":12},{\"x\":14,\"y\":11},{\"x\":13,\"y\":10},{\"x\":12,\"y\":10},{\"x\":11,\"y\":12},{\"x\":12,\"y\":13},{\"x\":12,\"y\":14},{\"x\":13,\"y\":15},{\"x\":13,\"y\":16},{\"x\":14,\"y\":14},{\"x\":14,\"y\":13},{\"x\":13,\"y\":12},{\"x\":13,\"y\":11},{\"x\":14,\"y\":16},{\"x\":13,\"y\":14},{\"x\":12,\"y\":15},{\"x\":11,\"y\":16},{\"x\":12,\"y\":16},{\"x\":13,\"y\":17},{\"x\":12,\"y\":17},{\"x\":10,\"y\":16},{\"x\":11,\"y\":15},{\"x\":10,\"y\":15},{\"x\":10,\"y\":14},{\"x\":10,\"y\":17},{\"x\":11,\"y\":19},{\"x\":11,\"y\":18},{\"x\":9,\"y\":16},{\"x\":11,\"y\":17},{\"x\":10,\"y\":18},{\"x\":9,\"y\":18},{\"x\":10,\"y\":19},{\"x\":11,\"y\":20},{\"x\":12,\"y\":19},{\"x\":8,\"y\":20},{\"x\":9,\"y\":19},{\"x\":8,\"y\":19},{\"x\":8,\"y\":18},{\"x\":13,\"y\":9},{\"x\":14,\"y\":9},{\"x\":13,\"y\":8}],\"tipoCeldas\":[3,3,3,3,2,2,2,3,3,3,3,3,2,2,3,3,3,2,3,3,3,3,2,2,3,3,3,3,3,3,2,3,2,3,2,2,2,2,2,3,2,2,2,3,3,2,4,4,4]}",
                            "{\"numEstado\":2,\"nombreEstado\":\"Estado 2\",\"tileCapital\":17,\"coordsTiles\":[{\"x\":15,\"y\":11},{\"x\":16,\"y\":11},{\"x\":17,\"y\":11},{\"x\":17,\"y\":10},{\"x\":18,\"y\":11},{\"x\":18,\"y\":10},{\"x\":18,\"y\":9},{\"x\":17,\"y\":8},{\"x\":18,\"y\":7},{\"x\":17,\"y\":6},{\"x\":16,\"y\":6},{\"x\":17,\"y\":7},{\"x\":16,\"y\":7},{\"x\":16,\"y\":9},{\"x\":15,\"y\":10},{\"x\":16,\"y\":10},{\"x\":17,\"y\":9},{\"x\":16,\"y\":8},{\"x\":19,\"y\":8},{\"x\":20,\"y\":9},{\"x\":20,\"y\":7},{\"x\":19,\"y\":9},{\"x\":19,\"y\":10},{\"x\":21,\"y\":9},{\"x\":20,\"y\":8},{\"x\":21,\"y\":7},{\"x\":22,\"y\":7},{\"x\":22,\"y\":8},{\"x\":21,\"y\":8},{\"x\":18,\"y\":8},{\"x\":19,\"y\":7},{\"x\":20,\"y\":11},{\"x\":19,\"y\":11},{\"x\":20,\"y\":13},{\"x\":19,\"y\":12},{\"x\":20,\"y\":6},{\"x\":20,\"y\":5},{\"x\":20,\"y\":4},{\"x\":19,\"y\":6},{\"x\":18,\"y\":6},{\"x\":16,\"y\":5},{\"x\":16,\"y\":4},{\"x\":15,\"y\":4},{\"x\":15,\"y\":6}],\"tipoCeldas\":[3,2,2,3,2,2,3,3,3,4,4,3,3,3,3,3,3,3,2,3,3,2,2,3,3,4,4,3,3,2,2,2,2,2,2,4,4,4,4,4,4,4,4,4]}",
                            "{\"numEstado\":3,\"nombreEstado\":\"Estado 3\",\"tileCapital\":3,\"coordsTiles\":[{\"x\":15,\"y\":12},{\"x\":16,\"y\":12},{\"x\":17,\"y\":13},{\"x\":17,\"y\":14},{\"x\":16,\"y\":14},{\"x\":16,\"y\":13},{\"x\":18,\"y\":13},{\"x\":19,\"y\":13},{\"x\":18,\"y\":12},{\"x\":17,\"y\":12},{\"x\":18,\"y\":14},{\"x\":18,\"y\":15},{\"x\":18,\"y\":17},{\"x\":19,\"y\":17},{\"x\":19,\"y\":16},{\"x\":20,\"y\":15},{\"x\":20,\"y\":14},{\"x\":19,\"y\":14},{\"x\":19,\"y\":15},{\"x\":18,\"y\":16},{\"x\":17,\"y\":16},{\"x\":17,\"y\":17},{\"x\":20,\"y\":17},{\"x\":20,\"y\":16},{\"x\":21,\"y\":15},{\"x\":19,\"y\":18},{\"x\":18,\"y\":18},{\"x\":16,\"y\":17},{\"x\":16,\"y\":16},{\"x\":16,\"y\":15},{\"x\":17,\"y\":15},{\"x\":21,\"y\":13},{\"x\":21,\"y\":12},{\"x\":22,\"y\":13},{\"x\":22,\"y\":14},{\"x\":21,\"y\":14},{\"x\":22,\"y\":15},{\"x\":20,\"y\":12},{\"x\":23,\"y\":15},{\"x\":24,\"y\":15},{\"x\":24,\"y\":14},{\"x\":25,\"y\":13},{\"x\":24,\"y\":13},{\"x\":23,\"y\":14},{\"x\":17,\"y\":18},{\"x\":17,\"y\":19},{\"x\":16,\"y\":19},{\"x\":16,\"y\":18},{\"x\":15,\"y\":19},{\"x\":14,\"y\":19},{\"x\":13,\"y\":18},{\"x\":14,\"y\":18},{\"x\":15,\"y\":18},{\"x\":15,\"y\":17},{\"x\":14,\"y\":17},{\"x\":15,\"y\":16},{\"x\":23,\"y\":16},{\"x\":22,\"y\":16},{\"x\":21,\"y\":16},{\"x\":21,\"y\":17}],\"tipoCeldas\":[2,2,3,3,3,2,3,3,2,3,3,3,2,3,3,2,2,3,3,3,2,2,3,3,3,3,2,3,3,3,3,2,3,2,2,2,3,2,3,3,3,3,2,3,2,2,3,3,3,3,3,3,3,3,3,3,3,3,3,3]}",
                            "{\"numEstado\":4,\"nombreEstado\":\"Estado 4\",\"tileCapital\":7,\"coordsTiles\":[{\"x\":20,\"y\":10},{\"x\":21,\"y\":10},{\"x\":21,\"y\":11},{\"x\":22,\"y\":11},{\"x\":22,\"y\":10},{\"x\":23,\"y\":9},{\"x\":22,\"y\":9},{\"x\":23,\"y\":10},{\"x\":24,\"y\":11},{\"x\":23,\"y\":11},{\"x\":23,\"y\":12},{\"x\":23,\"y\":13},{\"x\":22,\"y\":12},{\"x\":24,\"y\":10},{\"x\":25,\"y\":10},{\"x\":25,\"y\":9},{\"x\":25,\"y\":8},{\"x\":24,\"y\":8},{\"x\":24,\"y\":9},{\"x\":26,\"y\":9},{\"x\":23,\"y\":8},{\"x\":24,\"y\":7},{\"x\":23,\"y\":6},{\"x\":23,\"y\":7}],\"tipoCeldas\":[2,2,2,2,3,3,3,3,3,3,2,2,2,3,3,3,4,4,3,3,3,4,4,4]}",
                            "{\"numEstado\":5,\"nombreEstado\":\"Estado 5\",\"tileCapital\":11,\"coordsTiles\":[{\"x\":24,\"y\":12},{\"x\":25,\"y\":11},{\"x\":26,\"y\":11},{\"x\":26,\"y\":12},{\"x\":25,\"y\":12},{\"x\":27,\"y\":11},{\"x\":26,\"y\":10},{\"x\":27,\"y\":10},{\"x\":28,\"y\":9},{\"x\":28,\"y\":11},{\"x\":27,\"y\":12},{\"x\":28,\"y\":13},{\"x\":27,\"y\":14},{\"x\":28,\"y\":14},{\"x\":27,\"y\":13},{\"x\":26,\"y\":14},{\"x\":25,\"y\":14},{\"x\":26,\"y\":13},{\"x\":29,\"y\":14},{\"x\":29,\"y\":13},{\"x\":29,\"y\":12},{\"x\":28,\"y\":12},{\"x\":29,\"y\":15},{\"x\":28,\"y\":15},{\"x\":27,\"y\":15},{\"x\":28,\"y\":16},{\"x\":29,\"y\":16},{\"x\":30,\"y\":15}],\"tipoCeldas\":[2,3,3,2,3,2,2,3,3,2,3,3,3,3,3,3,3,3,3,3,3,3,3,2,2,3,3,3]}",
                            "{\"numEstado\":6,\"nombreEstado\":\"Estado 6\",\"tileCapital\":15,\"coordsTiles\":[{\"x\":25,\"y\":16},{\"x\":25,\"y\":15},{\"x\":26,\"y\":15},{\"x\":26,\"y\":17},{\"x\":26,\"y\":16},{\"x\":27,\"y\":17},{\"x\":28,\"y\":17},{\"x\":26,\"y\":18},{\"x\":27,\"y\":16},{\"x\":27,\"y\":18},{\"x\":28,\"y\":18},{\"x\":29,\"y\":17},{\"x\":29,\"y\":18},{\"x\":29,\"y\":19},{\"x\":28,\"y\":19},{\"x\":27,\"y\":19},{\"x\":26,\"y\":19},{\"x\":25,\"y\":19},{\"x\":25,\"y\":18},{\"x\":25,\"y\":17},{\"x\":24,\"y\":17},{\"x\":24,\"y\":16},{\"x\":23,\"y\":17},{\"x\":22,\"y\":18},{\"x\":22,\"y\":17},{\"x\":22,\"y\":19},{\"x\":23,\"y\":19},{\"x\":24,\"y\":19},{\"x\":24,\"y\":18},{\"x\":23,\"y\":18},{\"x\":23,\"y\":20},{\"x\":24,\"y\":20},{\"x\":24,\"y\":21},{\"x\":23,\"y\":21},{\"x\":23,\"y\":22},{\"x\":22,\"y\":22},{\"x\":22,\"y\":23},{\"x\":21,\"y\":24},{\"x\":23,\"y\":23},{\"x\":24,\"y\":22},{\"x\":25,\"y\":21},{\"x\":27,\"y\":21},{\"x\":27,\"y\":20},{\"x\":28,\"y\":20},{\"x\":29,\"y\":20},{\"x\":31,\"y\":20},{\"x\":30,\"y\":20},{\"x\":29,\"y\":21},{\"x\":29,\"y\":22},{\"x\":30,\"y\":22},{\"x\":30,\"y\":23},{\"x\":31,\"y\":23},{\"x\":30,\"y\":21},{\"x\":28,\"y\":21},{\"x\":25,\"y\":20},{\"x\":26,\"y\":20}],\"tipoCeldas\":[3,3,3,3,2,2,2,3,2,3,2,3,3,3,3,3,3,3,2,3,3,3,3,2,3,2,2,2,2,3,2,3,3,2,3,2,3,3,3,3,2,2,3,2,3,3,3,3,3,3,3,3,3,2,3,3]}",
                            "{\"numEstado\":7,\"nombreEstado\":\"Estado 7\",\"tileCapital\":21,\"coordsTiles\":[{\"x\":27,\"y\":9},{\"x\":26,\"y\":8},{\"x\":27,\"y\":7},{\"x\":28,\"y\":7},{\"x\":27,\"y\":8},{\"x\":28,\"y\":6},{\"x\":29,\"y\":6},{\"x\":29,\"y\":7},{\"x\":28,\"y\":8},{\"x\":29,\"y\":9},{\"x\":29,\"y\":10},{\"x\":29,\"y\":11},{\"x\":28,\"y\":10},{\"x\":30,\"y\":11},{\"x\":30,\"y\":9},{\"x\":30,\"y\":8},{\"x\":31,\"y\":9},{\"x\":29,\"y\":8},{\"x\":30,\"y\":7},{\"x\":30,\"y\":6},{\"x\":31,\"y\":6},{\"x\":31,\"y\":7},{\"x\":31,\"y\":8},{\"x\":32,\"y\":7},{\"x\":32,\"y\":6},{\"x\":32,\"y\":8},{\"x\":32,\"y\":9},{\"x\":33,\"y\":7},{\"x\":33,\"y\":5},{\"x\":33,\"y\":4},{\"x\":32,\"y\":5}],\"tipoCeldas\":[3,4,4,4,3,4,4,3,3,2,2,3,2,3,2,3,3,2,3,3,3,3,3,3,3,4,4,4,4,4,4]}",
                            "{\"numEstado\":8,\"nombreEstado\":\"Estado 8\",\"tileCapital\":21,\"coordsTiles\":[{\"x\":20,\"y\":18},{\"x\":21,\"y\":18},{\"x\":20,\"y\":19},{\"x\":21,\"y\":19},{\"x\":20,\"y\":20},{\"x\":21,\"y\":20},{\"x\":22,\"y\":20},{\"x\":22,\"y\":21},{\"x\":21,\"y\":21},{\"x\":21,\"y\":22},{\"x\":20,\"y\":22},{\"x\":21,\"y\":23},{\"x\":20,\"y\":23},{\"x\":19,\"y\":22},{\"x\":20,\"y\":21},{\"x\":19,\"y\":20},{\"x\":19,\"y\":19},{\"x\":18,\"y\":19},{\"x\":18,\"y\":20},{\"x\":19,\"y\":21},{\"x\":18,\"y\":22},{\"x\":19,\"y\":23},{\"x\":18,\"y\":23},{\"x\":17,\"y\":22},{\"x\":18,\"y\":21},{\"x\":17,\"y\":20},{\"x\":17,\"y\":21},{\"x\":16,\"y\":22},{\"x\":17,\"y\":23},{\"x\":17,\"y\":24},{\"x\":18,\"y\":24},{\"x\":19,\"y\":24},{\"x\":16,\"y\":21},{\"x\":20,\"y\":24},{\"x\":20,\"y\":25},{\"x\":21,\"y\":25},{\"x\":21,\"y\":26},{\"x\":22,\"y\":25},{\"x\":23,\"y\":25},{\"x\":22,\"y\":26},{\"x\":20,\"y\":26},{\"x\":19,\"y\":26},{\"x\":19,\"y\":25},{\"x\":18,\"y\":25},{\"x\":18,\"y\":26},{\"x\":19,\"y\":27},{\"x\":20,\"y\":27},{\"x\":20,\"y\":28},{\"x\":21,\"y\":28},{\"x\":22,\"y\":27},{\"x\":21,\"y\":27}],\"tipoCeldas\":[3,3,3,2,3,2,2,2,2,2,2,3,3,3,2,3,3,2,2,2,3,3,3,3,3,3,3,3,3,2,3,3,3,3,3,3,3,3,3,3,2,2,3,2,3,3,3,2,2,2,2]}",
                            "{\"numEstado\":9,\"nombreEstado\":\"Estado 9\",\"tileCapital\":19,\"coordsTiles\":[{\"x\":12,\"y\":18},{\"x\":13,\"y\":19},{\"x\":12,\"y\":20},{\"x\":13,\"y\":20},{\"x\":14,\"y\":20},{\"x\":15,\"y\":20},{\"x\":16,\"y\":20},{\"x\":14,\"y\":21},{\"x\":14,\"y\":22},{\"x\":13,\"y\":22},{\"x\":12,\"y\":22},{\"x\":12,\"y\":21},{\"x\":13,\"y\":21},{\"x\":11,\"y\":22},{\"x\":10,\"y\":22},{\"x\":10,\"y\":23},{\"x\":11,\"y\":23},{\"x\":11,\"y\":21},{\"x\":10,\"y\":21},{\"x\":9,\"y\":22},{\"x\":9,\"y\":23},{\"x\":9,\"y\":21},{\"x\":8,\"y\":22},{\"x\":8,\"y\":23},{\"x\":8,\"y\":24},{\"x\":9,\"y\":25},{\"x\":8,\"y\":25},{\"x\":7,\"y\":24},{\"x\":7,\"y\":23},{\"x\":7,\"y\":22},{\"x\":7,\"y\":21},{\"x\":8,\"y\":21},{\"x\":9,\"y\":20},{\"x\":10,\"y\":20},{\"x\":9,\"y\":24},{\"x\":15,\"y\":22},{\"x\":15,\"y\":21},{\"x\":14,\"y\":23},{\"x\":13,\"y\":23}],\"tipoCeldas\":[2,2,2,2,3,3,2,2,2,2,3,2,3,2,3,3,3,2,3,3,3,3,3,3,3,2,3,3,3,3,2,2,2,3,2,3,3,2,3]}",
                            "{\"numEstado\":10,\"nombreEstado\":\"Estado 10\",\"tileCapital\":16,\"coordsTiles\":[{\"x\":16,\"y\":23},{\"x\":16,\"y\":24},{\"x\":16,\"y\":25},{\"x\":15,\"y\":24},{\"x\":14,\"y\":24},{\"x\":15,\"y\":23},{\"x\":13,\"y\":24},{\"x\":12,\"y\":24},{\"x\":12,\"y\":23},{\"x\":11,\"y\":24},{\"x\":10,\"y\":24},{\"x\":11,\"y\":25},{\"x\":11,\"y\":26},{\"x\":12,\"y\":26},{\"x\":13,\"y\":25},{\"x\":14,\"y\":25},{\"x\":14,\"y\":26},{\"x\":15,\"y\":26},{\"x\":15,\"y\":25},{\"x\":12,\"y\":25},{\"x\":13,\"y\":26},{\"x\":15,\"y\":27},{\"x\":16,\"y\":27},{\"x\":17,\"y\":25},{\"x\":16,\"y\":26},{\"x\":17,\"y\":26},{\"x\":17,\"y\":27},{\"x\":15,\"y\":28},{\"x\":16,\"y\":29},{\"x\":15,\"y\":30},{\"x\":15,\"y\":29},{\"x\":14,\"y\":28},{\"x\":13,\"y\":28},{\"x\":14,\"y\":29},{\"x\":14,\"y\":30},{\"x\":14,\"y\":27},{\"x\":13,\"y\":30},{\"x\":14,\"y\":31},{\"x\":13,\"y\":32},{\"x\":13,\"y\":29},{\"x\":13,\"y\":27}],\"tipoCeldas\":[3,2,2,2,3,2,3,3,3,2,2,2,3,2,2,3,3,3,3,2,3,3,3,2,2,3,3,2,2,3,2,3,2,3,3,3,3,3,2,3,3]}",
                            "{\"numEstado\":11,\"nombreEstado\":\"Estado 11\",\"tileCapital\":27,\"coordsTiles\":[{\"x\":16,\"y\":28},{\"x\":17,\"y\":28},{\"x\":18,\"y\":28},{\"x\":18,\"y\":27},{\"x\":19,\"y\":28},{\"x\":19,\"y\":29},{\"x\":20,\"y\":29},{\"x\":19,\"y\":30},{\"x\":20,\"y\":31},{\"x\":21,\"y\":31},{\"x\":20,\"y\":32},{\"x\":21,\"y\":33},{\"x\":22,\"y\":33},{\"x\":21,\"y\":32},{\"x\":21,\"y\":30},{\"x\":20,\"y\":30},{\"x\":21,\"y\":29},{\"x\":22,\"y\":31},{\"x\":22,\"y\":30},{\"x\":22,\"y\":29},{\"x\":23,\"y\":29},{\"x\":23,\"y\":31},{\"x\":22,\"y\":32},{\"x\":23,\"y\":33},{\"x\":22,\"y\":34},{\"x\":21,\"y\":34},{\"x\":19,\"y\":31},{\"x\":18,\"y\":30},{\"x\":18,\"y\":29},{\"x\":18,\"y\":31},{\"x\":18,\"y\":32},{\"x\":17,\"y\":32},{\"x\":18,\"y\":33},{\"x\":18,\"y\":34},{\"x\":17,\"y\":34},{\"x\":17,\"y\":33},{\"x\":17,\"y\":31},{\"x\":17,\"y\":30},{\"x\":17,\"y\":29},{\"x\":16,\"y\":30}],\"tipoCeldas\":[3,3,3,3,3,3,3,3,3,2,2,3,3,2,3,3,3,2,3,3,3,3,3,3,3,3,3,3,3,3,2,2,2,3,3,2,3,3,3,3]}",
                            "{\"numEstado\":12,\"nombreEstado\":\"Estado 12\",\"tileCapital\":22,\"coordsTiles\":[{\"x\":22,\"y\":24},{\"x\":23,\"y\":24},{\"x\":24,\"y\":24},{\"x\":24,\"y\":23},{\"x\":25,\"y\":23},{\"x\":26,\"y\":23},{\"x\":25,\"y\":22},{\"x\":26,\"y\":24},{\"x\":26,\"y\":25},{\"x\":25,\"y\":25},{\"x\":24,\"y\":26},{\"x\":24,\"y\":25},{\"x\":24,\"y\":27},{\"x\":23,\"y\":26},{\"x\":23,\"y\":27},{\"x\":23,\"y\":28},{\"x\":22,\"y\":28},{\"x\":24,\"y\":28},{\"x\":24,\"y\":29},{\"x\":25,\"y\":29},{\"x\":25,\"y\":28},{\"x\":25,\"y\":27},{\"x\":25,\"y\":26},{\"x\":25,\"y\":24},{\"x\":26,\"y\":26},{\"x\":26,\"y\":27},{\"x\":27,\"y\":27},{\"x\":27,\"y\":26},{\"x\":27,\"y\":25},{\"x\":27,\"y\":23},{\"x\":27,\"y\":24},{\"x\":26,\"y\":21},{\"x\":26,\"y\":22},{\"x\":28,\"y\":25},{\"x\":28,\"y\":27},{\"x\":28,\"y\":28},{\"x\":27,\"y\":28},{\"x\":29,\"y\":27},{\"x\":28,\"y\":26}],\"tipoCeldas\":[3,3,2,3,2,2,3,2,3,3,3,3,2,2,2,2,2,2,3,3,3,3,3,2,3,3,3,2,2,2,2,2,2,2,2,3,3,3,2]}",
                            "{\"numEstado\":13,\"nombreEstado\":\"Estado 13\",\"tileCapital\":27,\"coordsTiles\":[{\"x\":27,\"y\":22},{\"x\":28,\"y\":22},{\"x\":28,\"y\":23},{\"x\":29,\"y\":23},{\"x\":28,\"y\":24},{\"x\":29,\"y\":25},{\"x\":29,\"y\":24},{\"x\":30,\"y\":24},{\"x\":31,\"y\":25},{\"x\":30,\"y\":26},{\"x\":29,\"y\":26},{\"x\":30,\"y\":25},{\"x\":31,\"y\":26},{\"x\":32,\"y\":25},{\"x\":31,\"y\":24},{\"x\":32,\"y\":23},{\"x\":32,\"y\":22},{\"x\":33,\"y\":21},{\"x\":32,\"y\":21},{\"x\":31,\"y\":22},{\"x\":31,\"y\":21},{\"x\":32,\"y\":24},{\"x\":33,\"y\":23},{\"x\":33,\"y\":22},{\"x\":34,\"y\":21},{\"x\":34,\"y\":23},{\"x\":33,\"y\":24},{\"x\":33,\"y\":25},{\"x\":32,\"y\":26},{\"x\":33,\"y\":27},{\"x\":34,\"y\":27},{\"x\":34,\"y\":26},{\"x\":34,\"y\":25},{\"x\":35,\"y\":23},{\"x\":33,\"y\":26},{\"x\":34,\"y\":24},{\"x\":35,\"y\":25},{\"x\":35,\"y\":26},{\"x\":35,\"y\":27},{\"x\":36,\"y\":26},{\"x\":36,\"y\":27},{\"x\":35,\"y\":28},{\"x\":37,\"y\":27},{\"x\":36,\"y\":28},{\"x\":31,\"y\":27},{\"x\":32,\"y\":27},{\"x\":32,\"y\":28},{\"x\":31,\"y\":29},{\"x\":32,\"y\":29},{\"x\":31,\"y\":30},{\"x\":32,\"y\":30},{\"x\":32,\"y\":31},{\"x\":33,\"y\":31},{\"x\":31,\"y\":28}],\"tipoCeldas\":[2,3,2,2,3,2,2,3,3,3,3,2,3,3,3,3,3,3,3,3,3,3,3,4,4,4,3,3,3,3,3,3,3,4,3,4,4,4,3,4,3,4,4,4,3,3,3,3,3,3,3,3,3,3]}",
                            "{\"numEstado\":14,\"nombreEstado\":\"Estado 14\",\"tileCapital\":13,\"coordsTiles\":[{\"x\":30,\"y\":27},{\"x\":29,\"y\":28},{\"x\":30,\"y\":28},{\"x\":30,\"y\":29},{\"x\":29,\"y\":30},{\"x\":30,\"y\":30},{\"x\":30,\"y\":31},{\"x\":29,\"y\":32},{\"x\":30,\"y\":32},{\"x\":31,\"y\":31},{\"x\":31,\"y\":32},{\"x\":32,\"y\":32},{\"x\":32,\"y\":33},{\"x\":33,\"y\":33},{\"x\":32,\"y\":34},{\"x\":31,\"y\":34},{\"x\":31,\"y\":33},{\"x\":30,\"y\":33},{\"x\":30,\"y\":34},{\"x\":33,\"y\":32},{\"x\":34,\"y\":32},{\"x\":35,\"y\":33},{\"x\":34,\"y\":33},{\"x\":34,\"y\":31},{\"x\":35,\"y\":31},{\"x\":34,\"y\":30},{\"x\":34,\"y\":29},{\"x\":33,\"y\":29},{\"x\":33,\"y\":30},{\"x\":34,\"y\":28},{\"x\":33,\"y\":28},{\"x\":35,\"y\":29},{\"x\":35,\"y\":30},{\"x\":34,\"y\":34},{\"x\":33,\"y\":34},{\"x\":34,\"y\":35},{\"x\":33,\"y\":35}],\"tipoCeldas\":[3,3,3,3,3,3,3,4,3,3,3,3,3,3,4,4,3,4,4,3,4,4,3,3,4,3,3,3,3,3,3,4,4,4,3,4,4]}",
                            "{\"numEstado\":15,\"nombreEstado\":\"Estado 15\",\"tileCapital\":4,\"coordsTiles\":[{\"x\":26,\"y\":28},{\"x\":26,\"y\":29},{\"x\":26,\"y\":30},{\"x\":25,\"y\":30},{\"x\":26,\"y\":31},{\"x\":27,\"y\":29},{\"x\":28,\"y\":29},{\"x\":29,\"y\":29},{\"x\":28,\"y\":30},{\"x\":28,\"y\":31},{\"x\":27,\"y\":31},{\"x\":27,\"y\":30},{\"x\":25,\"y\":31},{\"x\":24,\"y\":30},{\"x\":23,\"y\":30},{\"x\":24,\"y\":31},{\"x\":23,\"y\":32},{\"x\":24,\"y\":32},{\"x\":24,\"y\":33},{\"x\":25,\"y\":33},{\"x\":25,\"y\":32},{\"x\":26,\"y\":32},{\"x\":26,\"y\":33},{\"x\":27,\"y\":32},{\"x\":27,\"y\":33},{\"x\":27,\"y\":34},{\"x\":26,\"y\":34},{\"x\":25,\"y\":35},{\"x\":26,\"y\":35},{\"x\":25,\"y\":36},{\"x\":26,\"y\":36},{\"x\":25,\"y\":34},{\"x\":27,\"y\":35},{\"x\":27,\"y\":36},{\"x\":26,\"y\":37}],\"tipoCeldas\":[3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,4,3,4,3,4,3,4,3,4,4]}",
                            "{\"numEstado\":16,\"nombreEstado\":\"Estado 16\",\"tileCapital\":9,\"coordsTiles\":[{\"x\":16,\"y\":31},{\"x\":15,\"y\":31},{\"x\":14,\"y\":32},{\"x\":15,\"y\":32},{\"x\":16,\"y\":32},{\"x\":16,\"y\":33},{\"x\":15,\"y\":34},{\"x\":15,\"y\":33},{\"x\":14,\"y\":33},{\"x\":14,\"y\":34},{\"x\":15,\"y\":35},{\"x\":16,\"y\":34},{\"x\":16,\"y\":35},{\"x\":17,\"y\":35},{\"x\":17,\"y\":36},{\"x\":18,\"y\":36},{\"x\":19,\"y\":35},{\"x\":18,\"y\":35},{\"x\":19,\"y\":34},{\"x\":19,\"y\":33},{\"x\":19,\"y\":32},{\"x\":20,\"y\":33},{\"x\":20,\"y\":34},{\"x\":20,\"y\":35},{\"x\":21,\"y\":35},{\"x\":20,\"y\":36},{\"x\":19,\"y\":36},{\"x\":21,\"y\":36},{\"x\":22,\"y\":35},{\"x\":18,\"y\":37},{\"x\":17,\"y\":37},{\"x\":16,\"y\":36},{\"x\":15,\"y\":36},{\"x\":14,\"y\":35},{\"x\":13,\"y\":34},{\"x\":12,\"y\":34},{\"x\":11,\"y\":34},{\"x\":11,\"y\":33},{\"x\":12,\"y\":33},{\"x\":13,\"y\":35},{\"x\":13,\"y\":33}],\"tipoCeldas\":[3,3,3,2,3,2,3,3,3,3,3,3,3,3,3,4,3,3,3,2,2,2,3,3,3,4,4,4,4,4,4,4,4,3,3,4,4,2,2,4,3]}",
                            "{\"numEstado\":17,\"nombreEstado\":\"Estado 17\",\"tileCapital\":9,\"coordsTiles\":[{\"x\":12,\"y\":32},{\"x\":12,\"y\":31},{\"x\":13,\"y\":31},{\"x\":11,\"y\":32},{\"x\":10,\"y\":32},{\"x\":11,\"y\":31},{\"x\":11,\"y\":30},{\"x\":12,\"y\":29},{\"x\":12,\"y\":30},{\"x\":11,\"y\":29},{\"x\":11,\"y\":28},{\"x\":12,\"y\":28},{\"x\":11,\"y\":27},{\"x\":12,\"y\":27},{\"x\":10,\"y\":27},{\"x\":10,\"y\":26},{\"x\":10,\"y\":25},{\"x\":9,\"y\":26},{\"x\":10,\"y\":28},{\"x\":10,\"y\":30},{\"x\":10,\"y\":29},{\"x\":9,\"y\":28},{\"x\":8,\"y\":28},{\"x\":9,\"y\":27},{\"x\":9,\"y\":30},{\"x\":10,\"y\":31},{\"x\":9,\"y\":32},{\"x\":10,\"y\":33},{\"x\":10,\"y\":34},{\"x\":10,\"y\":35},{\"x\":9,\"y\":35},{\"x\":8,\"y\":34},{\"x\":9,\"y\":34},{\"x\":9,\"y\":33},{\"x\":8,\"y\":32},{\"x\":8,\"y\":33},{\"x\":7,\"y\":32},{\"x\":8,\"y\":31},{\"x\":9,\"y\":31},{\"x\":9,\"y\":29}],\"tipoCeldas\":[2,3,3,2,2,2,3,3,3,3,3,3,3,2,2,2,2,2,3,3,3,3,2,2,3,2,2,2,4,4,4,4,4,3,2,3,3,3,2,2]}",
                            "{\"numEstado\":18,\"nombreEstado\":\"Estado 18\",\"tileCapital\":5,\"coordsTiles\":[{\"x\":8,\"y\":35},{\"x\":6,\"y\":34},{\"x\":7,\"y\":33},{\"x\":7,\"y\":34},{\"x\":5,\"y\":34},{\"x\":5,\"y\":33},{\"x\":6,\"y\":33},{\"x\":6,\"y\":35},{\"x\":5,\"y\":35},{\"x\":4,\"y\":34},{\"x\":4,\"y\":33},{\"x\":3,\"y\":32},{\"x\":5,\"y\":32},{\"x\":4,\"y\":32},{\"x\":7,\"y\":36},{\"x\":4,\"y\":36},{\"x\":3,\"y\":36},{\"x\":2,\"y\":36},{\"x\":4,\"y\":35},{\"x\":3,\"y\":35},{\"x\":3,\"y\":34}],\"tipoCeldas\":[4,4,3,4,3,3,3,4,4,3,3,4,3,3,4,4,4,4,4,4,4]}",
                            "{\"numEstado\":19,\"nombreEstado\":\"Estado 19\",\"tileCapital\":14,\"coordsTiles\":[{\"x\":6,\"y\":32},{\"x\":6,\"y\":31},{\"x\":6,\"y\":30},{\"x\":5,\"y\":30},{\"x\":4,\"y\":27},{\"x\":4,\"y\":26},{\"x\":4,\"y\":30},{\"x\":5,\"y\":31},{\"x\":3,\"y\":28},{\"x\":3,\"y\":25},{\"x\":3,\"y\":24},{\"x\":4,\"y\":25},{\"x\":5,\"y\":25},{\"x\":5,\"y\":27},{\"x\":6,\"y\":29},{\"x\":5,\"y\":28},{\"x\":5,\"y\":26},{\"x\":6,\"y\":27},{\"x\":6,\"y\":28},{\"x\":7,\"y\":30},{\"x\":7,\"y\":31},{\"x\":7,\"y\":28},{\"x\":7,\"y\":29},{\"x\":8,\"y\":29},{\"x\":8,\"y\":30},{\"x\":3,\"y\":26}],\"tipoCeldas\":[3,3,4,4,4,3,4,3,4,4,4,3,3,3,3,4,3,3,3,3,3,3,3,3,3,4]}",
                            "{\"numEstado\":20,\"nombreEstado\":\"Estado 20\",\"tileCapital\":28,\"coordsTiles\":[{\"x\":8,\"y\":27},{\"x\":7,\"y\":27},{\"x\":6,\"y\":26},{\"x\":7,\"y\":26},{\"x\":8,\"y\":26},{\"x\":7,\"y\":25},{\"x\":6,\"y\":25},{\"x\":5,\"y\":24},{\"x\":6,\"y\":23},{\"x\":5,\"y\":23},{\"x\":6,\"y\":24},{\"x\":4,\"y\":24},{\"x\":6,\"y\":22},{\"x\":5,\"y\":20},{\"x\":6,\"y\":20},{\"x\":6,\"y\":21},{\"x\":7,\"y\":20},{\"x\":3,\"y\":20},{\"x\":4,\"y\":20},{\"x\":4,\"y\":19},{\"x\":5,\"y\":19},{\"x\":6,\"y\":19},{\"x\":5,\"y\":18},{\"x\":6,\"y\":18},{\"x\":7,\"y\":19},{\"x\":5,\"y\":17},{\"x\":3,\"y\":16},{\"x\":6,\"y\":17},{\"x\":4,\"y\":18},{\"x\":3,\"y\":18},{\"x\":3,\"y\":17},{\"x\":4,\"y\":17},{\"x\":5,\"y\":22},{\"x\":5,\"y\":21}],\"tipoCeldas\":[3,3,3,3,2,3,3,3,3,3,3,3,3,3,2,3,2,4,3,3,3,3,3,3,3,3,4,4,3,3,4,3,3,3]}",
                            "{\"numEstado\":21,\"nombreEstado\":\"Estado 21\",\"tileCapital\":24,\"coordsTiles\":[{\"x\":7,\"y\":18},{\"x\":7,\"y\":17},{\"x\":7,\"y\":16},{\"x\":8,\"y\":16},{\"x\":9,\"y\":17},{\"x\":8,\"y\":17},{\"x\":8,\"y\":15},{\"x\":7,\"y\":15},{\"x\":6,\"y\":16},{\"x\":6,\"y\":15},{\"x\":3,\"y\":14},{\"x\":3,\"y\":13},{\"x\":4,\"y\":13},{\"x\":5,\"y\":13},{\"x\":3,\"y\":12},{\"x\":4,\"y\":12},{\"x\":5,\"y\":12},{\"x\":6,\"y\":13},{\"x\":6,\"y\":14},{\"x\":5,\"y\":14},{\"x\":6,\"y\":12},{\"x\":8,\"y\":13},{\"x\":8,\"y\":14},{\"x\":7,\"y\":13},{\"x\":7,\"y\":14},{\"x\":9,\"y\":15}],\"tipoCeldas\":[3,3,3,3,2,2,3,3,4,4,4,4,4,4,4,4,3,3,3,4,3,3,3,3,3,2]}",
                            "{\"numEstado\":22,\"nombreEstado\":\"Estado 22\",\"tileCapital\":39,\"coordsTiles\":[{\"x\":11,\"y\":14},{\"x\":11,\"y\":13},{\"x\":10,\"y\":13},{\"x\":9,\"y\":13},{\"x\":9,\"y\":14},{\"x\":8,\"y\":12},{\"x\":7,\"y\":12},{\"x\":8,\"y\":11},{\"x\":9,\"y\":11},{\"x\":9,\"y\":12},{\"x\":10,\"y\":12},{\"x\":10,\"y\":11},{\"x\":11,\"y\":11},{\"x\":9,\"y\":10},{\"x\":10,\"y\":10},{\"x\":10,\"y\":9},{\"x\":11,\"y\":9},{\"x\":8,\"y\":8},{\"x\":9,\"y\":9},{\"x\":8,\"y\":10},{\"x\":7,\"y\":10},{\"x\":7,\"y\":9},{\"x\":8,\"y\":9},{\"x\":7,\"y\":11},{\"x\":6,\"y\":11},{\"x\":6,\"y\":10},{\"x\":7,\"y\":8},{\"x\":8,\"y\":7},{\"x\":7,\"y\":6},{\"x\":7,\"y\":7},{\"x\":6,\"y\":8},{\"x\":6,\"y\":9},{\"x\":5,\"y\":8},{\"x\":6,\"y\":7},{\"x\":8,\"y\":5},{\"x\":7,\"y\":4},{\"x\":8,\"y\":4},{\"x\":9,\"y\":5},{\"x\":9,\"y\":6},{\"x\":9,\"y\":7},{\"x\":10,\"y\":7},{\"x\":9,\"y\":8},{\"x\":8,\"y\":6},{\"x\":10,\"y\":6},{\"x\":11,\"y\":7},{\"x\":10,\"y\":8},{\"x\":10,\"y\":5},{\"x\":11,\"y\":5},{\"x\":12,\"y\":5},{\"x\":11,\"y\":4},{\"x\":10,\"y\":4},{\"x\":11,\"y\":6},{\"x\":5,\"y\":7},{\"x\":5,\"y\":6},{\"x\":6,\"y\":6},{\"x\":6,\"y\":5},{\"x\":5,\"y\":5},{\"x\":4,\"y\":8},{\"x\":5,\"y\":9},{\"x\":4,\"y\":9},{\"x\":4,\"y\":7}],\"tipoCeldas\":[2,2,2,2,3,2,2,2,2,2,3,2,2,2,2,3,3,3,3,2,3,3,3,3,3,3,3,3,4,4,3,3,3,4,4,4,4,4,3,3,3,3,3,3,4,3,4,4,4,4,4,4,4,4,4,4,4,4,3,4,4]}",
                            "{\"numEstado\":23,\"nombreEstado\":\"Estado 23\",\"tileCapital\":16,\"coordsTiles\":[{\"x\":30,\"y\":10},{\"x\":31,\"y\":10},{\"x\":31,\"y\":11},{\"x\":32,\"y\":11},{\"x\":32,\"y\":12},{\"x\":31,\"y\":12},{\"x\":31,\"y\":13},{\"x\":32,\"y\":13},{\"x\":33,\"y\":13},{\"x\":33,\"y\":12},{\"x\":34,\"y\":13},{\"x\":33,\"y\":14},{\"x\":30,\"y\":12},{\"x\":30,\"y\":13},{\"x\":34,\"y\":14},{\"x\":35,\"y\":13},{\"x\":34,\"y\":12},{\"x\":35,\"y\":11},{\"x\":35,\"y\":12},{\"x\":36,\"y\":12},{\"x\":36,\"y\":13},{\"x\":34,\"y\":11},{\"x\":34,\"y\":10},{\"x\":30,\"y\":14},{\"x\":31,\"y\":14},{\"x\":32,\"y\":14},{\"x\":33,\"y\":15},{\"x\":32,\"y\":15},{\"x\":33,\"y\":16},{\"x\":34,\"y\":15}],\"tipoCeldas\":[3,3,3,3,2,2,2,2,2,3,3,3,2,3,4,3,3,3,3,4,4,3,4,3,2,3,3,3,4,4]}",};
        string json;
        Estado estadoAux;
        elMapaReino.GetComponent<MapaReino>().listaEstados.Clear();
        elMapaReino.GetComponent<MapaReino>().listaEstados = new List<Estado>();
//        Debug.Log("count: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Count+" Capacity: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Capacity);
        for(int i=0; i<estados.Length; i++){
            elMapaReino.GetComponent<MapaReino>().listaEstados.Add(null);
        }
        //Debug.Log("Después del for. count: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Count+" Capacity: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Capacity);

        foreach(string archivo in estados){
            estadoAux = ((Estado) ScriptableObject.CreateInstance(typeof(Estado)));
            json = archivo;
            JsonUtility.FromJsonOverwrite(json,estadoAux); //elMapaReino.GetComponent<MapaReino>().listaEstados[contador]
 //           Debug.Log("Quiero insertar el estado: "+estadoAux.nombreEstado+" en posicion: "+estadoAux.numEstado+" y la lista tiene "+elMapaReino.GetComponent<MapaReino>().listaEstados.Count+" y la capacity: "+elMapaReino.GetComponent<MapaReino>().listaEstados.Capacity);
            elMapaReino.GetComponent<MapaReino>().listaEstados[estadoAux.numEstado] = estadoAux;
//            Debug.Log("He leído el mapa: "+elMapaReino.GetComponent<MapaReino>().listaEstados[estadoAux.numEstado].nombreEstado);
        }

//        Debug.Log("Archivos cargados, a pintar fronteras toca!");
        elMapaReino.GetComponent<MapaReino>().ActualizarEstadoDeTiles();
        PintarFronteras();
    }

    //Máximo estados + el neutral que será el 0
    public void AnyadirEstado(){
        if( elMapaReino.GetComponent<MapaReino>().listaEstados.Count < 24){
            numEstadoAnterior = numEstadoActual;
            numEstadoActual = elMapaReino.GetComponent<MapaReino>().listaEstados.Count();
            
            Estado estadoTemp = (Estado) ScriptableObject.CreateInstance(typeof(Estado));
            Debug.Log("Añadiendo estado. numero: "+numEstadoActual);
            estadoTemp.SetEstado(numEstadoActual,"Estado "+numEstadoActual,-1,new List<Vector2Int>(),new List<int>());

            elMapaReino.GetComponent<MapaReino>().listaEstados.Add(estadoTemp);
            
            //GameObject laCapital = Instantiate(elMapaReino.GetComponent<MapaReino>().simboloCapital, new Vector3(100,100,100), new Quaternion());
            //capitalesEstados.Add(laCapital);
            
            elCanvasEdicion.transform.GetChild(0).GetComponent<TMP_Text>().text = "Estado Actual: "+ numEstadoActual;
            
            Debug.Log("Estado añadido. Ahora tengo:"+elMapaReino.GetComponent<MapaReino>().listaEstados.Count+" estados. EstadoActual: "+numEstadoActual);
            estadoTemp.MostrarEstado();
        }
    }
    
    public void FinalizarTurno(){
        InicializarMovimientoEjercito(ejercitosP1);
        InicializarMovimientoEjercito(ejercitosP2);
        Debug.Log("Fin del turno del player: "+ currentPlayer);
        if(currentPlayer == 1){
            currentPlayer = 2;
        }else{
            currentPlayer = 1;
        }
        elCanvasUI.transform.GetChild(0).GetComponent<TMP_Text>().text = "Turno: Player"+ currentPlayer;
    }

    //Actulizamos los vecinos de cada estado
    public void ActualizarEstadosVecinos(){
        foreach(GameObject elObjeto in elMapaReino.GetComponent<MapaReino>().elGridMapa){
            HexTile tile = elObjeto.GetComponent<HexTile>();
            foreach( HexTile vecino in tile.neighbours){
                if( vecino.numEstado != tile.numEstado ){
                    //Debug.Log("En la tile: "+tile.coordenada+" del estado: "+tile.numEstado+" Añadiendo el vecino: "+vecino.numEstado);
//Para que no de error                    elMapaReino.GetComponent<MapaReino>().listaEstados[tile.numEstado].SetEstadoVecino(vecino.numEstado,true);
                }
            }
        }
    }

    //Nuevo algoritmo para realizarMovimientoJuego
/*


Si no estado seleccionado entonces
	lacapitalOrigen  = cogerla.
	Si laCapitalOrigen == ocupada && es del jugador actual(viendo el player de la tropa (Ejercito.cs) entonces
		EjercitoSeleccionado =cogerlo
		Si no ha movido el EjercitoSeleccionado entonces
			SeleccionarCapital(numestadoActual)
			numestadoAnterior=numestadoActual   //Esto tal vez sobre?
			estadoseleccionado = true
		si no
			Capital del player. No fer res
	else
		Capital del enemigo. Con o sin tropa. No fer res
	fi
si no  //Ya hay un estado seleccionado de antes
	estadoSeleccionado = false
	DesactivarCapitales
	laCapitalOrigen (ya la tenía del click anterior
	laCapitalDestino = cogerla 
	Si está ocupada por colega entonces
		Moverse y unirse
		...y toda la mandanga que haya que hacer
	si no Si ocupada por enemigo entonces
		Moverse y atacar
		...y toda la mandanga que haya que hacer
	Si no //Está libre
		ocuparla 
		...y toda la mandanga que haya que hacer
	fi
fi
*/

//
//    *** ***   *********   ***     ***
//  *** ** ***  ***   ***    ***   ***
//  ***    ***  ***   ***     *** ***
//  ***    ***  *********       ***
//
    private void RealizarMovimientoJuego(HexTile tile){
        //Modo juego
        if (moviendoTropa)  //Una tropa está en movimiento, ignoramos la selección
            return;
        //Si el estado está sin conquistar y estamos moviendo no hacer nada
        if(tile.numEstado ==0 ){ //El mar y terreno fuera del mapa no lo seleccionamos
            elMapaReino.GetComponent<MapaReino>().DesactivarCapitales();
            estadoSeleccionado = false;
            return;
        }
        Capital laCapitalOrigen;
        Capital laCapitalDestino;
//poner laCapitalOrigen y laCapitalDestino para tenerlas y quitar la origen
//como ocupada y ocupar la de destino
//La desocupación se hará al clickar el destino y la ocupación también (ahora, luego habrá que moverlo a
//donde el ejército acabe el movimiento, tal vez en Tick())
//Revisar el final de turno cuando se muevan todos los ejércitos
//También mirar los combates cómo se harían
        if( ! estadoSeleccionado ){  //No hay estado seleccionado
            laCapitalDestino = elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>();
            switch (laCapitalDestino.GetPropietario()){
                case 0: 
                    Debug.Log("El estado no es de nadie. No hago nada.");
                    break;
                case 1:
                    if(currentPlayer == 1){
                        ejercitoSeleccionado = laCapitalDestino.GetEjercitoOcupante();
                        if( laCapitalDestino.GetEjercitoOcupante() != null && ejercitoSeleccionado.GetComponent<Ejercito>().haMovido == false ){  //El currentPlayer tiene una tropa aquí que no ha movido
                            Debug.Log("Capital ocupada por currentPlayer con tropa no movida antes. Seleccionando estado y ejército.");
                            elMapaReino.GetComponent<MapaReino>().SeleccionarCapital(numEstadoActual);
                            numEstadoAnterior = numEstadoActual;
                            estadoSeleccionado = true;
                        }else{
                            Debug.Log("La capital es del currentPlayer, pero no tiene tropa o si la tiene ya ha movido. No hago nada.");
                        }
                    }else{
                        Debug.Log("Capital del enemigo. Con o sin tropa no hago nada.");
                    }
                    break;
                case 2:
                    if(currentPlayer == 2){
                        ejercitoSeleccionado = laCapitalDestino.GetEjercitoOcupante();
                        if( laCapitalDestino.GetEjercitoOcupante() != null && ejercitoSeleccionado.GetComponent<Ejercito>().haMovido == false ){  //El currentPlayer tiene una tropa aquí
                            Debug.Log("Capital ocupada con tropa. Seleccionando estado y ejército.");
                            elMapaReino.GetComponent<MapaReino>().SeleccionarCapital(numEstadoActual);
                            numEstadoAnterior = numEstadoActual;
                            estadoSeleccionado = true;
                        }else{
                            Debug.Log("La capital es del currentPlayer, pero no tiene tropa. No hago nada.");
                        }
                    }else{
                        Debug.Log("Capital del enemigo. Con o sin tropa no hago nada.");
                    }
                    break;
                default:
                    Debug.Log("ERROR: el propietario no es ni 0 ni 1 ni 2. Revisar.");
                    break;
            }
        }else{ //Hay un estado seleccionado con anterioridad, moveremos tropa para conquitar o atacar según el caso
//Hay que hacer que cuando el ejército se mueve de un estado se marque el estado como no ocupado (DesOcuparCapital). Que no sé cómo ahora mismo        
            estadoSeleccionado=false;
            elMapaReino.GetComponent<MapaReino>().DesactivarCapitales();
            laCapitalOrigen = elMapaReino.GetComponent<MapaReino>().capitalesEstados[numEstadoAnterior].GetComponent<Capital>();
            laCapitalDestino = elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>();
/*Para que no de error
            if( ! elMapaReino.GetComponent<MapaReino>().listaEstados[tile.numEstado].EsVecino(numEstadoAnterior) ){
                Debug.Log("El estado anterior: "+numEstadoAnterior+" no es vecino de: "+numEstadoActual+" No hacemos nada.");
                return;
            }
*/
            switch (laCapitalDestino.GetPropietario()){
                case 0: 
                        Debug.Log("El estado no es de nadie. Muevo el la tropa a: "+tile.name+" y desocupo la origen.");
                        ocupandoEstado = true;
                        if(currentPlayer == 1){
                            MoverEjercito(ejercitoSeleccionado);
                            laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                            //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP1);
                        }else{
                            MoverEjercito(ejercitoSeleccionado);
                            laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                            //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP2);
                        }
                        moviendoTropa = true;
                    break;
                case 1: //Ocupar/unir o atacar
                        if(currentPlayer == 1){
                            Debug.Log("Aquí siempre se mueve, si no hay diferencia si en destino hay o no tropa dejarlo con sólo el movimiento. Si es ataque se gestionará al llegar?(**PENDIENTE**)");
                            if( laCapitalDestino.GetEjercitoOcupante() != null){  //El currentPlayer tiene una tropa aquí, unirlas
                                uniendoTropas = true;
                                MoverEjercito(ejercitoSeleccionado);
                                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP1);
                                Debug.Log("Capital ocupada con tropa del currentPlayer. Desocupo la origen. Me muevo para unirlas. Falta implementar la unión");
                            }else{  //Está vacía, ocupar
                                ocupandoEstado = true;
                                MoverEjercito(ejercitoSeleccionado);
                                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP1);
                                Debug.Log("Capital es del currentPlayer, pero no tiene tropa. Desocupo la origen. Me muevo para ocuparla. Y desocupo la anterior");
                            }
                        }else{  //"Capital del enemigo. Conquistar o atacar"
                            if( laCapitalDestino.GetEjercitoOcupante() != null){  //El enemigo tiene una tropa aquí, ataco
                                atacando = true;
                                Debug.Log("ATACAR!!!!! Capital ocupada con tropa del enemigo. Me muevo para atacar. Desocupo la origen Falta implementar el ataque. No habría que conquistarla hasta después del choque o que la capital no tenga bandera cuando haya una unidad en ella.");
                                MoverEjercito(ejercitoSeleccionado);
                                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP2);
                            }else{
                                ocupandoEstado = true;
                                MoverEjercito(ejercitoSeleccionado);
                                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP2);
                                ocupandoEstado = true;
                                Debug.Log("Capital es del enemigo, pero no tiene tropa. Me muevo para ocuparla. Desocupo la origen.");
                            }
                        }
                    break;
                case 2:
                        if(currentPlayer == 2){
                            Debug.Log("Aquí siempre se mueve, si no hay diferencia si en destino hay o no tropa dejarlo con sólo el movimiento. Si es ataque se gestionará al llegar?(**PENDIENTE**)");
                            ocupandoEstado = true;
                            if( laCapitalDestino.GetEjercitoOcupante() != null){  //El currentPlayer tiene una tropa aquí, unirlas
                                MoverEjercito(ejercitoSeleccionado);
                                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP2);
                                Debug.Log("Capital ocupada con tropa del currentPlayer. Me muevo para unirlas. Desocupo la origen. Falta implementar la unión");
                            }else{  //Está vacía, ocupar
                                MoverEjercito(ejercitoSeleccionado);
                                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP2);
                                Debug.Log("Capital es del currentPlayer, pero no tiene tropa. Me muevo para ocuparla. desocupo la origen");
                            }
                        }else{  //"Capital del enemigo. Conquistar o atacar"
                            if( laCapitalDestino.GetEjercitoOcupante() != null){  //El enemigo tiene una tropa aquí, ataco
                                atacando = true;
                                Debug.Log("ATACAR!!!!! Capital ocupada con tropa del enemigo. Me muevo para atacar. Falta implementar el ataque. No habría que conquistarla hasta después del choque o que la capital no tenga bandera cuando haya una unidad en ella.");
                                MoverEjercito(ejercitoSeleccionado);
                                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP1);
                            }else{
                                ocupandoEstado = true;
                                Debug.Log("Capital es del enemigo, pero no tiene tropa. Me muevo para ocuparla.");
                                MoverEjercito(ejercitoSeleccionado);
                                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                //laCapitalDestino.GetComponent<Capital>().OcuparCapital(currentPlayer,indiceEjercitoP1);
                            }
                        }
                    break;
                default:
                    Debug.Log("Error: el propietario no es ni 0 ni 1 ni 2. Revisar.");
                    break;
            }
        }      
    }

    //Le doy la ruta al ejército para que se mueva
    private void MoverEjercito(GameObject elEjercito){
        
        Vector2Int coordsCapital = elMapaReino.GetComponent<MapaReino>().listaEstados[numEstadoActual].GetCoordsCapital();
        int posCapital = coordsCapital.y*elMapaReino.GetComponent<MapaReino>().gridSize.y+coordsCapital.x;
        HexTile tileDestino = elMapaReino.GetComponent<MapaReino>().elGridMapa[posCapital].GetComponent<HexTile>();
        path = PathFinder.FindPath(elEjercito.GetComponent<Ejercito>().currentTile,tileDestino);
        elEjercito.transform.LookAt(tileDestino.transform);
        elEjercito.GetComponent<Ejercito>().haMovido = true;
        path.Reverse();
        elEjercito.GetComponent<Ejercito>().SetPath(path);
        
    }

    //Inicializa las tropas del ejército para poder moverlas
    private void InicializarMovimientoEjercito(List<GameObject> ejercitosActuales){
        for(int i = 0; i<ejercitosActuales.Count; i++){
            ejercitosActuales[i].GetComponent<Ejercito>().haMovido = false;
        }
    }

    //Resuelve el combate entre el ejército del player 1 y el del 2 automáticamente
    //Cómo: Para cada ejército 
    //  unidades infantería x 1+unidades caballería x 2 + unidades catapulta x 3 + Random 100
    // Ganará el que saque la cifra más alta y devolvemos el número de dicho player
    private int ResolverCombate(GameObject ejP1, GameObject ejP2){
        Ejercito EjercitoP1 = ejP1.GetComponent<Ejercito>();
        Ejercito EjercitoP2 = ejP2.GetComponent<Ejercito>();
        int resP1 = EjercitoP1.numInfanteria + EjercitoP1.numCaballeria * 2 + EjercitoP1.numCatapulta * 3 + UnityEngine.Random.Range(0,100);
        int resP2 = EjercitoP2.numInfanteria + EjercitoP2.numCaballeria * 2 + EjercitoP2.numCatapulta * 3 + UnityEngine.Random.Range(0,100);
        Debug.Log("Res1: "+resP1+" Res2: "+resP2);
        if ( resP1 > resP2){
            return 1;
        }
        if ( resP1 < resP2 ){
            return 2;
        }
        //Si les ha dado el mismo resultado lo echamos a suertes
        return UnityEngine.Random.Range(1,3);
    }

    private void mostrarInfoJuego(){
        string mensaje = "currentPlayer: "+currentPlayer+" - indiceEjercitoSeleccionado: NO sé.";//+ejercitoSeleccionado.GetComponent<Ejercito>().indiceEjercito;
        mensaje +="\n moviendoTropa: "+moviendoTropa;
        mensaje +="\n atacando: "+atacando;
        mensaje +="\n uniendoTropas: "+ uniendoTropas+"\nocupandoEstado: "+ocupandoEstado;
        mensaje +="\n numEjercitosP1: "+numEjercitosP1+"\nnumEjercitosP2: "+numEjercitosP2;
        mensaje +="\n stop:"+stop+"\neditandoMapa: "+editandoMapa+"\nnumEstadoActual: "+numEstadoActual;
        mensaje +="\n numEstadoAnterior: "+numEstadoAnterior+"\nestadoSeleccionado: "+estadoSeleccionado;
        mensaje +="\nCapital  -  propietario  -  indexEjercito\n";
        MapaReino elMapa = elMapaReino.GetComponent<MapaReino>();
        Capital laCapital;
        for( int i=1 ; i<elMapa.capitalesEstados.Count; i++){
            laCapital = elMapa.capitalesEstados[i].GetComponent<Capital>();
            string elIndiceEjercito = "vacío.";
            if( laCapital.ejercitoOcupante != null)
                elIndiceEjercito = "?";//laCapital.ejercitoOcupante.GetComponent<Ejercito>().indiceEjercito.ToString();
            mensaje += "     "+ i+"      -       "+ laCapital.propietario +"       -       "+elIndiceEjercito+"\n";
        }
        elCanvasUI.transform.GetChild(2).GetComponent<TMP_Text>().text = "Info Juego:\n"+mensaje;
        //Debug.Log(mensaje);
    }
}
