using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

//using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


/*
Generar el grid de hexágonos: https://www.youtube.com/watch?v=EPaSmQ2vtek
Navegar por el grid: https://www.youtube.com/watch?v=wxVgIH0j8Wg
Camera controller: https://www.youtube.com/watch?v=rnqF6S7PfFA
Dibujar silueta: https://www.youtube.com/watch?v=ehyMwVnnnTg

*/

public class MapaReinoBackup20240824 : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize;

    public List<GameObject> elGrid;

    [Header("Tile Settings")]
    public float outerSize = 1f;
    public float innerSize = 0f;
    public float height = 1f;
    public bool isFlatTopped;
    public Material[] materiales; //0 = materialNeutral, materialAgua,materialHierba,materialRoca,materialArena;
    
    //public TileManager elTileManager;
    
    //Para gestionar los estados del reino
    //1.-(**HECHO**)Hacer que se pueda salvar el grid y los estados
    //2.-(**HECHO**)Hacer que se pueda cargar el grid y los estados
    //(**HECHO**) 3.-Hacer que los estados tengan sus fronteras 
    //      3.1.- (**HECHO**)Que no se creen dos bloques por frontera, sólo uno
    //      3.2.- Que se pueda pintar celdas de distinto material (agua, arena, tierra, ...)
    //4.-Que al clickar en una celda se destaque el estado al que pertenece
    //5.-Que el player vaya de la capital de un estado a la del otro
    //6.-Que los enemigos también se vayan de una capital a otra
    //7.-Si hay un enemigo/player se empieza el combate
    //8.-Si no hay nadie se conquista el estado
    //9.-
    public List<Estado> listaEstados = new List<Estado>();  //Creo que pondré 23+el neutral(0), como en el juego original
    private List<GameObject> capitalesEstados = new List<GameObject>();
    public GameObject simboloCapital;

    public List<int> borrarNumeros = new List<int>();

    private void OnEnable(){
        listaEstados.Add((Estado) ScriptableObject.CreateInstance(typeof(Estado)));
        listaEstados[0].SetEstado(0,"Estado 0",-1,new List<Vector2Int>(),new List<int>());

        Debug.Log("Estado añadido: "+ listaEstados.Count);

        //Creamos la lista para guardar el mapa
        elGrid = new List<GameObject>();
        LayoutGrid();

        for(int i=0; i<10;i++)
            borrarNumeros.Add(i*10);
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
            GameObject capitalAux = Instantiate(simboloCapital, new Vector3(400,0,0), new Quaternion());
            capitalesEstados.Add(capitalAux);        
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S)){
            string tempStr = "";
            for(int i=0; i<borrarNumeros.Count;i++){
                tempStr = tempStr+", "+borrarNumeros[i];
            }
            Debug.Log("La lista de números: "+ tempStr);
        }
        if (Input.GetKeyUp(KeyCode.A)){
            Debug.Log("Añado el 88 al final.");
            borrarNumeros.Add(88);
        }
        if (Input.GetKeyUp(KeyCode.Q)){
            Debug.Log("Quito elemento 5º");
            borrarNumeros.RemoveAt(5);
        }
        if (Input.GetKeyUp(KeyCode.L)){
            Debug.Log("Mostrando estados y sus celdas: ");
            int numEstado = 0;
            foreach(Estado estado in listaEstados){
                string mensaje = "";
                foreach(Vector2Int tileCoords in estado.coordsTiles){
                    //Debug.Log("????? Tengo que obtener la tile a partir de su x-y: x*numcols+y");
                    mensaje = mensaje + ", "+ elGrid[tileCoords.y*gridSize.y+tileCoords.x];
                }
                estado.MostrarEstado();
                numEstado++;
            }
        }
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
                elGrid.Add(tile);    //Añadimos el tile a la lista

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
        return new Vector3(xPosition, 0, -yPosition);
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
                HexTile tile = elGrid[posTile].GetComponent<HexTile>();
                if( tile.GetComponent<HexTile>().numEstado != elEstado.numEstado){
//                    Debug.Log("i: "+i+"         > La tile: "+ tile.name+" tiene mal el estado. Se lo pongo.");
                    tile.GetComponent<HexTile>().numEstado =  elEstado.numEstado;
                }
                tile.tipoCelda = elEstado.GetMaterialCelda(tile);
                tile.GetComponent<HexTile>().SetMaterial(materiales[elEstado.GetMaterialCelda(tile)]);
            }
        }
        ColocarCapitales();
    }

    public void ColocarCapital(int numCapital, Vector3 posicion){
        capitalesEstados[numCapital].transform.position = posicion; 
    }

    public void ColocarCapitales(){
        //Colocamos las capitales (el 0 es el neutral que no tiene capital, no lo moveremos)
        for(int i=1 ; i<listaEstados.Count; i++){
            Estado elEstado = listaEstados[i];
            int posTile = elEstado.GetCoordsCapital().y * gridSize.y + elEstado.GetCoordsCapital().x;
            //Debug.Log("Colocando capital: "+i+" del estado: "+elEstado.numEstado+" en la posTile: "+posTile);
            capitalesEstados[i].transform.position = elGrid[posTile].transform.position;
        }
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
}
