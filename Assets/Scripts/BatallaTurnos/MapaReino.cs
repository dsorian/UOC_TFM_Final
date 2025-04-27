using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;



//using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class MapaReino : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize;

    public List<GameObject> elGridMapa;

    [Header("Tile Settings")]
    public float outerSize = 1.1f;
    public float innerSize = 0f;
    public float height = 0.01f;
    public bool isFlatTopped;
    public Material[] materiales; //0 = materialNeutral, materialAgua,materialHierba,materialRoca,materialArena;
    public Material[] materialesSeleccion; //0 = material resaltado, 1 = material seleccionado 2 = material objetivos

    //public TileManager elTileManager;
    
    public List<Estado> listaEstados = new List<Estado>();  //Creo que pondré 23+el neutral(0), como en el juego original
    public List<GameObject> capitalesEstados = new List<GameObject>();
    public GameObject simboloCapital;

    private void OnEnable(){
        listaEstados.Add((Estado) ScriptableObject.CreateInstance(typeof(Estado)));
        listaEstados[0].SetEstado(0,"Estado 0",-1,new List<Vector2Int>(),new List<int>());

//        Debug.Log("Estado añadido: "+ listaEstados.Count);

        //Creamos la lista para guardar el mapa
        elGridMapa = new List<GameObject>();
        LayoutGrid();
    }

    //// Si no lo comento dibuja dos hexágonos y salen warnings
    /*
    private void OnValidate(){
        if (Application.isPlaying){
            LayoutGrid();
        }
    }
*/
    // Start is called before the first frame update
    void Start()
    {
        //Creamos las 23 capitales desde el principio y las ponemos fuera de cámara y luego sólo hay que moverlas
        //La 0 sería la neutral que no se usará
        for(int i=0; i<24; i++){
            //posTile = elMapaReino.GetComponent<MapaReino>().listaEstados[i].coordsTiles[0].y * elMapaReino.GetComponent<MapaReino>().gridSize.y+elMapaReino.GetComponent<MapaReino>().listaEstados[0].coordsTiles[0].x;
            GameObject capitalAux = Instantiate(simboloCapital, new Vector3(400,0,0), new Quaternion(0,180,0,0));
            capitalesEstados.Add(capitalAux);        
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.L)){
            Debug.Log("Mostrando estados y sus celdas: ");
            int numEstado = 0;
            foreach(Estado estado in listaEstados){
                string mensaje = "";
                foreach(Vector2Int tileCoords in estado.coordsTiles){
                    //Debug.Log("????? Tengo que obtener la tile a partir de su x-y: x*numcols+y");
                    mensaje = mensaje + ", "+ elGridMapa[tileCoords.y*gridSize.y+tileCoords.x];
                }
                estado.MostrarEstado();
                numEstado++;
            }
        }*/
    }

    private void LayoutGrid(){
        for (int y = 0; y < gridSize.y; y++){
            for (int x = 0; x < gridSize.x; x++){
                GameObject tile = new GameObject($"Hex {x},{y}", typeof(HexTile));
                tile.transform.position = GetPositionForHexFromCoordinate( new Vector2Int(x,y));

                HexTile hextile = tile.GetComponent<HexTile>();
                hextile.isFlatTopped = isFlatTopped;
                hextile.outerSize = outerSize;
                hextile.innersize = innerSize;
                hextile.height = height;
                hextile.coordenada = new Vector2Int(x,y);
                hextile.materiales = materiales;
                hextile.nombre = "Hex "+x+","+y;

                //hextile.elTileManager = elTileManager;
                //El material será el mismo para todos
                //Material elMaterial;// = materiales[3];  //Todos roca, por ahora
                
                int tipoCelda = 3;
                int numEstado = 0;
                if( x==0 || x==gridSize.x-1 || y==0 || y==gridSize.y-1 ){
                    //elMaterial = materialNeutral;
                    tipoCelda = 0;
                    numEstado = 0; //Estado neutral
                }/*else{
                    //de momento el material se pone random
                    tipoCelda = UnityEngine.Random.Range(1,4);
                    elMaterial = null;
                    if( tipoCelda == 1 ){
                        elMaterial = material1;
                    }
                    if( tipoCelda ==2 ){
                        elMaterial = material2;
                    }
                    if(tipoCelda == 3){
                        elMaterial = material3;
                    }
                }*/
                hextile.tipoCelda = tipoCelda;
                hextile.numEstado = numEstado;//Test con tres tipos de provincia
                hextile.materialCelda = materiales[tipoCelda];
                hextile.DrawMesh();

                tile.transform.SetParent(transform, true);
                elGridMapa.Add(tile);    //Añadimos el tile a la lista

                //Asignar sus coordenadas de desplazamiento para el uso humano (Columna, fila)
                hextile.offsetCoordinate = new Vector2Int(x,y);
                //Asignar/convertir esto a coordenadas cúbicas para navegación
                hextile.cubeCoordinate = Utilities.OffsetToCube(hextile.offsetCoordinate);
                //Añado la Tile a su estado
                //if( hextile.numEstado != -1){
                listaEstados[hextile.numEstado].AnyadirTile(hextile);
                //}
            }
        }
    }

    public Vector3 GetPositionForHexFromCoordinate(Vector2Int coordinate){
        int column = coordinate.x;
        int row = coordinate.y;
        float width;
        float height;
        float xPosition = 0;
        float yPosition = 0;
        bool shouldOffset;
        float horizontalDistance;
        float verticalDistance;
        float offset;
        float size = outerSize;

        if ( !isFlatTopped){
            shouldOffset = (row % 2) == 0;
            width = Mathf.Sqrt(3) * size;
            height = 2f * size;

            horizontalDistance = width;
            verticalDistance = height * (3f/4f);

            offset = (shouldOffset) ? width/2 : 0;
            xPosition = (column * (horizontalDistance)) + offset;
            yPosition = (row * verticalDistance);
        }else{
            shouldOffset = (column % 2) == 0;
            width = 2f * size;
            height = Mathf.Sqrt(3) * size;

            horizontalDistance = width * (3f / 4f);
            verticalDistance = height;

            offset = (shouldOffset) ? height/2 : 0;
            xPosition = (column * (horizontalDistance));
            yPosition = (row * verticalDistance) - offset;
        }
        return new Vector3(xPosition, 0, -yPosition)+new Vector3(0,0,-200); //Para que cree el mapa -200 en la z y no en 0,0 que molesta con el campo de batalla
    }

    //Cambia el estado de una tile, quitándola del estado origen y poniéndola en el de destino
    public void CambiarTileDeEstado(HexTile tile, int estadoDestino){
        //Quito la tesela del estado origen
        listaEstados[tile.numEstado].QuitarTile(tile);
        //Meto la tesela en el estado destino
        listaEstados[estadoDestino].AnyadirTile(tile);
    }

    //Actualizamos el estado de todas las tiles para cuando cambie al cargar un mapa
    public void ActualizarEstadoDeTiles(){
        //para todos los estados hacer
        //  para todas las tiles del estado hacer
        //      coger la offsetcoordinate de la tile y ponerle el estado en el que estamos
        foreach(Estado elEstado in listaEstados){
            for( int i=0; i<elEstado.coordsTiles.Count;i++){
                int posTile = elEstado.coordsTiles[i].y * gridSize.y + elEstado.coordsTiles[i].x;
                HexTile tile = elGridMapa[posTile].GetComponent<HexTile>();
                if( tile.GetComponent<HexTile>().numEstado != elEstado.numEstado){
//                    Debug.Log("i: "+i+"         > La tile: "+ tile.name+" tiene mal el estado. Se lo pongo.");
                    tile.GetComponent<HexTile>().numEstado =  elEstado.numEstado;
                }
                tile.tipoCelda = elEstado.GetMaterialCelda(tile);
                tile.GetComponent<HexTile>().SetMaterial(materiales[elEstado.GetMaterialCelda(tile)]);
            }
        }
        //ColocarCapitales();
    }

    public void ColocarCapital(int numCapital, Vector3 posicion){
        capitalesEstados[numCapital].transform.position = posicion; 
    }

    public void ColocarCapitales(){
        //Colocamos las capitales (el 0 es el neutral que no tiene capital, no lo moveremos)
        Estado elEstado;
        for(int i=1 ; i<listaEstados.Count; i++){
            elEstado = listaEstados[i];
            int posTile = elEstado.GetCoordsCapital().y * gridSize.y + elEstado.GetCoordsCapital().x;
            //Debug.Log("Colocando capital: "+i+" del estado: "+elEstado.numEstado+" en la posTile: "+posTile);
            capitalesEstados[i].transform.position = elGridMapa[posTile].transform.position;
            capitalesEstados[i].transform.SetParent(elGridMapa[posTile].transform, true);
        }
    }

    //Cuando al seleccionar un estado activamos su capital y vecinos como seleccionados
    //Obsoleto, para quitar
    public void SeleccionarCapital(int numEstado){
        DesactivarCapitales();
        capitalesEstados[numEstado].GetComponent<Capital>().ActivarCapital(new Color(0f,0f,1f));
        for( int i=0;  i < listaEstados[numEstado].estadosVecinos.Length;i++){
            if(listaEstados[numEstado].estadosVecinos[i] == true){//Activo también los vecinos (en rojo)
                capitalesEstados[i].GetComponent<Capital>().ActivarCapital(new Color(1f,0f,0f));
            }
        }
    }

    public void SeleccionarEstadoYVecinos(int numEstado){
        if(numEstado == -1 || numEstado == 0)
            return;
        NoResaltarNingunEstado();
        Debug.Log("Seleccionamos el estado: "+numEstado);
        ResaltarEstado(numEstado, 1);
        capitalesEstados[numEstado].GetComponent<Capital>().ActivarCapital(new Color(0f,0f,1f));
        for( int i=0;  i < listaEstados[numEstado].estadosVecinos.Length;i++){
            if(listaEstados[numEstado].estadosVecinos[i] == true){//Activo también los vecinos 
                ResaltarEstado(i, 2);
                capitalesEstados[i].GetComponent<Capital>().ActivarCapital(new Color(0f,0f,1f));
            }
        }
    }

    //Para resaltar el estado cuando pasemos sobre él con el tipo que queramos 
    //0 = material resaltado, 1 = material seleccionado 2 = material objetivos
    public void ResaltarEstado(int numEstado, int tipoResaltado){
        if(numEstado == -1 || numEstado == 0)
            return;
//        Debug.Log("Resaltamos el estado: "+numEstado);

        foreach(var lasCoords in listaEstados[numEstado].coordsTiles){
            //Obtener la tile que es
            int posTile = lasCoords.y * gridSize.y +  lasCoords.x;
            HexTile tile = elGridMapa[posTile].GetComponent<HexTile>();
            //Le cambio material al de resaltado
            tile.ResaltarCelda(materialesSeleccion[tipoResaltado]);
        }
    }

    public void NoResaltarEstado(int numEstado){
        if(numEstado == -1 || numEstado == 0)
            return;
//        Debug.Log("NO Resaltamos el estado: "+numEstado);

        foreach(var lasCoords in listaEstados[numEstado].coordsTiles){
            //Obtener la tile que es
            int posTile = lasCoords.y * gridSize.y +  lasCoords.x;
            HexTile tile = elGridMapa[posTile].GetComponent<HexTile>();
            //Le cambio material al de resaltado
            tile.SetMaterial(tile.materialCelda);
            capitalesEstados[numEstado].GetComponent<Capital>().DesactivarCapital();
        }
    }

    public void NoResaltarNingunEstado(){
        Debug.Log("NO Resaltamos ningún estado");

        foreach( Estado unEstado in listaEstados){
            foreach(var lasCoords in listaEstados[unEstado.numEstado].coordsTiles){
                //Obtener la tile que es
                int posTile = lasCoords.y * gridSize.y +  lasCoords.x;
                HexTile tile = elGridMapa[posTile].GetComponent<HexTile>();
                //Le cambio material al de resaltado
                tile.SetMaterial(tile.materialCelda);
                capitalesEstados[unEstado.numEstado].GetComponent<Capital>().DesactivarCapital();
            }
        }
    }

    //Para cambiar el material de las celdas del estado seleccionado y sus vecinos
    public void SeleccionarSoloEstado(int numEstado){
        if(numEstado == -1 || numEstado == 0)
            return;
        Debug.Log("Resaltamos sólo el estado: "+numEstado+" En modo edición puede que falle porque ha cambiado mucho!!!!!");
        
        capitalesEstados[numEstado].GetComponent<Capital>().ActivarCapital(new Color(0f,0f,1f));
        foreach(var lasCoords in listaEstados[numEstado].coordsTiles){
            //Obtener la tile que es
            int posTile = lasCoords.y * gridSize.y +  lasCoords.x;
            HexTile tile = elGridMapa[posTile].GetComponent<HexTile>();
            //Le cambio material al de resaltado
            tile.ResaltarCelda(materialesSeleccion[1]);
        }
    }

    public void DesactivarCapitales(){
        foreach(GameObject capital in capitalesEstados){
            capital.GetComponent<Capital>().DesactivarCapital();
        }
    }

    //Obtenemos la tile de la capital de un estado. 
    //Para que la IA sepa dónde mover o dónde poner una nnueva unidad, etc
    public HexTile GetTileCapital(int numEstado){
        Estado elEstado = listaEstados[numEstado];
        int posTile = elEstado.GetCoordsCapital().y * gridSize.y + elEstado.GetCoordsCapital().x;
        return elGridMapa[posTile].GetComponent<HexTile>();
    }

    public bool CapitalOcupada(int numEstado, int numPlayer){
        bool isOcupada = false;
        if(capitalesEstados[numEstado].GetComponent<Capital>().GetPropietario() == numPlayer){
            isOcupada = true;
        }
Debug.Log("¿Capital de estado: "+numEstado +" ocupada por player: "+numPlayer+"?: "+isOcupada);
        return isOcupada;
    }


    public int GetPropietario(int numEstado){
        return capitalesEstados[numEstado].GetComponent<Capital>().propietario;
    }

    public List<int> GetCapitalesDesocupadasPlayer(int numPlayer){
        List<int> listaCapitales = new List<int>();

        for(int i=0; i<capitalesEstados.Count; i++){
            if( capitalesEstados[i].GetComponent<Capital>().propietario == numPlayer && capitalesEstados[i].GetComponent<Capital>().ejercitoOcupante == null){
                listaCapitales.Add(i);
            }
        }
        return listaCapitales;
    }

    public List<int> GetCapitalesPlayer(int numPlayer){
        List<int> listaCapitales = new List<int>();

        for(int i=0; i<capitalesEstados.Count; i++){
            if( capitalesEstados[i].GetComponent<Capital>().propietario == numPlayer){
                listaCapitales.Add(i);
            }
        }
        return listaCapitales;
    }


/*
    A partir de las coordenadas de cada tile obtener en qué estado está y cambiárselo
    public void ActualizarEstadoCeldas(){
        HexTile[] hexTiles = gameObject.GetComponentsInChildren<HexTile>();
        foreach(HexTile tile in hexTiles){
            tile.numEstado = ????
        }
    }
*/

    //  *******      ******      ********    ******     *       *        ******
    //  **     *    *      *        **      *      *    *       *       *      *
    //  **     *    *      *        **      *      *    *       *       *      *
    //  *******      ******         **       ******     *       *        ******
    //  **     *    *      *        **      *      *    *       *       *      *
    //  **     *    *      *        **      *      *    *       *       *      *
    //  *******     *      *        **      *      *     *****   *****  *      *
    
}
