using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/*
    Para el juego por turnos, representa un ejército en el mapa
 */
public class Ejercito : MonoBehaviour
{
    public Vector3Int cubeCoordinate;
    public HexTile currentTile;
    public LineRenderer _renderer;
    protected List<HexTile> currentPath;
    public HexTile nextTile;
    protected bool gotPath;
    protected Vector3 targetPosition;
    public int numPlayer = 1;   //Player al que pertenece
    //public int indiceEjercito;  //Número de ejército
    public bool haMovido = false;
    public int numCatapulta = 1;  //Cantidad de catapultas
    public int numInfanteria = 6;  //Cantidad de infantería
    public int numCaballeria = 6;  //Cantidad de caballería
    public Animator anim;
    public SkinnedMeshRenderer miSkinnedMeshRenderer;
    public Material miMaterial;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLineRenderer(List<HexTile> tiles){
        
        if (_renderer == null) { return; }

        List<Vector3> points = new List<Vector3>();
        foreach ( HexTile tile in tiles ){
            points.Add(tile.transform.position + new Vector3(0, 0.5f, 0));
        }
        _renderer.positionCount = points.Count;
        _renderer.SetPositions(points.ToArray());
    }

    public void SetPath(List<HexTile> elPath){
        currentPath = elPath;
        gotPath = true;
    }

    //Devuelve el número de casillas que le quedan para llegar a destino.
    //Así podré saber cuándo le queda una para llegar al combate y Activar la animación
    public int HandleMovement(){
        if ( currentPath == null || currentPath.Count <= 1 ){
            nextTile = null;

            if ( currentPath != null && currentPath.Count > 0 ){
                currentTile = currentPath[0];
                nextTile = currentTile;
            }

            gotPath = false;
            UpdateLineRenderer( new List<HexTile>());
            Debug.Log("Unidad ha llegado");
            //anim.Play("sword and shield idle");
            anim.SetBool("idle", true);
            anim.SetBool("andando", false);
        }else{
            anim.SetBool("idle", false);
            anim.SetBool("andando", true);
            if(currentPath.Count < 4){
                currentTile = currentPath[0];
                nextTile = currentPath[1];
            }
            else{//Para que vaya más rápido al moverse en trayectos largos
                currentTile = currentPath[1];
                nextTile = currentPath[2];
            }
            //transform.LookAt(currentTile.transform.position + new Vector3(0f,1.0f,0f));


            //Nos movemos
            this.transform.position = nextTile.transform.position + new Vector3(0,0f,0);

            //If the next tile is non traversable, stop moving
            /*
            if(nextTile.tileType != HexTileGenerationSettings.TileType.Standard){
                currentPath.Clear();
                HandleMovement();
                return;
            }*/
            //targetPosition = nextTile.transform.position + new Vector3(0,1f,0);
            gotPath = true;
            if(currentPath.Count < 4)  //Si es un camino corto, me muevo a la siguiente casilla
                currentPath.RemoveAt(0);
            else{ //Si es un camino largo, me he movido dos lugares
                currentPath.RemoveAt(0);
                currentPath.RemoveAt(1);
            }
   //         TileManager.instance.playerPos = nextTile.cubeCoordinate;
            UpdateLineRenderer(currentPath);
            //Debug.Log("UNidad en tránsito");
        }
        if(gotPath)
            return currentPath.Count;
        else
            return -1;
    }

    public bool GotCurrentPath(){
        return gotPath;
    }

    public void AnyadirTropas(int catapultas, int infanterias, int caballerias){
        numCatapulta += catapultas;
        numInfanteria += infanterias;
        numCaballeria += caballerias;
    }

    public string GetUnidades(){
        return "Catapult:"+numCatapulta+"\nInfantry:"+numInfanteria+"\nCavalry:"+numCaballeria;
    }

    public void SetUnidades(int nCatapultas, int nInfanteria, int nCaballeria){
        numCatapulta = nCatapultas;
        numInfanteria = nInfanteria;
        numCaballeria = nCaballeria;
    }

    public void Combatir(){
        anim.SetBool("idle", false);
        anim.SetBool("andando", false);
        anim.SetBool("atacando", true);
        anim.SetBool("defendiendo", false);
        anim.SetBool("muerto", false);
    }

    public void Andar(){
        anim.SetBool("idle", false);
        anim.SetBool("andando", true);
        anim.SetBool("atacando", false);
        anim.SetBool("defendiendo", false);
        anim.SetBool("muerto", false);
    }

    public void Idle(){
        anim.SetBool("idle", true);
        anim.SetBool("andando", false);
        anim.SetBool("atacando", false);
        anim.SetBool("defendiendo", false);
        anim.SetBool("muerto", false);
    }

    public void Morir(){
        Debug.Log("Ejercito.Morir(): de jugador "+numPlayer+" ha muerto.");
        anim.SetBool("idle", false);
        anim.SetBool("andando", false);
        anim.SetBool("atacando", false);
        anim.SetBool("defendiendo", false);
        anim.SetBool("muerto", true);
    }

    public int GetPoder(){
        return numCatapulta*3+numCaballeria*2+numInfanteria;
    }
}
