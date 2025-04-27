using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampoBatallaHex : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize;
    public List<GameObject> elGridCampoBatalla;

    [Header("Tile Settings")]
    public float outerSize = 1.1f;
    public float innerSize = 0f;
    public float height = 0.01f;
    public bool isFlatTopped;
    public Material[] materiales; //0 = materialNeutral, materialAgua,materialHierba,materialRoca,materialArena;
    public Vector2Int posIniCatapulta1,posIniInfanteria1,posIniCaballeria1;
    public Vector2Int posIniCatapulta2,posIniInfanteria2,posIniCaballeria2;
    public BatallaManager elBatallaManager;

    private void OnEnable(){
        //Creamos la lista para guardar el mapa
        elGridCampoBatalla = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B)){
            Debug.Log("COMENTADO: Creando campo de batalla.");
            //CrearCampoBatallaHex();
        }
    }

    private void CrearCampoBatallaHex(){
        for (int y = 0; y < gridSize.y; y++){
            for (int x = 0; x < gridSize.x; x++){
                GameObject tile = new GameObject($"Hex {x},{y}", typeof(HexTile));
                tile.transform.position = GetPositionForHexFromCoordinate( new Vector2Int(x,y))+new Vector3(0,0,-100);

                HexTile hextile = tile.GetComponent<HexTile>();
                hextile.isFlatTopped = isFlatTopped;
                hextile.outerSize = outerSize;
                hextile.innersize = innerSize;
                hextile.height = height;
                hextile.coordenada = new Vector2Int(x,y);
                hextile.materiales = materiales;
                
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
                elGridCampoBatalla.Add(tile);    //Añadimos el tile a la lista

                //Asignar sus coordenadas de desplazamiento para el uso humano (Columna, fila)
                hextile.offsetCoordinate = new Vector2Int(x,y);
                //Asignar/convertir esto a coordenadas cúbicas para navegación
                hextile.cubeCoordinate = Utilities.OffsetToCube(hextile.offsetCoordinate);
                //}
            }
        }
        elBatallaManager.CreateCombatUnitsHex();
        
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
