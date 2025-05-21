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
using UnityEngine.Tilemaps;
//using UnityEngine.Rendering.VirtualTexturing;



/*
 * Para controlar los eventos de los hexágonos.
 * Resaltarlos cuando se pase por encima de ellos, seleccionar cuando se 
 * clicke, colocar al jugador, pathfinding, ...
*/


/*TODO:
*/


public class TileManager : MonoBehaviour
{
    //public TileManager instance;
    //public GameObject player;
    //public GameObject[] enemies; //Enemigos a clonar
    public bool mostrarInfoDebug = false;
    public GameObject highlight;
    public GameObject selector;
    private List<HexTile> path;
    private Dictionary<Vector3Int, HexTile> tiles;
    public GameObject unaFrontera, unaPlaya; //Para clonar en las fronteras de las celdas

    private bool stop = false; //Para el tick del movimiento
    public static TileManager instance;
   
    //Para gestión de los turnos
    public GameObject modeloEjercito;
    public Material[] materialesEjercito;
    public int currentPlayer = 1;
    public GameObject ejercitoSeleccionado = null;
    private int celdasRestantes = -1;  //-1=no hay tropa en movimiento  valor=número de casillas hasta el destino antes->índice de la tropa en movimiento del jugador actual
    public float velocidadTropa = 5.0f;
    private bool atacando = false;
    private bool luchando = false; //Se está realizando un combate
    private float tiempoLucha = 2.5f; //Tiempo que durará la lucha
    private bool uniendoTropas = false;
    private bool ocupandoEstado = false;
    public int numTropaNueva = -1;  //-1 no estoy colocando. Otro valor-> el índice de la tropa en la lista de ejércitos
    private bool comprobandoFinTurno = false;

    //Info partida
    public GameObject elMapaReino;
    public Jugador[] jugadores = new Jugador[3];  //0 no se usa, 1 player1, 2 player2
    public int numEjercitosP1=2;
    public int numEjercitosP2 = 2;

    //Para editar el mapa y crear los estados
    private bool editandoMapa = false;
    public int numEstadoActual=0;
    public int numEstadoAnterior=0;  //Para saber qué estado había seleccionado antes
    public bool estadoSeleccionado = false;
    public bool modoTurnos = true; //true=Combates automáticos false=Combates reales
    public bool oponenteCPU = false;
//    public bool combateEnCurso = false;
    public bool combateActivo = false; //false=estamos con el mapa true=Estamos en el campo de batalla(para desactivar el tick)
    public string vencedor = "";

    public Camera camaraMapa,camaraBatalla;
    public GameObject elCampoBatallaManager;
    public int contadorTurnos = 1;  //Llevar la cuenta de los turnos
    //private int tipoCeldaActual;
    public GameObject elCanvasEdicion,elCanvasUI_Mapa,elCanvasUI_Batalla, elCanvasPausa, elCanvasFinPartida, elCanvasOpciones;
    public TMP_Text mensajeFinPartida;
    public LineRenderer elLineRenderer;
    public GameObject[] decoradosCeldas;  //Para decorar las celdas 0=árbol 1=Montaña
    private int estadoResaltado = -1;

    public CurtainAnimator laCortinilla; //Cortinilla para la transición entre escenas.

    //Para la AI
    public AI_Turnos_SistemaReglas la_AI_Turnos;
    public AI_CombateReal la_AI_Real;

    private float deltaTimeParaFPS = 0.0f;
    //Sonidos
    public SoundManager elSoundManager;
    //public AudioClip clickBoton;    
    public bool tutorialActivo = false; //Para mostrar el tutorial y enseñar cómo se juega
    private int posTutorial = 0; //Para el tutorial, para saber en qué parte del tutorial estamos
    private string[]  textosTutorial = 
        {"Welcome to Hegemonic Wars! This tutorial will guide you through the basics of the game. \nClick to continue.",
        "Here is the map of the game. You can see your available troops by hovering the mouse cursor over their territory. \nClick to continue.",
        "If you click in one available territory you select that troop. Then you can click on a neighbour territory. You can only move to the adjacent territories that are shown after clicking. \nClick to continue.",
        "If the destination territory is not occupied, it will become yours if it isn't already. If it's occupied, you will have to fight for it. \nClick to continue.",
        "In the autocombat mode, the troops will fight automatically. The one with the most units will have more probability to win. \nClick to continue.",
        "When you get enough gold, you get one new troop to place in one of your territories. \nClick to continue.",
        "The player that destroys all the enemy troops wins! \nClick to continue.",
        "In the manual combat mode, you will fight in a battlefield moving your troops and using the terrain to your advantage. \nClick to continue.",
        "In the manual combat mode, you will fight in a battlefield moving your troops and using the terrain to your advantage. \nClick to continue.",
        "Player 1 keys: Move (WASD) Attack (J) Deffence (K) Next unit (Spacebar).\n Player 2 keys: Move: Arrow keys Attack: Numpad 5 Defence: Numpad 6 Next Unit: Numpad 0 \nClick to continue.",
        "Each unit has a different attack and move speed. Beware! Catapult can kill your own troops",
        "Defeat all the enemy forces to win the battle and conquer the territory. \nClick to continue.",
        "Enjoy the game! \nClick to continue.",
        "Enjoy the game! \nClick to continue."
        };
    public bool grabandoTrailer = false; //Para grabar el trailer para controlar los eventos del mismo
    
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

        
    }
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        StartCoroutine(Tick());

        Time.timeScale = 1;

        laCortinilla.PlayOpenCurtainAnimation(false);

        CargarMapaJuego();


        /***/
        jugadores[1] = new Jugador();
        jugadores[2] = new Jugador();

        //Creamos los ejércitos en sus casillas
        HexTile tile;
        jugadores[1].ejercitos = new List<GameObject>();
        for (int i = 0; i < numEjercitosP1; i++)
        {
            jugadores[1].ejercitos.Add(Instantiate(modeloEjercito, new Vector3(0, 0, 0), new Quaternion()));
            jugadores[1].ejercitos[i].GetComponentInChildren<SkinnedMeshRenderer>().material = materialesEjercito[1];

            HexTile[] lasTiles = tiles.Values.ToArray();
            tile = lasTiles[UnityEngine.Random.Range(0, lasTiles.Length)];
            jugadores[1].ejercitos[i].GetComponent<Ejercito>().currentTile = tile;
            //            jugadores[1].ejercitos[i].GetComponent<Ejercito>().indiceEjercito = i;
            jugadores[1].ejercitos[i].GetComponent<Ejercito>().numPlayer = 1;
        }
        jugadores[2].ejercitos = new List<GameObject>();
        for (int i = 0; i < numEjercitosP2; i++)
        {
            //Lo creamos
            jugadores[2].ejercitos.Add(Instantiate(modeloEjercito, new Vector3(0, 0, 0), new Quaternion()));
            jugadores[2].ejercitos[i].GetComponentInChildren<SkinnedMeshRenderer>().material = materialesEjercito[2];

            HexTile[] lasTiles = tiles.Values.ToArray();
            tile = lasTiles[UnityEngine.Random.Range(0, lasTiles.Length)];
            jugadores[2].ejercitos[i].GetComponent<Ejercito>().currentTile = tile;
            //            jugadores[2].ejercitos[i].GetComponent<Ejercito>().indiceEjercito = i;
            jugadores[2].ejercitos[i].GetComponent<Ejercito>().numPlayer = 2;
        }



        /***/
        if (PlayerPrefs.GetInt("tutorialActivo") == 1)
            tutorialActivo = true;
        else
            tutorialActivo = false;
        Vector2Int auxVector2;
        //HexTile tile;
        int posIni1;  // para ocupar siempre el mismo estado luego poner -->UnityEngine.Random.Range(1,12);
        int posIni2;  // como arriba o poner lo que interese para el juego UnityEngine.Random.Range(14,21);
        if (tutorialActivo)
        {
            posIni1 = 9;
            posIni2 = 3;
        }
        else
        {
            posIni1 = UnityEngine.Random.Range(1,11);  //Para debug poner 9
            posIni2 = UnityEngine.Random.Range(14,21);  //Para debug poner 3
        }
        for (int i = 0; i < jugadores[1].ejercitos.Count; i++)
        {
            auxVector2 = elMapaReino.GetComponent<MapaReino>().listaEstados[posIni1].GetCoordsCapital();
            tile = elMapaReino.GetComponent<MapaReino>().elGridMapa[auxVector2.y * elMapaReino.GetComponent<MapaReino>().gridSize.y + auxVector2.x].GetComponent<HexTile>();
            jugadores[1].ejercitos[i].transform.position = tile.transform.position + new Vector3(0f, 0f, 0f);
            jugadores[1].ejercitos[i].transform.LookAt(new Vector3(0, 0, -500));  //Mira siempre al sur
            jugadores[1].ejercitos[i].GetComponent<Ejercito>().currentTile = tile;
            elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().OcuparCapital(jugadores[1].ejercitos[i]);
            if (tutorialActivo)
                posIni1 = 10;
            else
                posIni1++;
        }
        //Pongo la siguiente capital ya ocupada sin ejército
        elMapaReino.GetComponent<MapaReino>().capitalesEstados[posIni1 + 1].GetComponent<Capital>().SetPropietario(1);

        for (int i = 0; i < jugadores[2].ejercitos.Count; i++)
        {
            auxVector2 = elMapaReino.GetComponent<MapaReino>().listaEstados[posIni2].GetCoordsCapital();
            tile = elMapaReino.GetComponent<MapaReino>().elGridMapa[auxVector2.y * elMapaReino.GetComponent<MapaReino>().gridSize.y + auxVector2.x].GetComponent<HexTile>();
            jugadores[2].ejercitos[i].transform.position = tile.transform.position + new Vector3(0, 0f, 0);
            jugadores[2].ejercitos[i].transform.LookAt(new Vector3(0, 0, -500));  //Mira siempre al sur
            jugadores[2].ejercitos[i].GetComponent<Ejercito>().currentTile = tile;
            elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().OcuparCapital(jugadores[2].ejercitos[i]);
            if (tutorialActivo)
                posIni2 = 6;
            else
                posIni2++;
        }
        //Pongo la siguiente capital ya ocupada sin ejército
        elMapaReino.GetComponent<MapaReino>().capitalesEstados[posIni2 + 1].GetComponent<Capital>().SetPropietario(2);

        //player.transform.position = player.GetComponent<Ejercito>().currentTile.transform.position + new Vector3(0,1f,0);

        PintarFronteras();
        ActualizarEstadosVecinos();
        elMapaReino.GetComponent<MapaReino>().ColocarCapitales();

        numEstadoAnterior = 0;
        numEstadoActual = 0;

        elCanvasEdicion.transform.GetChild(0).GetComponent<TMP_Text>().text = "Estado Actual: " + numEstadoActual;
        //elCanvasUI_Mapa.transform.GetChild(0).GetComponent<TMP_Text>().text = "Turno: Player"+currentPlayer;
        elCanvasUI_Mapa.GetComponent<CanvasUI_MapaController>().ResaltarTurnoPlayer(currentPlayer);
        //elCanvasUI_Mapa.transform.GetChild(5).GetComponent<TMP_Text>().text = "Oro Player 1: "+jugadores[1].cantidadOro+"\nOro Player 2: "+jugadores[2].cantidadOro;
        elCanvasUI_Mapa.GetComponent<CanvasUI_MapaController>().EstablecerOroPlayers(jugadores[1].cantidadOro, jugadores[2].cantidadOro);
        int porTurnos = PlayerPrefs.GetInt("modoTurnos");
        //porTurnos=1; //Para forzar el si es porturnos==1 o no porturnos==2
        if (porTurnos == 1)
        {
            modoTurnos = true;
        }
        else
        {
            modoTurnos = false;
        }
        int numPlayers = PlayerPrefs.GetInt("numPlayers");

        //Activamos el modo tutorial y mostramos el pergamino para los textos de tutorial
        if (tutorialActivo)
        {
            if (modoTurnos)
                elCanvasUI_Mapa.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = textosTutorial[0];
            else
            {
                //Venimos del paso anterior del manual. Mostramos el mensaje para pasar al combate manual
                posTutorial = 7;
                elCanvasUI_Mapa.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = textosTutorial[posTutorial];
                SiguienteFaseTutorial();
            }
        }
        elCanvasUI_Mapa.transform.GetChild(2).gameObject.SetActive(tutorialActivo);
        //Para forzar el número de jugadores, modoTurnos y currentplayer


        //currentPlayer = 2;
        /*
        numPlayers=1; 
        PlayerPrefs.SetInt("modoTurnos", 1);
        PlayerPrefs.SetInt("numPlayers", 1);
        */
        if (!tutorialActivo)
        {
            if (numPlayers == 1)
            {
                oponenteCPU = true;
                la_AI_Turnos.turnoIAterminado = true;
                la_AI_Turnos.gameObject.SetActive(true);
                la_AI_Real.gameObject.SetActive(true);
                StartCoroutine(la_AI_Turnos.PerformAIActions());
            }
            else
            {
                oponenteCPU = false;
                la_AI_Turnos.gameObject.SetActive(false);
                la_AI_Real.gameObject.SetActive(false);
            }
        }
        contadorTurnos = 1;
        elCampoBatallaManager.GetComponent<BatallaManager>().elTileManager = this;
        
        if (grabandoTrailer)
        {
            PlayerPrefs.SetInt("modoTurnos", 1);
            PlayerPrefs.SetInt("numPlayers", 2);
            oponenteCPU = false;
            la_AI_Turnos.gameObject.SetActive(false);
            la_AI_Real.gameObject.SetActive(false);
            Debug.Log("Grabando trailer. Eliminamos los ejércitos de los jugadores para que no se vean al empezar el trailer");
            //Eliminamos los ejércitos de los jugadores para que no se vean al empezar el trailer
            for (int i = jugadores[1].ejercitos.Count - 1; i >= 0; i--)
            {
                Debug.Log("Eliminando ejército: " + i + " = " + jugadores[1].ejercitos[i].name);
                Destroy(jugadores[1].ejercitos[i]);
                EliminarEjercito(jugadores[1].ejercitos[i]);
            }
            for (int i = jugadores[2].ejercitos.Count - 1; i >= 0; i--)
            {
                Destroy(jugadores[2].ejercitos[i]);
                EliminarEjercito(jugadores[2].ejercitos[i]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        //Para mostrar FPS
        deltaTimeParaFPS += (Time.deltaTime - deltaTimeParaFPS) * 0.1f;

        if ( Input.GetKeyUp(KeyCode.O)){
            Debug.Log("Opción O pulsada: "+AccionActiva());
        }
        if(Input.GetKeyDown(KeyCode.P)){
        }


        //Comprobamos si se ha acabado la partida.
        if(jugadores[1].ejercitos.Count == 0 && !grabandoTrailer){
            Debug.Log("Victoria del ejército 2. Mostraremos mensaje y volveremos al principio.");
            mensajeFinPartida.text = "PLAYER 2 WINS!!!!!";
            Time.timeScale = 0;
            elCanvasFinPartida.gameObject.SetActive(true);
            //SceneManager.LoadScene("Presentacion_y_menus", LoadSceneMode.Single);
        }
        if(jugadores[2].ejercitos.Count == 0 && !grabandoTrailer){
            Debug.Log("Victoria del ejército 1. Mostraremos mensaje y volveremos al principio.");
            mensajeFinPartida.text = "PLAYER 1 WINS!!!!!";
            Time.timeScale = 0;
            elCanvasFinPartida.gameObject.SetActive(true);
            //SceneManager.LoadScene("Presentacion_y_menus", LoadSceneMode.Single);
        }

        if (Input.GetKeyUp(KeyCode.E)){
            Debug.Log("tilemanager: Has pulsado E. Mostraremos info juego y si está descomentada la edición del mapa.");
            mostrarInfoDebug = !mostrarInfoDebug;
            elCanvasUI_Mapa.transform.GetChild(0).GetComponent<TMP_Text>().text = "";
            /*
            Desactivamos la edición para que el jugador no la active el jugador
            editandoMapa = !editandoMapa;
            if ( editandoMapa ){
                elCanvasEdicion.gameObject.SetActive(true);
                elCanvasUI_Mapa.gameObject.SetActive(false);
            }else{
                elCanvasEdicion.gameObject.SetActive(false);
                elCanvasUI_Mapa.gameObject.SetActive(true);
            }
            elCanvasEdicion.transform.GetChild(0).GetComponent<TMP_Text>().text = "Estado Actual: "+ numEstadoActual;
            */
        }
        if ( Input.GetKeyUp(KeyCode.Escape)){
            /*ANTES DESELECCIONÁBAMOS EL EJÉRCITO ACTUAL, AHORA VAMOS A MOSTRAR LAS OPCIONES
            ejercitoSeleccionado = null;
            estadoSeleccionado = false;
            //elMapaReino.GetComponent<MapaReino>().DesactivarCapitales();
            elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
            */
            if( elCanvasPausa.gameObject.activeSelf ){
                Time.timeScale = 1;
                elCanvasPausa.gameObject.SetActive(false);
                elCanvasOpciones.SetActive(false);
            }
            else{
                Time.timeScale = 0;
                elCanvasPausa.gameObject.SetActive(true);
            }
        }
        if(mostrarInfoDebug)
            mostrarInfoJuego();

        if( Input.GetKeyUp(KeyCode.O)){
            Debug.Log("Pintando línea.");
            // Draws a blue line from this transform to the target
            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine (elMapaReino.GetComponent<MapaReino>().capitalesEstados[3].transform.position, elMapaReino.GetComponent<MapaReino>().capitalesEstados[4].transform.position);
            /*
            List<Vector3> points = new List<Vector3>();
            for(int i=1; i<24;i++){
                points.Add(elMapaReino.GetComponent<MapaReino>().capitalesEstados[i].transform.position);
            }
            elLineRenderer.positionCount = points.Count;
            elLineRenderer.SetPositions(points.ToArray());
            */
        }
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


    //  ********    *******     ***         *******     *********   ********
    //  ***         **          ***         **          *********   ********
    //  *****       ****        ***         ****        ***            **
    //  *****       ****        ***         ****        ***            **
    //      ***     **          *********   **          *********      **
    //  *******     *******     *********   *******     *********      **
    public void OnHighlightTile(HexTile tile){
        //Debug.Log("Resaltamos el estado: "+tile.numEstado);
        if( tutorialActivo || grabandoTrailer)
            return;
        if( Time.timeScale == 0)
            return;
        //Si le toca a la máquina o estamos colocando una tropa no resalto nada
        if( oponenteCPU && currentPlayer == 2)
            return;

        highlight.transform.position = tile.transform.position;
        if( editandoMapa && tile.numEstado != -1){  //Estamos en modo edición
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
            //Si hay unidad en el estado, mostramos la info del ejército
            Capital laCapital = elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>();
            if( laCapital.ejercitoOcupante != null ){
                //elCanvasUI_Mapa.transform.GetChild(3).GetComponent<TMP_Text>().text = laCapital.ejercitoOcupante.GetComponent<Ejercito>().GetUnidades();
                elCanvasUI_Mapa.GetComponent<CanvasUI_MapaController>().MostrarInfoEjercito(laCapital.ejercitoOcupante.GetComponent<Ejercito>().GetUnidades());
            }
            if(estadoSeleccionado || luchando || atacando || uniendoTropas || ocupandoEstado || numTropaNueva != -1)  //Si he seleccionado ya un estado o se está desplazando una unidad o colocando una tropa no tengo que resaltar nada
                return; 
            //Resaltamos el estado sobre el que se pasa sólo si es del jugador actual (humano) y tiene tropa
            if( estadoResaltado != tile.numEstado){
                elMapaReino.GetComponent<MapaReino>().NoResaltarEstado(estadoResaltado);
                //Sólo resalto si es del player y no ha movido
                if( elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().propietario == currentPlayer &&
                    elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().ejercitoOcupante != null &&
                    elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().ejercitoOcupante.GetComponent<Ejercito>().haMovido == false){

//                        Debug.Log("Resaltando estado: "+tile.numEstado+" estado anterior era: "+estadoResaltado);
                        estadoResaltado = tile.numEstado;
                        elMapaReino.GetComponent<MapaReino>().ResaltarEstado(estadoResaltado, 0);
                }else{
                    estadoResaltado = -1;
                }
                //elMapaReino.GetComponent<MapaReino>().SeleccionarEstado(numEstadoActual);
            }
        }
    }

    public void OnSelectTile(HexTile tile){
        if (tutorialActivo)
            return;
        if(Time.timeScale == 0)
            return;
        if(numTropaNueva != -1){  //Hay que colocar tropa nueva
            //No se ha seleccionado dónde ponerla ni es el turno de la IA
            if(  contadorTurnos%2==0 && oponenteCPU ){
                ColocarEjercito(tile);
                return;
            }
            if( elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().seleccionada == false)
                return;
            ColocarEjercito(tile);
            return;
        }
        if(celdasRestantes != -1 || ocupandoEstado || atacando)
            return;
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
    //          ***     ***     ******  ***   **
    //          ***     ***     ***     *** **
    //          ***     ***     ***     *** **
    //          ***     ***     ******  ***   **
    //Para el movimiento que modificará el gamemanager según los movimientos de los jugadores
    public IEnumerator Tick(){
        while(true){
            if(combateActivo)
                break;
            if(ejercitoSeleccionado == null)
                yield return null;
            if (stop){
                yield return null;
            }else{
                celdasRestantes = -1;
                //Si tiene path establecido lo movemos en ese path y salimos (sólo se mueve un ejército cada vez)
                if (ejercitoSeleccionado != null && ejercitoSeleccionado.GetComponent<Ejercito>().GotCurrentPath()){
                    
                    celdasRestantes = ejercitoSeleccionado.GetComponent<Ejercito>().HandleMovement();
                    //Que HandleMovement devuelva si le queda una casilla para llegar y si es para combatir que no 
                    //siga, que los dos ejércitos se miren y reproducir el combate y resolverlo.
                    //Básicamente poner aquí el combate y luego el else de celdasRestantes = -1 para los otros
                    //dos casos (ocupar o unir tropas)
                    Capital laCapitalDestino = elMapaReino.GetComponent<MapaReino>().capitalesEstados[numEstadoActual].GetComponent<Capital>();
                    if( celdasRestantes == 2 && atacando){
                        if( elSoundManager.UnidadSeleccionadaP1Source.isPlaying)
                            elSoundManager.StopMusic("UnidadSeleccionadaP1Source");
                        Debug.Log("=======>En Tick: He llegado a mi destino que es atacar al enemigo. luchando: "+luchando);
                        GameObject ejercitoEnemigo = laCapitalDestino.GetComponent<Capital>().ejercitoOcupante;
                        ejercitoEnemigo.transform.LookAt(ejercitoSeleccionado.transform.position);
                        ejercitoSeleccionado.transform.LookAt(ejercitoEnemigo.transform.position);

                        if( modoTurnos){
                            //
                            //COMBATE POR TURNOS
                            //
                            ejercitoEnemigo.GetComponent<Ejercito>().Combatir();
                            ejercitoSeleccionado.GetComponent<Ejercito>().Combatir();
                            StartCoroutine(TiempoCombateTurnos());
                            yield return new WaitForSeconds(3f);  //Esperamos 3 segundos de combate, marcado por tiempoLucha
                            Debug.Log("Ha acabado el combate por turnos. luchando: "+luchando);
                            vencedor = ResolverCombate(ejercitoSeleccionado,laCapitalDestino.ejercitoOcupante);
                        }else{
                            Debug.Log("Para aquí en un bucle hasta que acabe el combate y luego seguir");
                            //
                            //**COMBATE REAL**
                            //
                            //Pasamos la info de las unidades para que se generen los ejércitos que toque
                            //MIRAR CÓMO SE HACE EN EL DE TURNOS PORQUE ES LO MISMO SALVO QUE EN LUGAR DE REPRODUCIR LA ANIMACIÓN DE LUCHA
                            // Y LUEGO VER EL VENCEDOR, HAY QUE HACER EL COMBATE REAL (LA ANIMACIÓN SÓLO QUE MÁS LARGA) Y VER EL VENCEDOR. 
                            // ASÍ PARECE MÁS SENCILLO QUE ESTO DE ABAJO.
                            
                            //Cerramos la cortinilla para que no se aprecie el cambio de escenario.
                            elSoundManager.StopMusic();
                            while( laCortinilla.cortinaCerrada == false ){
                                laCortinilla.targetContent.GetComponentInChildren<TMP_Text>().text = "TO\nBATTLE!!!";
                                laCortinilla.targetContent.GetComponent<Image>().color = new Color(0f,157f,0f);
                                laCortinilla.PlayFullCurtainAnimation();
                                yield return new WaitForSeconds(1.0f);  //Esperamos 1 segundo para comprobar si la cortinilla está cerrada
                            }
                            combateActivo = !combateActivo;
                            Debug.Log("Cambiando a combate activo: "+combateActivo);
                            camaraMapa.gameObject.SetActive(!combateActivo);
                            camaraBatalla.gameObject.SetActive(combateActivo);
                            elCanvasUI_Mapa.gameObject.SetActive(!combateActivo);
                            elCanvasUI_Batalla.gameObject.SetActive(combateActivo);
                            elSoundManager.PlayMusic(elSoundManager.musicaBatalla[0],false,1.0f,"Batalla");

                            //elCampoBatallaManager.GetComponent<BatallaManager>().combateActivo = combateActivo;
                            int tipoEscenario = UnityEngine.Random.Range(0,3); //Que elija random el tipo de terreno, en el futuro ya lo cambiaré
                            if( ejercitoSeleccionado.GetComponent<Ejercito>().numPlayer == 1){
                                elCampoBatallaManager.GetComponent<BatallaManager>().elEjercitoP1 = ejercitoSeleccionado;
                                elCampoBatallaManager.GetComponent<BatallaManager>().ejercitoAtacante = "Player1";
                                elCampoBatallaManager.GetComponent<BatallaManager>().elEjercitoP2 = laCapitalDestino.ejercitoOcupante;
                                elCampoBatallaManager.GetComponent<CampoBatallaTerrain>().InicializarTerreno(tipoEscenario);
                            }
                            else{
                                elCampoBatallaManager.GetComponent<BatallaManager>().elEjercitoP1 = laCapitalDestino.ejercitoOcupante;
                                elCampoBatallaManager.GetComponent<BatallaManager>().elEjercitoP2 = ejercitoSeleccionado;
                                elCampoBatallaManager.GetComponent<BatallaManager>().ejercitoAtacante = "Player2";
                                elCampoBatallaManager.GetComponent<CampoBatallaTerrain>().InicializarTerreno(tipoEscenario);
                            }
                            la_AI_Real.combateRealActivo = true;
                            la_AI_Real.EscogerUnidadObjetivo();
                            combateActivo = true;
                            //Llamaremos a batallaManager para que haga la batallaStartCoroutine(TiempoCombateReal());
                            while( combateActivo ){
                                yield return new WaitForSeconds(3.0f);  //Esperamos cada 3 segundos de combate
                            }
                            //Cerramos la cortinilla para que no se aprecie el cambio de escenario al volver al mapa.
                            while( laCortinilla.cortinaCerrada == false ){
                                if( vencedor == "atacante"){
                                    if( currentPlayer == 1){
                                        laCortinilla.targetContent.GetComponentInChildren<TMP_Text>().text = "PLAYER 1 \n WINS";
                                        laCortinilla.targetContent.GetComponent<Image>().color = new Color(0f,0f,157f);
                                    }else{
                                        laCortinilla.targetContent.GetComponentInChildren<TMP_Text>().text = "PLAYER 2 \n WINS";
                                        laCortinilla.targetContent.GetComponent<Image>().color = new Color(157f,0f,0f);
                                    }
                                }else{
                                    if( currentPlayer == 1){
                                        laCortinilla.targetContent.GetComponentInChildren<TMP_Text>().text = "PLAYER 2 \n WINS";
                                        laCortinilla.targetContent.GetComponent<Image>().color = new Color(157f,0f,0f);
                                    }else{
                                        laCortinilla.targetContent.GetComponentInChildren<TMP_Text>().text = "PLAYER 1 \n WINS";
                                        laCortinilla.targetContent.GetComponent<Image>().color = new Color(0f,0f,157f);
                                    }
                                }
                                laCortinilla.PlayFullCurtainAnimation();
                                yield return new WaitForSeconds(1.0f);  //Esperamos 1 segundo para comprobar si la cortinilla está cerrada
                            }
                            elCampoBatallaManager.GetComponent<BatallaManager>().DestruirCampoBatalla();
                            camaraMapa.gameObject.SetActive(!combateActivo);
                            camaraBatalla.gameObject.SetActive(combateActivo);
                            elCanvasUI_Mapa.gameObject.SetActive(!combateActivo);
                            elCanvasUI_Batalla.gameObject.SetActive(combateActivo);
                        }
                        if( vencedor == "atacante"){
                            Debug.Log("Ha ganado el atacante!! Elimino unidad del player defensor. Hay que ocupar el estado.");
                            /*uso el count de ejercito y así no controla en número que me da problemas
                            //Descuento la unidad del número de ejércitos del jugador que no está jugando (defensor) porque ha perdido
                            if( currentPlayer == 1){
                                jugadores[2].numEjercitos--;
                            }else{
                                jugadores[1].numEjercitos--;
                            }
                            */
                            //Informo en la cortinilla: elCanvasUI_Mapa.transform.GetChild(4).GetComponent<TMP_Text>().text = "Panel info:\n¡¡¡Victoria del ejército atacante!!!";
                            ejercitoSeleccionado.GetComponent<Ejercito>().Andar();
                            //Destroy(laCapitalDestino.ejercitoOcupante);
                            laCapitalDestino.ejercitoOcupante.GetComponent<Ejercito>().Morir();                                
                            EliminarEjercito(laCapitalDestino.ejercitoOcupante);
                            laCapitalDestino.GetComponent<Capital>().OcuparCapital(ejercitoSeleccionado);
                        }else{
                            /*uso el count de ejercito y así no controla en número que me da problemas
                            //Descuento la unidad del número de ejércitos del jugador que está jugando (atacante) porque ha perdido
                            if( currentPlayer == 1){
                                jugadores[1].numEjercitos--;
                            }else{
                                jugadores[2].numEjercitos--;
                            }
                            */
                            //Informo en la cortinilla: Debug.Log("Ha ganado el player defensor!! Eliminar unidad del player atacante. Hay que ocupar el estado.");
                            //elCanvasUI_Mapa.transform.GetChild(4).GetComponent<TMP_Text>().text = "\n¡¡¡Victoria del ejército defensor!!!";
                            //Destroy(ejercitoSeleccionado);
                            ejercitoSeleccionado.GetComponent<Ejercito>().Morir();
                            EliminarEjercito(ejercitoSeleccionado);
                            laCapitalDestino.ejercitoOcupante.transform.LookAt(new Vector3(0,0,-500));//Mirar al sur
                            laCapitalDestino.ejercitoOcupante.GetComponent<Ejercito>().Idle();

                            laCapitalDestino.GetComponent<Capital>().OcuparCapital(laCapitalDestino.ejercitoOcupante);
                        }
                        la_AI_Real.combateRealActivo = false;
                        atacando = false;

                    }else{
                        if( celdasRestantes == -1 ){//Ha llegado a su destino
                            if( elSoundManager.UnidadSeleccionadaP1Source.isPlaying)
                                elSoundManager.StopMusic("UnidadSeleccionadaP1Source");
                            ejercitoSeleccionado.transform.LookAt(new Vector3(0,0,-500));//Mirar al sur
                            laCapitalDestino = elMapaReino.GetComponent<MapaReino>().capitalesEstados[numEstadoActual].GetComponent<Capital>();
                            if( ocupandoEstado){
                                Debug.Log("=======>En Tick: He llegado a mi destino que es ocupar el estado. Lo ocupa player: "+ejercitoSeleccionado.GetComponent<Ejercito>().numPlayer);//+" El ejército: "+ejercitoSeleccionado.GetComponent<Ejercito>().indiceEjercito);
                                //elCanvasUI_Mapa.transform.GetChild(3).GetComponent<TMP_Text>().text = "Panel info:\nTerritorio ocupado.";
                                ocupandoEstado = false;
                                laCapitalDestino.GetComponent<Capital>().OcuparCapital(ejercitoSeleccionado);
                            }
                            if( uniendoTropas ){
                                Debug.Log("=======>En Tick: He llegado a mi destino que es unir las tropas. Las dos cuentan como movidas.");
                                //elCanvasUI_Mapa.transform.GetChild(3).GetComponent<TMP_Text>().text = "\nTropas unidas.";
                                uniendoTropas = false;
                                laCapitalDestino.ejercitoOcupante.GetComponent<Ejercito>().AnyadirTropas(ejercitoSeleccionado.GetComponent<Ejercito>().numCatapulta,ejercitoSeleccionado.GetComponent<Ejercito>().numInfanteria,ejercitoSeleccionado.GetComponent<Ejercito>().numCaballeria);
                                laCapitalDestino.ejercitoOcupante.GetComponent<Ejercito>().haMovido = true;
                                EliminarEjercito(ejercitoSeleccionado);
                                Destroy(ejercitoSeleccionado);
                            }
                        }
                    }
                }
                //Comprobamos si se ha finalizado el turno
                if( celdasRestantes == -1 ){
                    if( elSoundManager.UnidadSeleccionadaP1Source.isPlaying)
                        elSoundManager.StopMusic("UnidadSeleccionadaP1Source");
                    if( elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
                        elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
                    //Si hemos movido todas las tropas finaliza el turno
                    //Reiniciamos los movimientos de cada ejército
                   
                    int numMovidos = 0;
                    int tamanyoArray = jugadores[currentPlayer].ejercitos.Count;
                    foreach( GameObject unEjercito in jugadores[currentPlayer].ejercitos){
                        if( unEjercito.GetComponent<Ejercito>().haMovido){
                            numMovidos++;
                        }
                    }
                    if( tamanyoArray == numMovidos){
                        if( ! comprobandoFinTurno ){
                            Debug.Log("A comprobar fin de turno, si le damos oro o no y puede comprar tropa.");
                            ComprobarFinTurno();
                        }else{
                            FinalizarTurno();
                        }
                    }
                }
                //Fin de comprobación de fin de turno

                yield return new WaitForSeconds(0.7f);
                //StartCoroutine(Tick());
            }
            //}
        }
    }

    public IEnumerator TiempoCombateTurnos(){
        luchando = true;
        while(luchando){
            Debug.Log("Dándose de leches "+tiempoLucha+" luchando: "+luchando);
            if(tiempoLucha > 0){
                tiempoLucha--;
                elSoundManager.PlayRandomSound(elSoundManager.sonidosEspada, 0.5f, "Mapa");
                elSoundManager.PlayRandomSound(elSoundManager.sonidosGritoGolpe, 0.5f, "Mapa");
                //No va fino esto: ejercitoSeleccionado.GetComponent<Ejercito>().Combatir();
                yield return new WaitForSeconds(1.0f);
            }
            else{
                luchando = false;
                tiempoLucha = 2.5f;
                elSoundManager.PlayRandomSound(elSoundManager.sonidosMuerte, 0.8f, "Mapa");
                Debug.Log("Fin de darse de leches. luchando:"+ luchando);
                yield return null;
            }
        }
    }


    private void PintarFronteras(){
        //Pinto todas las fronteras
        HexTile[] hexTiles = gameObject.GetComponentsInChildren<HexTile>();

        foreach( HexTile laTile in hexTiles){
            laTile.PintarMisFronteras(unaFrontera,unaPlaya);
        }
    }

/*
    private void ResaltarEstado(int numEstado){
        Debug.Log("ResaltarEstado: el estado cambiará el material de todas sus celdas a Mat_BrilloSeleccionado.\nHabrá que trackear sobre qué estado se está para resaltarlo y volverlo a su estado anterior al no estar sobre él y resaltar el siguiente. \n También se podría usar para resaltar los estados a los que se puede mover/atacar con otro material.");
    }
*/
    private void ColocarArboles(){
        HexTile[] hexTiles = gameObject.GetComponentsInChildren<HexTile>();
        //HexTile kkk = elGrid[tileCoords.y*gridSize.y+tileCoords.x];
        Vector2Int gridSize = elMapaReino.GetComponent<MapaReino>().gridSize;

        for( int i=0; i< elMapaReino.GetComponent<MapaReino>().listaEstados.Count;i++){
            Estado unEstado = elMapaReino.GetComponent<MapaReino>().listaEstados[i];
            for( int j=0; j < unEstado.coordsTiles.Count;j++){
                HexTile laTile = hexTiles[unEstado.coordsTiles[j].y * gridSize.y + unEstado.coordsTiles[j].x];
                //Ponemos árboles o no siempre queno sea capital 
                if( laTile.tipoCelda > 1 && UnityEngine.Random.Range(0,100) < 10 && unEstado.tileCapital != j ){
                    GameObject unArbol = Instantiate(decoradosCeldas[0], laTile.transform.position ,Quaternion.Euler(0,UnityEngine.Random.Range(5,360),0));
                    unArbol.transform.localScale -= new Vector3(0.75f,0.75f,0.75f);
                    unArbol.transform.SetParent(laTile.transform, true);
                }
            }
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

        string[] estados=  {"{\"numEstado\":0,\"nombreEstado\":\"Estado 0\",\"tileCapital\":-1,\"coordsTiles\":[{\"x\":1,\"y\":1},{\"x\":2,\"y\":1},{\"x\":3,\"y\":1},{\"x\":4,\"y\":1},{\"x\":5,\"y\":1},{\"x\":6,\"y\":1},{\"x\":7,\"y\":1},{\"x\":8,\"y\":1},{\"x\":9,\"y\":1},{\"x\":10,\"y\":1},{\"x\":11,\"y\":1},{\"x\":12,\"y\":1},{\"x\":13,\"y\":1},{\"x\":14,\"y\":1},{\"x\":15,\"y\":1},{\"x\":16,\"y\":1},{\"x\":17,\"y\":1},{\"x\":18,\"y\":1},{\"x\":19,\"y\":1},{\"x\":20,\"y\":1},{\"x\":21,\"y\":1},{\"x\":22,\"y\":1},{\"x\":23,\"y\":1},{\"x\":24,\"y\":1},{\"x\":25,\"y\":1},{\"x\":26,\"y\":1},{\"x\":27,\"y\":1},{\"x\":28,\"y\":1},{\"x\":29,\"y\":1},{\"x\":30,\"y\":1},{\"x\":31,\"y\":1},{\"x\":32,\"y\":1},{\"x\":33,\"y\":1},{\"x\":34,\"y\":1},{\"x\":35,\"y\":1},{\"x\":36,\"y\":1},{\"x\":37,\"y\":1},{\"x\":38,\"y\":1},{\"x\":1,\"y\":2},{\"x\":2,\"y\":2},{\"x\":3,\"y\":2},{\"x\":4,\"y\":2},{\"x\":5,\"y\":2},{\"x\":6,\"y\":2},{\"x\":7,\"y\":2},{\"x\":8,\"y\":2},{\"x\":9,\"y\":2},{\"x\":10,\"y\":2},{\"x\":11,\"y\":2},{\"x\":12,\"y\":2},{\"x\":13,\"y\":2},{\"x\":14,\"y\":2},{\"x\":15,\"y\":2},{\"x\":16,\"y\":2},{\"x\":17,\"y\":2},{\"x\":18,\"y\":2},{\"x\":19,\"y\":2},{\"x\":20,\"y\":2},{\"x\":21,\"y\":2},{\"x\":22,\"y\":2},{\"x\":23,\"y\":2},{\"x\":24,\"y\":2},{\"x\":25,\"y\":2},{\"x\":26,\"y\":2},{\"x\":27,\"y\":2},{\"x\":28,\"y\":2},{\"x\":29,\"y\":2},{\"x\":30,\"y\":2},{\"x\":31,\"y\":2},{\"x\":32,\"y\":2},{\"x\":33,\"y\":2},{\"x\":34,\"y\":2},{\"x\":35,\"y\":2},{\"x\":36,\"y\":2},{\"x\":37,\"y\":2},{\"x\":38,\"y\":2},{\"x\":1,\"y\":3},{\"x\":2,\"y\":3},{\"x\":3,\"y\":3},{\"x\":4,\"y\":3},{\"x\":5,\"y\":3},{\"x\":6,\"y\":3},{\"x\":7,\"y\":3},{\"x\":8,\"y\":3},{\"x\":9,\"y\":3},{\"x\":10,\"y\":3},{\"x\":11,\"y\":3},{\"x\":12,\"y\":3},{\"x\":13,\"y\":3},{\"x\":14,\"y\":3},{\"x\":15,\"y\":3},{\"x\":16,\"y\":3},{\"x\":17,\"y\":3},{\"x\":18,\"y\":3},{\"x\":19,\"y\":3},{\"x\":20,\"y\":3},{\"x\":21,\"y\":3},{\"x\":22,\"y\":3},{\"x\":23,\"y\":3},{\"x\":24,\"y\":3},{\"x\":25,\"y\":3},{\"x\":26,\"y\":3},{\"x\":27,\"y\":3},{\"x\":28,\"y\":3},{\"x\":29,\"y\":3},{\"x\":30,\"y\":3},{\"x\":31,\"y\":3},{\"x\":32,\"y\":3},{\"x\":33,\"y\":3},{\"x\":34,\"y\":3},{\"x\":35,\"y\":3},{\"x\":36,\"y\":3},{\"x\":37,\"y\":3},{\"x\":38,\"y\":3},{\"x\":1,\"y\":4},{\"x\":2,\"y\":4},{\"x\":3,\"y\":4},{\"x\":4,\"y\":4},{\"x\":5,\"y\":4},{\"x\":6,\"y\":4},{\"x\":9,\"y\":4},{\"x\":12,\"y\":4},{\"x\":13,\"y\":4},{\"x\":14,\"y\":4},{\"x\":17,\"y\":4},{\"x\":18,\"y\":4},{\"x\":19,\"y\":4},{\"x\":21,\"y\":4},{\"x\":22,\"y\":4},{\"x\":23,\"y\":4},{\"x\":24,\"y\":4},{\"x\":25,\"y\":4},{\"x\":26,\"y\":4},{\"x\":27,\"y\":4},{\"x\":28,\"y\":4},{\"x\":29,\"y\":4},{\"x\":30,\"y\":4},{\"x\":31,\"y\":4},{\"x\":32,\"y\":4},{\"x\":34,\"y\":4},{\"x\":35,\"y\":4},{\"x\":36,\"y\":4},{\"x\":37,\"y\":4},{\"x\":38,\"y\":4},{\"x\":1,\"y\":5},{\"x\":2,\"y\":5},{\"x\":3,\"y\":5},{\"x\":7,\"y\":5},{\"x\":13,\"y\":5},{\"x\":14,\"y\":5},{\"x\":15,\"y\":5},{\"x\":17,\"y\":5},{\"x\":18,\"y\":5},{\"x\":19,\"y\":5},{\"x\":21,\"y\":5},{\"x\":22,\"y\":5},{\"x\":23,\"y\":5},{\"x\":24,\"y\":5},{\"x\":25,\"y\":5},{\"x\":26,\"y\":5},{\"x\":27,\"y\":5},{\"x\":28,\"y\":5},{\"x\":29,\"y\":5},{\"x\":30,\"y\":5},{\"x\":31,\"y\":5},{\"x\":34,\"y\":5},{\"x\":35,\"y\":5},{\"x\":36,\"y\":5},{\"x\":37,\"y\":5},{\"x\":38,\"y\":5},{\"x\":1,\"y\":6},{\"x\":2,\"y\":6},{\"x\":12,\"y\":6},{\"x\":13,\"y\":6},{\"x\":14,\"y\":6},{\"x\":21,\"y\":6},{\"x\":22,\"y\":6},{\"x\":24,\"y\":6},{\"x\":25,\"y\":6},{\"x\":26,\"y\":6},{\"x\":27,\"y\":6},{\"x\":33,\"y\":6},{\"x\":34,\"y\":6},{\"x\":35,\"y\":6},{\"x\":36,\"y\":6},{\"x\":37,\"y\":6},{\"x\":38,\"y\":6},{\"x\":1,\"y\":7},{\"x\":2,\"y\":7},{\"x\":3,\"y\":7},{\"x\":13,\"y\":7},{\"x\":14,\"y\":7},{\"x\":15,\"y\":7},{\"x\":25,\"y\":7},{\"x\":26,\"y\":7},{\"x\":34,\"y\":7},{\"x\":35,\"y\":7},{\"x\":36,\"y\":7},{\"x\":37,\"y\":7},{\"x\":38,\"y\":7},{\"x\":1,\"y\":8},{\"x\":2,\"y\":8},{\"x\":3,\"y\":8},{\"x\":11,\"y\":8},{\"x\":12,\"y\":8},{\"x\":14,\"y\":8},{\"x\":15,\"y\":8},{\"x\":33,\"y\":8},{\"x\":34,\"y\":8},{\"x\":35,\"y\":8},{\"x\":36,\"y\":8},{\"x\":37,\"y\":8},{\"x\":38,\"y\":8},{\"x\":1,\"y\":9},{\"x\":2,\"y\":9},{\"x\":3,\"y\":9},{\"x\":12,\"y\":9},{\"x\":15,\"y\":9},{\"x\":33,\"y\":9},{\"x\":34,\"y\":9},{\"x\":35,\"y\":9},{\"x\":36,\"y\":9},{\"x\":37,\"y\":9},{\"x\":38,\"y\":9},{\"x\":1,\"y\":10},{\"x\":2,\"y\":10},{\"x\":3,\"y\":10},{\"x\":4,\"y\":10},{\"x\":5,\"y\":10},{\"x\":14,\"y\":10},{\"x\":32,\"y\":10},{\"x\":33,\"y\":10},{\"x\":35,\"y\":10},{\"x\":36,\"y\":10},{\"x\":37,\"y\":10},{\"x\":38,\"y\":10},{\"x\":1,\"y\":11},{\"x\":2,\"y\":11},{\"x\":3,\"y\":11},{\"x\":4,\"y\":11},{\"x\":5,\"y\":11},{\"x\":33,\"y\":11},{\"x\":36,\"y\":11},{\"x\":37,\"y\":11},{\"x\":38,\"y\":11},{\"x\":1,\"y\":12},{\"x\":2,\"y\":12},{\"x\":37,\"y\":12},{\"x\":38,\"y\":12},{\"x\":1,\"y\":13},{\"x\":2,\"y\":13},{\"x\":37,\"y\":13},{\"x\":38,\"y\":13},{\"x\":1,\"y\":14},{\"x\":2,\"y\":14},{\"x\":35,\"y\":14},{\"x\":36,\"y\":14},{\"x\":37,\"y\":14},{\"x\":38,\"y\":14},{\"x\":1,\"y\":15},{\"x\":2,\"y\":15},{\"x\":3,\"y\":15},{\"x\":31,\"y\":15},{\"x\":35,\"y\":15},{\"x\":36,\"y\":15},{\"x\":37,\"y\":15},{\"x\":38,\"y\":15},{\"x\":1,\"y\":16},{\"x\":2,\"y\":16},{\"x\":30,\"y\":16},{\"x\":31,\"y\":16},{\"x\":32,\"y\":16},{\"x\":34,\"y\":16},{\"x\":35,\"y\":16},{\"x\":36,\"y\":16},{\"x\":37,\"y\":16},{\"x\":38,\"y\":16},{\"x\":1,\"y\":17},{\"x\":2,\"y\":17},{\"x\":30,\"y\":17},{\"x\":31,\"y\":17},{\"x\":32,\"y\":17},{\"x\":33,\"y\":17},{\"x\":34,\"y\":17},{\"x\":35,\"y\":17},{\"x\":36,\"y\":17},{\"x\":37,\"y\":17},{\"x\":38,\"y\":17},{\"x\":1,\"y\":18},{\"x\":2,\"y\":18},{\"x\":30,\"y\":18},{\"x\":31,\"y\":18},{\"x\":32,\"y\":18},{\"x\":33,\"y\":18},{\"x\":34,\"y\":18},{\"x\":35,\"y\":18},{\"x\":36,\"y\":18},{\"x\":37,\"y\":18},{\"x\":38,\"y\":18},{\"x\":1,\"y\":19},{\"x\":2,\"y\":19},{\"x\":3,\"y\":19},{\"x\":30,\"y\":19},{\"x\":31,\"y\":19},{\"x\":32,\"y\":19},{\"x\":33,\"y\":19},{\"x\":34,\"y\":19},{\"x\":35,\"y\":19},{\"x\":36,\"y\":19},{\"x\":37,\"y\":19},{\"x\":38,\"y\":19},{\"x\":1,\"y\":20},{\"x\":32,\"y\":20},{\"x\":33,\"y\":20},{\"x\":34,\"y\":20},{\"x\":35,\"y\":20},{\"x\":36,\"y\":20},{\"x\":37,\"y\":20},{\"x\":38,\"y\":20},{\"x\":1,\"y\":21},{\"x\":2,\"y\":21},{\"x\":35,\"y\":21},{\"x\":36,\"y\":21},{\"x\":37,\"y\":21},{\"x\":38,\"y\":21},{\"x\":1,\"y\":22},{\"x\":2,\"y\":22},{\"x\":34,\"y\":22},{\"x\":35,\"y\":22},{\"x\":36,\"y\":22},{\"x\":37,\"y\":22},{\"x\":38,\"y\":22},{\"x\":1,\"y\":23},{\"x\":2,\"y\":23},{\"x\":3,\"y\":23},{\"x\":36,\"y\":23},{\"x\":37,\"y\":23},{\"x\":38,\"y\":23},{\"x\":1,\"y\":24},{\"x\":2,\"y\":24},{\"x\":35,\"y\":24},{\"x\":36,\"y\":24},{\"x\":37,\"y\":24},{\"x\":38,\"y\":24},{\"x\":1,\"y\":25},{\"x\":2,\"y\":25},{\"x\":36,\"y\":25},{\"x\":37,\"y\":25},{\"x\":38,\"y\":25},{\"x\":1,\"y\":26},{\"x\":2,\"y\":26},{\"x\":37,\"y\":26},{\"x\":38,\"y\":26},{\"x\":1,\"y\":27},{\"x\":2,\"y\":27},{\"x\":38,\"y\":27},{\"x\":1,\"y\":28},{\"x\":2,\"y\":28},{\"x\":37,\"y\":28},{\"x\":38,\"y\":28},{\"x\":1,\"y\":29},{\"x\":2,\"y\":29},{\"x\":3,\"y\":29},{\"x\":36,\"y\":29},{\"x\":37,\"y\":29},{\"x\":38,\"y\":29},{\"x\":1,\"y\":30},{\"x\":2,\"y\":30},{\"x\":3,\"y\":30},{\"x\":36,\"y\":30},{\"x\":37,\"y\":30},{\"x\":38,\"y\":30},{\"x\":1,\"y\":31},{\"x\":2,\"y\":31},{\"x\":3,\"y\":31},{\"x\":4,\"y\":31},{\"x\":29,\"y\":31},{\"x\":36,\"y\":31},{\"x\":37,\"y\":31},{\"x\":38,\"y\":31},{\"x\":1,\"y\":32},{\"x\":2,\"y\":32},{\"x\":28,\"y\":32},{\"x\":35,\"y\":32},{\"x\":36,\"y\":32},{\"x\":37,\"y\":32},{\"x\":38,\"y\":32},{\"x\":1,\"y\":33},{\"x\":2,\"y\":33},{\"x\":3,\"y\":33},{\"x\":28,\"y\":33},{\"x\":29,\"y\":33},{\"x\":36,\"y\":33},{\"x\":37,\"y\":33},{\"x\":38,\"y\":33},{\"x\":1,\"y\":34},{\"x\":2,\"y\":34},{\"x\":23,\"y\":34},{\"x\":24,\"y\":34},{\"x\":28,\"y\":34},{\"x\":29,\"y\":34},{\"x\":35,\"y\":34},{\"x\":36,\"y\":34},{\"x\":37,\"y\":34},{\"x\":38,\"y\":34},{\"x\":1,\"y\":35},{\"x\":2,\"y\":35},{\"x\":23,\"y\":35},{\"x\":24,\"y\":35},{\"x\":28,\"y\":35},{\"x\":29,\"y\":35},{\"x\":30,\"y\":35},{\"x\":31,\"y\":35},{\"x\":32,\"y\":35},{\"x\":35,\"y\":35},{\"x\":36,\"y\":35},{\"x\":37,\"y\":35},{\"x\":38,\"y\":35},{\"x\":1,\"y\":36},{\"x\":8,\"y\":36},{\"x\":9,\"y\":36},{\"x\":10,\"y\":36},{\"x\":11,\"y\":36},{\"x\":12,\"y\":36},{\"x\":13,\"y\":36},{\"x\":14,\"y\":36},{\"x\":22,\"y\":36},{\"x\":23,\"y\":36},{\"x\":24,\"y\":36},{\"x\":28,\"y\":36},{\"x\":29,\"y\":36},{\"x\":30,\"y\":36},{\"x\":31,\"y\":36},{\"x\":32,\"y\":36},{\"x\":33,\"y\":36},{\"x\":34,\"y\":36},{\"x\":35,\"y\":36},{\"x\":36,\"y\":36},{\"x\":37,\"y\":36},{\"x\":38,\"y\":36},{\"x\":1,\"y\":37},{\"x\":2,\"y\":37},{\"x\":3,\"y\":37},{\"x\":4,\"y\":37},{\"x\":5,\"y\":37},{\"x\":6,\"y\":37},{\"x\":7,\"y\":37},{\"x\":9,\"y\":37},{\"x\":10,\"y\":37},{\"x\":11,\"y\":37},{\"x\":12,\"y\":37},{\"x\":13,\"y\":37},{\"x\":14,\"y\":37},{\"x\":15,\"y\":37},{\"x\":16,\"y\":37},{\"x\":19,\"y\":37},{\"x\":20,\"y\":37},{\"x\":21,\"y\":37},{\"x\":22,\"y\":37},{\"x\":23,\"y\":37},{\"x\":24,\"y\":37},{\"x\":25,\"y\":37},{\"x\":28,\"y\":37},{\"x\":29,\"y\":37},{\"x\":30,\"y\":37},{\"x\":31,\"y\":37},{\"x\":32,\"y\":37},{\"x\":33,\"y\":37},{\"x\":34,\"y\":37},{\"x\":35,\"y\":37},{\"x\":36,\"y\":37},{\"x\":37,\"y\":37},{\"x\":38,\"y\":37},{\"x\":1,\"y\":38},{\"x\":2,\"y\":38},{\"x\":3,\"y\":38},{\"x\":4,\"y\":38},{\"x\":5,\"y\":38},{\"x\":6,\"y\":38},{\"x\":8,\"y\":38},{\"x\":9,\"y\":38},{\"x\":10,\"y\":38},{\"x\":11,\"y\":38},{\"x\":12,\"y\":38},{\"x\":13,\"y\":38},{\"x\":14,\"y\":38},{\"x\":15,\"y\":38},{\"x\":16,\"y\":38},{\"x\":17,\"y\":38},{\"x\":18,\"y\":38},{\"x\":19,\"y\":38},{\"x\":20,\"y\":38},{\"x\":21,\"y\":38},{\"x\":22,\"y\":38},{\"x\":23,\"y\":38},{\"x\":24,\"y\":38},{\"x\":25,\"y\":38},{\"x\":28,\"y\":38},{\"x\":29,\"y\":38},{\"x\":30,\"y\":38},{\"x\":31,\"y\":38},{\"x\":32,\"y\":38},{\"x\":33,\"y\":38},{\"x\":34,\"y\":38},{\"x\":35,\"y\":38},{\"x\":36,\"y\":38},{\"x\":37,\"y\":38},{\"x\":38,\"y\":38},{\"x\":4,\"y\":15},{\"x\":4,\"y\":16},{\"x\":5,\"y\":15},{\"x\":4,\"y\":14},{\"x\":5,\"y\":16},{\"x\":12,\"y\":7},{\"x\":26,\"y\":38},{\"x\":27,\"y\":38},{\"x\":27,\"y\":37},{\"x\":11,\"y\":35},{\"x\":12,\"y\":35},{\"x\":7,\"y\":35},{\"x\":8,\"y\":37},{\"x\":7,\"y\":38},{\"x\":5,\"y\":36},{\"x\":6,\"y\":36},{\"x\":2,\"y\":20},{\"x\":3,\"y\":21},{\"x\":3,\"y\":22},{\"x\":4,\"y\":21},{\"x\":4,\"y\":22},{\"x\":4,\"y\":23},{\"x\":3,\"y\":27},{\"x\":4,\"y\":29},{\"x\":5,\"y\":29},{\"x\":4,\"y\":28},{\"x\":4,\"y\":5},{\"x\":3,\"y\":6},{\"x\":4,\"y\":6},{\"x\":0,\"y\":39},{\"x\":1,\"y\":39},{\"x\":2,\"y\":39},{\"x\":3,\"y\":39},{\"x\":4,\"y\":39},{\"x\":5,\"y\":39},{\"x\":6,\"y\":39},{\"x\":7,\"y\":39},{\"x\":8,\"y\":39},{\"x\":9,\"y\":39},{\"x\":2,\"y\":39},{\"x\":3,\"y\":39},{\"x\":4,\"y\":39},{\"x\":5,\"y\":39},{\"x\":6,\"y\":39},{\"x\":7,\"y\":39},{\"x\":1,\"y\":39},{\"x\":0,\"y\":38},{\"x\":0,\"y\":37},{\"x\":0,\"y\":39},{\"x\":9,\"y\":39},{\"x\":11,\"y\":39},{\"x\":12,\"y\":39},{\"x\":10,\"y\":39},{\"x\":8,\"y\":39},{\"x\":13,\"y\":39},{\"x\":14,\"y\":39},{\"x\":15,\"y\":39},{\"x\":16,\"y\":39},{\"x\":17,\"y\":39},{\"x\":18,\"y\":39},{\"x\":19,\"y\":39},{\"x\":20,\"y\":39},{\"x\":21,\"y\":39},{\"x\":22,\"y\":39},{\"x\":23,\"y\":39},{\"x\":24,\"y\":39},{\"x\":25,\"y\":39},{\"x\":26,\"y\":39},{\"x\":27,\"y\":39},{\"x\":28,\"y\":39},{\"x\":29,\"y\":39},{\"x\":30,\"y\":39},{\"x\":31,\"y\":39},{\"x\":32,\"y\":39},{\"x\":33,\"y\":39},{\"x\":34,\"y\":39},{\"x\":35,\"y\":39},{\"x\":36,\"y\":39},{\"x\":37,\"y\":39},{\"x\":38,\"y\":39},{\"x\":39,\"y\":39},{\"x\":39,\"y\":38},{\"x\":39,\"y\":37},{\"x\":39,\"y\":36},{\"x\":39,\"y\":35},{\"x\":39,\"y\":34},{\"x\":39,\"y\":33},{\"x\":39,\"y\":32},{\"x\":39,\"y\":31},{\"x\":39,\"y\":30},{\"x\":39,\"y\":29},{\"x\":39,\"y\":28},{\"x\":39,\"y\":27},{\"x\":39,\"y\":25},{\"x\":39,\"y\":26},{\"x\":39,\"y\":24},{\"x\":39,\"y\":23},{\"x\":39,\"y\":21},{\"x\":39,\"y\":22},{\"x\":39,\"y\":20},{\"x\":39,\"y\":19},{\"x\":39,\"y\":18},{\"x\":39,\"y\":17},{\"x\":39,\"y\":16},{\"x\":39,\"y\":15},{\"x\":39,\"y\":14},{\"x\":39,\"y\":13},{\"x\":39,\"y\":12},{\"x\":39,\"y\":11},{\"x\":39,\"y\":10},{\"x\":39,\"y\":9},{\"x\":39,\"y\":8},{\"x\":39,\"y\":7},{\"x\":39,\"y\":6},{\"x\":39,\"y\":5},{\"x\":39,\"y\":4},{\"x\":39,\"y\":3},{\"x\":39,\"y\":2},{\"x\":39,\"y\":1},{\"x\":39,\"y\":0},{\"x\":38,\"y\":2},{\"x\":38,\"y\":1},{\"x\":38,\"y\":0},{\"x\":36,\"y\":0},{\"x\":35,\"y\":0},{\"x\":34,\"y\":0},{\"x\":33,\"y\":0},{\"x\":32,\"y\":0},{\"x\":31,\"y\":0},{\"x\":29,\"y\":0},{\"x\":37,\"y\":0},{\"x\":30,\"y\":0},{\"x\":28,\"y\":0},{\"x\":27,\"y\":0},{\"x\":26,\"y\":0},{\"x\":25,\"y\":0},{\"x\":24,\"y\":0},{\"x\":22,\"y\":0},{\"x\":19,\"y\":0},{\"x\":18,\"y\":0},{\"x\":20,\"y\":0},{\"x\":21,\"y\":0},{\"x\":23,\"y\":0},{\"x\":15,\"y\":0},{\"x\":17,\"y\":0},{\"x\":16,\"y\":0},{\"x\":14,\"y\":0},{\"x\":13,\"y\":0},{\"x\":12,\"y\":0},{\"x\":11,\"y\":0},{\"x\":10,\"y\":0},{\"x\":9,\"y\":0},{\"x\":8,\"y\":0},{\"x\":7,\"y\":0},{\"x\":6,\"y\":0},{\"x\":5,\"y\":0},{\"x\":4,\"y\":0},{\"x\":3,\"y\":0},{\"x\":2,\"y\":0},{\"x\":1,\"y\":0},{\"x\":0,\"y\":0},{\"x\":0,\"y\":1},{\"x\":0,\"y\":2},{\"x\":0,\"y\":4},{\"x\":0,\"y\":5},{\"x\":0,\"y\":3},{\"x\":0,\"y\":7},{\"x\":0,\"y\":8},{\"x\":0,\"y\":6},{\"x\":0,\"y\":9},{\"x\":0,\"y\":10},{\"x\":0,\"y\":12},{\"x\":0,\"y\":14},{\"x\":0,\"y\":11},{\"x\":0,\"y\":16},{\"x\":0,\"y\":18},{\"x\":0,\"y\":22},{\"x\":0,\"y\":24},{\"x\":0,\"y\":26},{\"x\":0,\"y\":30},{\"x\":0,\"y\":32},{\"x\":0,\"y\":33},{\"x\":0,\"y\":34},{\"x\":0,\"y\":36},{\"x\":0,\"y\":29},{\"x\":0,\"y\":28},{\"x\":0,\"y\":27},{\"x\":0,\"y\":25},{\"x\":0,\"y\":19},{\"x\":0,\"y\":20},{\"x\":0,\"y\":17},{\"x\":0,\"y\":35},{\"x\":0,\"y\":31},{\"x\":0,\"y\":23},{\"x\":0,\"y\":21},{\"x\":0,\"y\":15},{\"x\":0,\"y\":13}],\"tipoCeldas\":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]}",
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
        ColocarArboles();
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

    /* 
     *  *****   ******   **      **
     *  **        **     ** *    **
     *  ****      **     **   *  **
     *  **        **     **    * **
     *  **      ******   **      **
     *  
     * Al acabar el turno de cada jugador:
     * -Reiniciamos los ejércitos para que puedan mover
     */

    public void botonFinTurnoPulsado(){
        Debug.Log("botonFinTurnoPulsado()"+" currentPlayer: "+currentPlayer+" oponenteCPU: "+oponenteCPU+" comprobandoFinTurno: "+comprobandoFinTurno+" luchando: "+luchando+" atacando: "+atacando+" uniendoTropas: "+uniendoTropas+" ocupandoEstado: "+ocupandoEstado+" numTropaNueva: "+numTropaNueva);
        //Si le toca a la máquina y es su turno no hacemos nada
        if( oponenteCPU && currentPlayer == 2)
            return;

        //audioSourceMapa.PlayOneShot(clickBoton);
        elSoundManager.PlaySound(elSoundManager.sonidosMenu,0, 0.4f);
        //Si se está a mitad de una acción no finalizamos turno
        if( luchando || atacando || uniendoTropas || ocupandoEstado || numTropaNueva != -1)
            return;
        foreach(GameObject elEjercito in jugadores[currentPlayer].ejercitos){
            elEjercito.GetComponent<Ejercito>().haMovido = true;
        }
        elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
        estadoSeleccionado = false;
        //Debug.Log("Fin del turno del player: "+ currentPlayer+" Compruebo su oro y si ha ganado una unidad para que la coloque.");
        ComprobarFinTurno();
    }

    public void ComprobarFinTurno(){
        comprobandoFinTurno = true;
        //Hay que colocar una tropa nueva antes de acabar el turno. Colocartropanueva llamará a FinalizarTurno()
        Debug.Log("Fin del turno del player: "+ currentPlayer+" Compruebo su oro y si ha ganado una unidad para que la coloque.");
        //Tiene suficientes territorios y no hay que colocar tropa ==> Le damos oro y si es necesario generamos la nueva tropa.
        int territoriosConquistados = GetTerritoriosConquistados(currentPlayer);
        if (  territoriosConquistados >= 5 && numTropaNueva ==  -1 ){
Debug.Log("ENTRANDO" + jugadores[currentPlayer].cantidadOro+" numtropanueva: "+numTropaNueva+" currentPlayer: "+currentPlayer+" oponenteCPU: "+oponenteCPU+" comprobandoFinTurno: "+comprobandoFinTurno+" luchando: "+luchando+" atacando: "+atacando+" uniendoTropas: "+uniendoTropas+" ocupandoEstado: "+ocupandoEstado);
            jugadores[currentPlayer].cantidadOro++;

            Debug.Log("*** PENDIENTE *** El player "+currentPlayer+" tiene: "+jugadores[currentPlayer].cantidadOro+" oro. Le damos +1 ORO y si tiene suficiente una unidad que deberá colocar.");
            //Tiene suficiente oro para comprar una unidad y tiene algún territorio sin ocupar. Se la damos y que la coloque antes de seguir el turno
            if( elMapaReino.GetComponent<MapaReino>().GetCapitalesDesocupadasPlayer(currentPlayer) != null && jugadores[currentPlayer].cantidadOro>=5 && territoriosConquistados > jugadores[currentPlayer].ejercitos.Count){
                Debug.Log("El player: "+currentPlayer+" tiene suficiente oro: "+jugadores[currentPlayer].cantidadOro+" y reinos disponibles: "+elMapaReino.GetComponent<MapaReino>().GetCapitalesDesocupadasPlayer(currentPlayer).Count+"  Le damos tropa.");
                jugadores[currentPlayer].cantidadOro -= 5;
                AddEjercito(currentPlayer);

                //Es el turno de la IA, colocamos la tropa 
                if( grabandoTrailer || (contadorTurnos%2 == 0 && oponenteCPU)){
                    List<int> indicesCapitalesVacias = elMapaReino.GetComponent<MapaReino>().GetCapitalesDesocupadasPlayer(2);
                    //La colocamos en una de las capitales vacías aleatoriamente
                    HexTile laTile = elMapaReino.GetComponent<MapaReino>().GetTileCapital(indicesCapitalesVacias[UnityEngine.Random.Range(0,indicesCapitalesVacias.Count-1)]);
                    ColocarEjercito(laTile);
                }
            }
        }
        elCanvasUI_Mapa.GetComponent<CanvasUI_MapaController>().EstablecerOroPlayers(jugadores[1].cantidadOro,jugadores[2].cantidadOro);
        if (numTropaNueva == -1)
            FinalizarTurno();
    }

    public void FinalizarTurno(){
        elCanvasUI_Mapa.GetComponent<CanvasUI_MapaController>().EstablecerOroPlayers(jugadores[1].cantidadOro,jugadores[2].cantidadOro);
        //Si es el turno de la IA llamamos a su comprobación.
        Debug.Log("Finalizar turno begin");
        if( contadorTurnos%2==0 && oponenteCPU ){
            la_AI_Turnos.turnoIAterminado = true;
        }
        InicializarMovimientoEjercito(jugadores[currentPlayer].ejercitos);
        if(currentPlayer == 1){
            currentPlayer = 2;
            la_AI_Turnos.turnoIAterminado = false;
        }else{
            currentPlayer = 1;
            la_AI_Turnos.turnoIAterminado = true;
        }
        Debug.Log("Finalizarturno antes de actualizar los letreros");
        //elCanvasUI_Mapa.transform.GetChild(4).GetComponent<TMP_Text>().text = "Oro Player 1: "+jugadores[1].cantidadOro+"\nOro Player 2: "+jugadores[2].cantidadOro;
        elCanvasUI_Mapa.GetComponent<CanvasUI_MapaController>().EstablecerOroPlayers(jugadores[1].cantidadOro,jugadores[2].cantidadOro);
        contadorTurnos++;
//        elCanvasUI_Mapa.transform.GetChild(5).GetComponent<TMP_Text>().text = "Turno: "+contadorTurnos;
        elCanvasUI_Mapa.GetComponent<CanvasUI_MapaController>().ResaltarTurnoPlayer(currentPlayer);
        comprobandoFinTurno = false;
    }

    //Actualizamos los vecinos de cada estado
    public void ActualizarEstadosVecinos(){
        foreach(GameObject elObjeto in elMapaReino.GetComponent<MapaReino>().elGridMapa){
            HexTile tile = elObjeto.GetComponent<HexTile>();
            foreach( HexTile vecino in tile.neighbours){
                if( vecino.numEstado != tile.numEstado ){
                    //Debug.Log("En la tile: "+tile.coordenada+" del estado: "+tile.numEstado+" Añadiendo el vecino: "+vecino.numEstado);
                    elMapaReino.GetComponent<MapaReino>().listaEstados[tile.numEstado].SetEstadoVecino(vecino.numEstado,true);
                }
            }
        }
    }

//
//    *** ***   *********   ***     ***     ******
//  *** ** ***  ***   ***    ***   ***      ***
//  ***    ***  ***   ***     *** ***       *****
//  ***    ***  ***   ***      ** **        ***
//  ***    ***  *********       ***         ******
//
    private void RealizarMovimientoJuego(HexTile tile){
        //Modo juego
        if (celdasRestantes != -1)  //Una tropa está en movimiento, ignoramos la selección
            return;
        //Si el estado está sin conquistar y estamos moviendo no hacer nada
        if(tile.numEstado ==0 ){ //El mar y terreno fuera del mapa no lo seleccionamos
            //elMapaReino.GetComponent<MapaReino>().DesactivarCapitales();
            elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
            estadoSeleccionado = false;
            return;
        }
        Capital laCapitalOrigen;
        Capital laCapitalDestino;

        if( ! estadoSeleccionado ){  //No hay estado seleccionado
            estadoSeleccionado = true;
            laCapitalOrigen = elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>();
            if( laCapitalOrigen.ejercitoOcupante != null && laCapitalOrigen.ejercitoOcupante.GetComponent<Ejercito>().numPlayer == currentPlayer){
                ejercitoSeleccionado = laCapitalOrigen.ejercitoOcupante;
                if ( ! ejercitoSeleccionado.GetComponent<Ejercito>().haMovido ){
                    Debug.Log("Capital ocupada por player: "+currentPlayer+" con tropa no movida antes. Seleccionando estado y ejército.");
                    //elMapaReino.GetComponent<MapaReino>().SeleccionarCapital(numEstadoActual);
                    //elMapaReino.GetComponent<MapaReino>().SeleccionarEstado(numEstadoActual);
                    elMapaReino.GetComponent<MapaReino>().SeleccionarEstadoYVecinos(numEstadoActual);
                    //elCanvasUI_Mapa.transform.GetChild(3).GetComponent<TMP_Text>().text = ejercitoSeleccionado.GetComponent<Ejercito>().GetUnidades();
                    elCanvasUI_Mapa.GetComponent<CanvasUI_MapaController>().MostrarInfoEjercito(ejercitoSeleccionado.GetComponent<Ejercito>().GetUnidades());
                }else{
                    Debug.Log("Capital del player y ya ha movido. No hago nada.");
                    ejercitoSeleccionado = null;
                    estadoSeleccionado = false;
                    elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
                }
            }else{
                Debug.Log("Capital del enemigo. Con o sin tropa. No hago nada.");
                ejercitoSeleccionado = null;
                estadoSeleccionado = false;
                elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
            }
        }else{ //Hay un estado seleccionado con anterioridad, moveremos tropa para conquistar o atacar según el caso
            estadoSeleccionado = false;
            //elMapaReino.GetComponent<MapaReino>().DesactivarCapitales();
            elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
            
            if( elMapaReino.GetComponent<MapaReino>().listaEstados[tile.numEstado].EsVecino(numEstadoAnterior) == false){
                Debug.Log("El estado anterior: "+numEstadoAnterior+" no es vecino de: "+numEstadoActual+" No hacemos nada.");
                return;
            }
            laCapitalOrigen = elMapaReino.GetComponent<MapaReino>().capitalesEstados[numEstadoAnterior].GetComponent<Capital>();
            laCapitalDestino = elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>();
            if( laCapitalDestino.ejercitoOcupante != null && laCapitalDestino.ejercitoOcupante.GetComponent<Ejercito>().numPlayer == currentPlayer ){
                Debug.Log("Capital ocupada con tropa del currentPlayer. Desocupo la origen. Me muevo para unirlas.");
                uniendoTropas = true;
                MoverEjercito(ejercitoSeleccionado);
                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
                                
            }else if( laCapitalDestino.ejercitoOcupante != null && laCapitalDestino.ejercitoOcupante.GetComponent<Ejercito>().numPlayer != currentPlayer ){
                Debug.Log("¡¡¡AL ATAQUE!!! Capital ocupada con tropa del enemigo. Desocupo la origen. Me muevo para atacar. Combatirán automático al llegar.");
                atacando = true;
                MoverEjercito(ejercitoSeleccionado);
                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
            }else if(laCapitalDestino.ejercitoOcupante == null) {
                ocupandoEstado = true;
                Debug.Log("Capital destino está libre. Me da igual de quien sea. Desocupo la origen. Me muevo para ocuparla.");
                MoverEjercito(ejercitoSeleccionado);
                laCapitalOrigen.GetComponent<Capital>().DesOcuparCapital(currentPlayer,ejercitoSeleccionado);
            }else{
                Debug.Log("ERROR: Este caso no está contemplado. REVISAR!!!!");
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
        elSoundManager.PlayMusic(elSoundManager.musicaUnidadSeleccionada[1],true,0.5f,"UnidadSeleccionadaP1Source");
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
    private string ResolverCombate(GameObject ejAtacante, GameObject ejDefensor){
        Ejercito EjercitoAta = ejAtacante.GetComponent<Ejercito>();
        Ejercito EjercitoDef = ejDefensor.GetComponent<Ejercito>();
        int resP1 = EjercitoAta.numInfanteria + EjercitoAta.numCaballeria * 2 + EjercitoAta.numCatapulta * 5 + UnityEngine.Random.Range(1,10);
        int resP2 = EjercitoDef.numInfanteria + EjercitoDef.numCaballeria * 2 + EjercitoDef.numCatapulta * 5 + UnityEngine.Random.Range(1,10);
        Debug.Log("Res1: "+resP1+" Res2: "+resP2+" Quito la catapulta y dejo un random entre 1 y 6 de las otras al vencedor.");
        if ( resP1 > resP2){
            ejAtacante.GetComponent<Ejercito>().SetUnidades(0,UnityEngine.Random.Range(1,6),UnityEngine.Random.Range(1,6));
            return "atacante";
        }
        if ( resP1 < resP2 ){
            ejDefensor.GetComponent<Ejercito>().SetUnidades(0,UnityEngine.Random.Range(1,6),UnityEngine.Random.Range(1,6));
            return "defensor";
        }
        //Si les ha dado el mismo resultado lo echamos a suertes
        if( UnityEngine.Random.Range(1,3) == 1){
            ejAtacante.GetComponent<Ejercito>().SetUnidades(0,UnityEngine.Random.Range(1,6),UnityEngine.Random.Range(1,6));
            return "atacante";
        }
        else{
            ejDefensor.GetComponent<Ejercito>().SetUnidades(0,UnityEngine.Random.Range(1,6),UnityEngine.Random.Range(1,6));
            return "defensor";
        }
    }

    private void mostrarInfoJuego(){
        string mensaje = "";
        float fps = 1.0f / deltaTimeParaFPS;
        mensaje += "FPS: "+ Mathf.Ceil(fps).ToString();
        mensaje += "\ncurrentPlayer: "+currentPlayer;//+ejercitoSeleccionado.GetComponent<Ejercito>().indiceEjercito;
        if( ejercitoSeleccionado != null)
            mensaje+="\nplayer: "+ejercitoSeleccionado.GetComponent<Ejercito>().numPlayer;//+" ejército seleccionado: "+ejercitoSeleccionado.GetComponent<Ejercito>().indiceEjercito;
        mensaje +="\n celdasRestantes: "+celdasRestantes+" atacando: "+atacando;
        mensaje +="\n uniendoTropas: "+ uniendoTropas+" ocupandoEstado: "+ocupandoEstado;
        mensaje +="\n numEjercitosP1: "+jugadores[1].ejercitos.Count+" numEjercitosP2: "+jugadores[2].ejercitos.Count;
        mensaje +="\n stop:"+stop+" editandoMapa: "+editandoMapa+" numEstadoActual: "+numEstadoActual;
        mensaje +="\n numEstadoAnterior: "+numEstadoAnterior+" estadoSeleccionado: "+estadoSeleccionado;
        mensaje +="\nCapital  -  propietario  -  indexEjercito\n";
        MapaReino elMapa = elMapaReino.GetComponent<MapaReino>();
        Capital laCapital;
        for( int i=1 ; i<elMapa.capitalesEstados.Count; i++){
            laCapital = elMapa.capitalesEstados[i].GetComponent<Capital>();

            string elIndiceEjercito = "vacío.";
            if( laCapital.ejercitoOcupante != null){
                if(laCapital.propietario == 1)
                    elIndiceEjercito = jugadores[1].ejercitos.IndexOf(laCapital.ejercitoOcupante).ToString();
                if(laCapital.propietario == 2)
                    elIndiceEjercito = jugadores[2].ejercitos.IndexOf(laCapital.ejercitoOcupante).ToString();
            }
            mensaje += "     "+ i+"      -       "+ laCapital.propietario +"       -       "+elIndiceEjercito+"\n";
        }
        elCanvasUI_Mapa.transform.GetChild(0).GetComponent<TMP_Text>().text = "Info Juego:\n"+mensaje;
        //Debug.Log(mensaje);
    }

    public void EliminarEjercito(GameObject elEjercito){
        if (elEjercito.GetComponent<Ejercito>().numPlayer == 1)
            jugadores[1].ejercitos.Remove(elEjercito);
        else
            jugadores[2].ejercitos.Remove(elEjercito);
    }

    //Obtener la cantidad de territorios conquistados por el jugador indicado para generar
    //el oro que corresponda
    private int GetTerritoriosConquistados(int numPlayer){
        int territoriosConquistados = 0; 
        foreach( GameObject capi in elMapaReino.GetComponent<MapaReino>().capitalesEstados){
            if( capi.GetComponent<Capital>().propietario == numPlayer){
                territoriosConquistados++;
            }
        }
        return territoriosConquistados;
    }

    private void AddEjercitoOLD(int numPlayer){
        //Añadir un ejército a la lista del player
        //Resaltar todos los territorios del player para colocar la unidad
        //      La coloque en un territorio vacío u ocupado para unirlas es igual porque es el final de su turno
        //parar el turno hasta que el player elija dónde colocar el nuevo ejército
        GameObject nuevoEjercito = Instantiate(modeloEjercito, new Vector3(0,0,0), new Quaternion());
        nuevoEjercito.GetComponent<Ejercito>().GetComponentInChildren<SkinnedMeshRenderer>().material = materialesEjercito[numPlayer];
        jugadores[numPlayer].ejercitos.Add(nuevoEjercito);
        int indiceEjercito = jugadores[numPlayer].ejercitos.IndexOf(nuevoEjercito);

        Debug.Log("En AddEjercito player "+numPlayer+". Tamaño array: "+jugadores[numPlayer].ejercitos.Count+" el índiceEjercito: "+indiceEjercito);
        jugadores[numPlayer].ejercitos[indiceEjercito].GetComponent<Ejercito>().numPlayer = numPlayer;
//        jugadores[numPlayer].ejercitos[indiceEjercito].GetComponent<Ejercito>().indiceEjercito = indiceEjercito;
        jugadores[numPlayer].ejercitos[indiceEjercito].transform.position = highlight.transform.position+ new Vector3(0,1f,0);
        jugadores[numPlayer].ejercitos[indiceEjercito].transform.SetParent(highlight.transform,true);
        numTropaNueva = indiceEjercito;
        
        /*Ya no resalto la capital
        foreach ( GameObject capital in elMapaReino.GetComponent<MapaReino>().capitalesEstados){
            if( capital.GetComponent<Capital>().propietario == numPlayer && capital.GetComponent<Capital>().ejercitoOcupante == null){
                capital.GetComponent<Capital>().ActivarCapital(new Color(1f,1f,0f));
            }
        }*/
        //Ahora resalto el estado completo
        elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
        for ( int i = 1 ; i < elMapaReino.GetComponent<MapaReino>().capitalesEstados.Count(); i++){
            Capital laCapital = elMapaReino.GetComponent<MapaReino>().capitalesEstados[i].GetComponent<Capital>();
            if( laCapital.propietario == numPlayer && laCapital.ejercitoOcupante == null){
                Debug.Log("¿Qué pasa aquí?: estado: "+i+" laCapital.propietario: "+laCapital.propietario+ " laCapital.jercitoOcupante: "+laCapital.ejercitoOcupante);
                elMapaReino.GetComponent<MapaReino>().SeleccionarSoloEstado(i);
            }
        }   
    }

    //El nuevo ejército se añade a la lista de ejércitos
    public void AddEjercito(int numPlayer)
    {
        // Crear el nuevo ejército
        GameObject nuevoEjercito = Instantiate(modeloEjercito, new Vector3(0, 0, 0), Quaternion.identity);
        nuevoEjercito.GetComponent<Ejercito>().GetComponentInChildren<SkinnedMeshRenderer>().material = materialesEjercito[numPlayer];

        // Añadir el nuevo ejército al final de la lista
        jugadores[numPlayer].ejercitos.Add(nuevoEjercito);
        int indiceEjercito = jugadores[numPlayer].ejercitos.Count - 1;

        // Configurar el nuevo ejército
        Debug.Log("En AddEjercito player " + numPlayer + ". Tamaño array: " + jugadores[numPlayer].ejercitos.Count + " el índiceEjercito: " + indiceEjercito);
        nuevoEjercito.GetComponent<Ejercito>().numPlayer = numPlayer;
        //nuevoEjercito.GetComponent<Ejercito>().indiceEjercito = indiceEjercito;
        nuevoEjercito.transform.position = highlight.transform.position + new Vector3(0, 1f, 0);
        nuevoEjercito.transform.SetParent(highlight.transform, true);

        // Actualizar el estado para colocar el ejército
        numTropaNueva = indiceEjercito;

        // Resaltar los estados disponibles para colocar el ejército
        elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
        for (int i = 1; i < elMapaReino.GetComponent<MapaReino>().capitalesEstados.Count(); i++)
        {
            Capital laCapital = elMapaReino.GetComponent<MapaReino>().capitalesEstados[i].GetComponent<Capital>();
            if (laCapital.propietario == numPlayer && laCapital.ejercitoOcupante == null)
            {
                Debug.Log("Estado disponible para colocar ejército: " + i);
                elMapaReino.GetComponent<MapaReino>().SeleccionarSoloEstado(i);
            }
        }

    }



    public void ColocarEjercito(HexTile tile){
        Debug.Log("numTropaNueva: "+numTropaNueva+ " a la tile: "+tile.name);
        //Hay una tropa a colocar
        jugadores[currentPlayer].ejercitos[numTropaNueva].transform.SetParent(null,true);
        jugadores[currentPlayer].ejercitos[numTropaNueva].GetComponent<Ejercito>().currentTile = tile;

        Vector2Int aux = elMapaReino.GetComponent<MapaReino>().listaEstados[tile.numEstado].GetCoordsCapital();
        tile = elMapaReino.GetComponent<MapaReino>().elGridMapa[aux.y*elMapaReino.GetComponent<MapaReino>().gridSize.y+aux.x].GetComponent<HexTile>();
        jugadores[currentPlayer].ejercitos[numTropaNueva].transform.position = tile.transform.position; //+ new Vector3(0f,1f,0f);
        jugadores[currentPlayer].ejercitos[numTropaNueva].transform.LookAt(new Vector3(0,0,-500));  //Mira siempre al sur
        jugadores[currentPlayer].ejercitos[numTropaNueva].GetComponent<Ejercito>().currentTile = tile;
        jugadores[currentPlayer].ejercitos[numTropaNueva].GetComponent<Ejercito>().haMovido = true;
        elMapaReino.GetComponent<MapaReino>().capitalesEstados[tile.numEstado].GetComponent<Capital>().OcuparCapital(jugadores[currentPlayer].ejercitos[numTropaNueva]);
        numTropaNueva = -1;
        //elMapaReino.GetComponent<MapaReino>().DesactivarCapitales();
        elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
        //FinalizarTurno();
    }

    //Para que la consulte la IA o el tutorial y sepa que se está haciendo algo y debe esperar
    public bool AccionActiva(){
        //if( contadorTurnos%2==0 && oponenteCPU ){        
            Debug.Log("Acción activa luchando || atacando || uniendoTropas || ocupandoEstado: "+luchando +"||"+ atacando +"||"+ uniendoTropas +"||"+ ocupandoEstado);
            return luchando || atacando || uniendoTropas || ocupandoEstado;
       /* }else{
            Debug.Log("NO Acción activa luchando || atacando || uniendoTropas || ocupandoEstado: "+luchando +"||"+ atacando +"||"+ uniendoTropas +"||"+ ocupandoEstado);
        }
        return  false;*/
    }

    public void VolverPantallaInicial(){
        if(combateActivo)
            elSoundManager.PlaySound(elSoundManager.sonidosMenu,0, 0.4f);
        else
            elSoundManager.PlaySound(elSoundManager.sonidosMenu,0, 0.4f);
        SceneManager.LoadScene("Presentacion_y_menus", LoadSceneMode.Single);
    }

    public void ContinuarPartida(){
        Time.timeScale = 1;
        if(combateActivo)
            elSoundManager.PlaySound(elSoundManager.sonidosMenu,0, 0.4f);//audioSourceBatalla.PlayOneShot(clickBoton);
        else
            elSoundManager.PlaySound(elSoundManager.sonidosMenu,0, 0.4f);//audioSourceMapa.PlayOneShot(clickBoton);
        elCanvasPausa.gameObject.SetActive(false);
    }

    public void QuitGame(){
        elSoundManager.PlaySound(elSoundManager.sonidosMenu,0, 0.4f);
        Application.Quit();
    }

    public void OpenSettings(){
        elSoundManager.PlaySound(elSoundManager.sonidosMenu,0, 0.4f);
        elCanvasOpciones.gameObject.SetActive(true);
    }

    public void CloseSettings(){
        elSoundManager.PlaySound(elSoundManager.sonidosMenu,0, 0.4f);
        elCanvasOpciones.gameObject.SetActive(false);
        elCanvasPausa.gameObject.SetActive(false);
        ContinuarPartida();
    }

    public void SiguienteFaseTutorial(){
        if(AccionActiva() && posTutorial < 8)
            return;

        posTutorial++;
        Debug.Log("PosTutorial:"+posTutorial);
        if(posTutorial >= textosTutorial.Length){
            elCanvasUI_Mapa.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "FIN DEL TUTORIAL! Ahora deberíamos ir al principio.";
            posTutorial = 0;
            Debug.Log("FIN DEL TUTORIAL:"+posTutorial);
        }else{
            elCanvasUI_Mapa.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = textosTutorial[posTutorial];
            switch(posTutorial ){
                case 1:
                    //Resaltar un estado
                    estadoResaltado=0;
                    for(int i = 0 ; i < elMapaReino.GetComponent<MapaReino>().capitalesEstados.Count; i++){
                        if(elMapaReino.GetComponent<MapaReino>().capitalesEstados[i].GetComponent<Capital>().propietario == 1
                            && elMapaReino.GetComponent<MapaReino>().capitalesEstados[i].GetComponent<Capital>().ejercitoOcupante != null){
                            estadoResaltado = i;
                        }
                    }
                    Debug.Log("posTutorial: "+posTutorial+" Resalto el estado: "+estadoResaltado);
                    elMapaReino.GetComponent<MapaReino>().ResaltarEstado(estadoResaltado, 0);
                break;
                case 2:
                    //Elegir un estado
                    numEstadoActual = estadoResaltado;
                    elMapaReino.GetComponent<MapaReino>().NoResaltarNingunEstado();
                    Debug.Log("Tutorial: estadoResaltado: "+estadoResaltado+" numEstadoActual: "+numEstadoActual);
                    HexTile laTile = elMapaReino.GetComponent<MapaReino>().GetTileCapital(estadoResaltado);
                    RealizarMovimientoJuego(laTile);
                    break;
                case 3:
                    //Mover un ejército
                    numEstadoAnterior = numEstadoActual;
                    HexTile laTiledest = elMapaReino.GetComponent<MapaReino>().GetTileCapital(8);
                    numEstadoActual = laTiledest.numEstado;
                    RealizarMovimientoJuego(laTiledest);
                    break;
                case 4:
                    //Elegir estado origen
                    numEstadoActual = estadoResaltado = 9;
                    HexTile laTileOrig = elMapaReino.GetComponent<MapaReino>().GetTileCapital(estadoResaltado);
                    RealizarMovimientoJuego(laTileOrig);
                    //Mover un ejército
                    numEstadoAnterior = numEstadoActual;
                    HexTile laTiledesti = elMapaReino.GetComponent<MapaReino>().GetTileCapital(3);
                    numEstadoActual = laTiledesti.numEstado;
                    RealizarMovimientoJuego(laTiledesti);
                    break;
                case 5:
                    //Sólo mostrar mensaje
                case 6:
                    //Sólo mostrar mensaje
                    break;
                case 7:
                    //Cargamos de nuevo el mapa en modo combate manual para mostrar el tutorial del combate
                    PlayerPrefs.SetInt("modoTurnos", 2);
                    PlayerPrefs.SetInt("numPlayers", 1);
                    PlayerPrefs.SetInt("tutorialActivo", 1);
                    SceneManager.LoadScene("Mapa", LoadSceneMode.Single);
                    break;
                case 8:
                    //Elegir estado origen
                    numEstadoActual = estadoResaltado = 9;
                    HexTile laTileOrigen = elMapaReino.GetComponent<MapaReino>().GetTileCapital(estadoResaltado);
                    RealizarMovimientoJuego(laTileOrigen);
                    //Mover un ejército
                    numEstadoAnterior = numEstadoActual;
                    HexTile laTiledestino = elMapaReino.GetComponent<MapaReino>().GetTileCapital(3);
                    numEstadoActual = laTiledestino.numEstado;
                    RealizarMovimientoJuego(laTiledestino);
                    elCampoBatallaManager.GetComponent<BatallaManager>().tutorialActivo = true;
                    elCanvasUI_Mapa.transform.GetChild(2).gameObject.SetActive(false);
                    elCanvasUI_Batalla.transform.GetChild(2).gameObject.SetActive(true);
                    Debug.Log("Posición 8 del tutorial. Vamos al campo de batalla");
                    break;
                case 9:
                    elCanvasUI_Batalla.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = textosTutorial[posTutorial];
                    Debug.Log("Posición 9 del tutorial. En el campo de batalla");
                    break;
                case 10:
                    elCanvasUI_Batalla.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = textosTutorial[posTutorial];
                    Debug.Log("Posición 10 del tutorial. En el campo de batalla");
                    break;
                case 11:
                    elCanvasUI_Batalla.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = textosTutorial[posTutorial];
                    Debug.Log("Posición 11 del tutorial. En el campo de batalla");
                    break;
                case 12:
                    elCanvasUI_Batalla.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = textosTutorial[posTutorial];
                    break;
                case 13: 
                    SceneManager.LoadScene("Presentacion_y_menus", LoadSceneMode.Single);
                    break;
                default:
                    Debug.LogError("Error en el tutorial. No se ha encontrado el estado resaltado para el paso: "+posTutorial);
                    break;
            }
        }        
    }
}