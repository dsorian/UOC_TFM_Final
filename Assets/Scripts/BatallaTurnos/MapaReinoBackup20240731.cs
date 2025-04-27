using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
Generar el grid de hexágonos: https://www.youtube.com/watch?v=EPaSmQ2vtek
Navegar por el grid: https://www.youtube.com/watch?v=wxVgIH0j8Wg
Camera controller: https://www.youtube.com/watch?v=rnqF6S7PfFA
Dibujar silueta: https://www.youtube.com/watch?v=ehyMwVnnnTg

*/
//Backup antes de poner los estados en lugar de poner todas las celdas
public class MapaReinoBackup20240731 : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize;

    public List<GameObject> elGrid;

    [Header("Tile Settings")]
    public float outerSize = 1f;
    public float innerSize = 0f;
    public float height = 1f;
    public bool isFlatTopped;
    public Material material1, material2,material3,materialNeutral;
    //public TileManager elTileManager;
    
    //Para gestionar los estados del reino
    private Estado[] estados = new Estado[23];  //Creo que pondré 23, como en el juego original
    //1.-Hacer que se pueda salvar el grid y los estados
    //2.-Hacer que se pueda cargar el grid y los estados
    //3.-Hacer que los estados se dibujen con su linerenderer (esto irá en Estado.cs)
    //4.-Que al clickar en una celda se destaque el estado al que pertenece
    //5.-Que el player vaya de la capital de un estado a la del otro
    //6.-Que los enemigos también se vayan de una capital a otra
    //7.-Si hay un enemigo/player se empieza el combate
    //8.-Si no hay nadie se conquista el estado

    private void OnEnable(){
        //Creamos la lista para guardar el mapa
        elGrid = new List<GameObject>();
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
        
    }

    // Update is called once per frame
    void Update()
    {

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
                //hextile.elTileManager = elTileManager;
                
                Material elMaterial = null;
                int res = 0;
                if( x==0 || x==gridSize.x-1 || y==0 || y==gridSize.y-1 ){
                    elMaterial = materialNeutral;
                    res = -1;
                }else{
                    //de momento el material se pone random
                    res = UnityEngine.Random.Range(1,4);
                    elMaterial = null;
                    if( res == 1 ){
                        elMaterial = material1;
                    }
                    if( res ==2 ){
                        elMaterial = material2;
                    }
                    if(res == 3){
                        elMaterial = material3;
                    }
                }
                hextile.numEstado = res;//Test con tres tipos de provincia
                hextile.materialCelda = elMaterial;
                hextile.DrawMesh();

                tile.transform.SetParent(transform, true);
                elGrid.Add(tile);    //Añadimos el tile a la lista

                //Asignar sus coordenadas de desplazamiento para el uso humano (Columna, fila)
                hextile.offsetCoordinate = new Vector2Int(x,y);
                //Asignar/convertir esto a coordenadas cúbicas para navegación
                hextile.cubeCoordinate = Utilities.OffsetToCube(hextile.offsetCoordinate);
                
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
}
