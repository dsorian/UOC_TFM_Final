using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using TMPro;




/*
    Para gestionar las batallas
*/
public class BatallaManager : MonoBehaviour
{
    public TileManager elTileManager;
    public CampoBatallaHex elCampoBatallaHex;
    public CampoBatallaTerrain elCampoBatallaTerrain;
    public GameObject elCanvasUI_Batalla;
    public Material[] materialesEjercito;
    public GameObject modeloCatapulta, modeloInfanteria, modeloCaballeria,modeloUnidadManager;
    public GameObject elEjercitoP1 = null, elEjercitoP2 = null;
    private GameObject[] unidadesManagerP1,unidadesManagerP2;
    public Vector2Int posIniCatapultaP1,posIniInfanteriaP1,posIniCaballeriaP1;
    public Vector2Int posStartCatapultaP2,posStartInfanteriaP2,posStartCaballeriaP2;
    public Vector2Int posIniCatapultaP2,posIniInfanteriaP2,posIniCaballeriaP2;
    public int unidadSeleccionadaP1=0,unidadSeleccionadaP2=0;
    public bool tutorialActivo; //Para mostrar el tutorial y enseñar cómo se juega
    public string ejercitoAtacante = "";
    //private bool oponenteCPU;
    public AI_CombateReal laAICombateReal;
    public SoundManager elSoundManager;
   
    // Start is called before the first frame update
    void Start(){
        if( PlayerPrefs.GetInt("tutorialActivo") == 1)
            tutorialActivo = true;
        else
            tutorialActivo = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(tutorialActivo)
            return;
            
        if( ! elTileManager.combateActivo)
            return;

        if(EsFinBatalla()){
            Debug.Log("Ha ganado un player, habría que volver al mapa.");
            FinalizarBatalla();
            return;
        }

        if(Input.GetButtonDown("SiguienteUnidadP1")){
            unidadSeleccionadaP1 = SiguienteUnidadManagerP1();

            if( elSoundManager.UnidadSeleccionadaP1Source.isPlaying)
                elSoundManager.StopMusic("UnidadSeleccionadaP1Source");
            if(unidadSeleccionadaP1 == 2)
                elSoundManager.PlayRandomSound(elSoundManager.sonidosCaballos,0.5f,"Batalla");
            elSoundManager.PlayMusic(elSoundManager.musicaUnidadSeleccionada[unidadSeleccionadaP1],true,0.5f,"UnidadSeleccionadaP1Source");
        }
        if(Input.GetButtonDown("SiguienteUnidadP2")){
            unidadSeleccionadaP2 = SiguienteUnidadManagerP2();
            
            if( elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
                elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
            if(unidadSeleccionadaP2 == 2)
                elSoundManager.PlayRandomSound(elSoundManager.sonidosCaballos,0.5f,"Batalla");
            elSoundManager.PlayMusic(elSoundManager.musicaUnidadSeleccionada[unidadSeleccionadaP2],true,0.5f,"UnidadSeleccionadaP2Source");
        }
    }

    public void CreateCombatUnitsHex(){
        Debug.Log("Creando unidad de infantería.");
        Vector3 posInicial = elCampoBatallaHex.elGridCampoBatalla[elCampoBatallaHex.posIniInfanteria1.y*elCampoBatallaHex.gridSize.y+elCampoBatallaHex.posIniInfanteria1.x].transform.position;
        //infanteriaP1 = Instantiate(modeloInfanteria,posInicial + new Vector3(0,1,0), new Quaternion());
    }

    public void CreateAllCombatUnitsTerrain(){
        Vector3 posInicial = Vector3.zero;
        unidadesManagerP1 = new GameObject[3];
        unidadesManagerP2 = new GameObject[3];
        if(elEjercitoP1 == null){  //Para debug, creamos dos ejércitos completos
            Debug.Log("Creando unidades. Son null. Creo ejército completo.");
           
            //Player 1
            posInicial = new Vector3(posIniCatapultaP1.x,80,posIniCatapultaP1.y);
            unidadesManagerP1[0] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            unidadesManagerP1[0].GetComponent<UnidadManager>().CrearUnidades(modeloCatapulta,2,materialesEjercito[0],materialesEjercito[1],1,true,0);
            unidadSeleccionadaP1 = 0;

            posInicial = new Vector3(posIniInfanteriaP1.x,80,posIniInfanteriaP1.y);
            unidadesManagerP1[1] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            unidadesManagerP1[1].GetComponent<UnidadManager>().CrearUnidades(modeloInfanteria,6,materialesEjercito[4],null,1,false,1);

            posInicial = new Vector3(posIniCaballeriaP1.x,80,posIniCaballeriaP1.y);
            unidadesManagerP1[2] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            unidadesManagerP1[2].GetComponent<UnidadManager>().CrearUnidades(modeloCaballeria,6,materialesEjercito[6],materialesEjercito[7],1,false, 2);
            
            //
            //Player 2
            //
            posInicial = new Vector3(posIniCatapultaP2.x,80,posIniCatapultaP2.y);
            unidadesManagerP2[0] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            unidadesManagerP2[0].GetComponent<UnidadManager>().CrearUnidades(modeloCatapulta,2,materialesEjercito[2],materialesEjercito[3],2,true,0);
            unidadSeleccionadaP2 = 0;

            posInicial = new Vector3(posIniInfanteriaP2.x,80,posIniInfanteriaP2.y);
            unidadesManagerP2[1] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            unidadesManagerP2[1].GetComponent<UnidadManager>().CrearUnidades(modeloInfanteria,6,materialesEjercito[5],null,2,false,1);

            posInicial = new Vector3(posIniCaballeriaP2.x,80,posIniCaballeriaP2.y);
            unidadesManagerP2[2] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            unidadesManagerP2[2].GetComponent<UnidadManager>().CrearUnidades(modeloCaballeria,6,materialesEjercito[8],materialesEjercito[9],2,false,2);
        }else{ //Creamos las unidades que me pasan
            Debug.Log("Creando las unidades que me han pasado desde el mapa.");

            int nCatapultaP1 = elEjercitoP1.GetComponent<Ejercito>().numCatapulta;
            int nInfanteriaP1 = elEjercitoP1.GetComponent<Ejercito>().numInfanteria;
            int nCaballeriaP1 = elEjercitoP1.GetComponent<Ejercito>().numCaballeria;

            int nCatapultaP2 = elEjercitoP2.GetComponent<Ejercito>().numCatapulta;
            int nInfanteriaP2 = elEjercitoP2.GetComponent<Ejercito>().numInfanteria;
            int nCaballeriaP2 = elEjercitoP2.GetComponent<Ejercito>().numCaballeria;
            
            Debug.Log("Unides P1:\n Catapultas: "+nCatapultaP1+" Soldados: "+nInfanteriaP1+" Caballeria: "+nCaballeriaP1);

            unidadesManagerP1 = new GameObject[3];
            unidadesManagerP2 = new GameObject[3];

            //Player 1

            posInicial = new Vector3(posIniCatapultaP1.x,80,posIniCatapultaP1.y);
            unidadesManagerP1[0] = Instantiate(modeloUnidadManager, posInicial , new Quaternion());
            if( nCatapultaP1 == 0)
                unidadesManagerP1[0].GetComponent<UnidadManager>().muerto = true;
            Debug.Log("Creando catapultas P1.");
            unidadesManagerP1[0].GetComponent<UnidadManager>().CrearUnidades(modeloCatapulta,nCatapultaP1,materialesEjercito[0],materialesEjercito[1],1,true,0);
            
            posInicial = new Vector3(posIniInfanteriaP1.x,80,posIniInfanteriaP1.y);
            unidadesManagerP1[1] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            if( nInfanteriaP1 == 0)
                unidadesManagerP1[1].GetComponent<UnidadManager>().muerto = true;
            unidadesManagerP1[1].GetComponent<UnidadManager>().CrearUnidades(modeloInfanteria,nInfanteriaP1,materialesEjercito[4],null,1,false,1);

            posInicial = new Vector3(posIniCaballeriaP1.x,80,posIniCaballeriaP1.y);
            unidadesManagerP1[2] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            if( nCaballeriaP1 == 0)
                unidadesManagerP1[2].GetComponent<UnidadManager>().muerto = true;
            unidadesManagerP1[2].GetComponent<UnidadManager>().CrearUnidades(modeloCaballeria,nCaballeriaP1,materialesEjercito[6],materialesEjercito[7],1,false, 2);
            unidadSeleccionadaP1 = 0;
            if( nCatapultaP1 == 0)
                unidadSeleccionadaP1 = 1;
                if( nInfanteriaP1 == 0 )
                    unidadSeleccionadaP1 = 2;
            //
            //Player 2
            //

            posInicial = new Vector3(posIniCatapultaP2.x,80,posIniCatapultaP2.y);
            unidadesManagerP2[0] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            if( nCatapultaP2 == 0)
                unidadesManagerP2[0].GetComponent<UnidadManager>().muerto = true;
            unidadesManagerP2[0].GetComponent<UnidadManager>().CrearUnidades(modeloCatapulta,nCatapultaP2,materialesEjercito[2],materialesEjercito[3],2,true,0);
            unidadSeleccionadaP2 = 0;

            posInicial = new Vector3(posIniInfanteriaP2.x,80,posIniInfanteriaP2.y);
            unidadesManagerP2[1] = Instantiate(modeloUnidadManager,posInicial ,  new Quaternion());//* Quaternion.Euler (0f, 180f, 0f)
            if( nInfanteriaP2 == 0)
                unidadesManagerP2[1].GetComponent<UnidadManager>().muerto = true;
            unidadesManagerP2[1].GetComponent<UnidadManager>().CrearUnidades(modeloInfanteria,nInfanteriaP2,materialesEjercito[5],null,2,false,1);

            posInicial = new Vector3(posIniCaballeriaP2.x,80,posIniCaballeriaP2.y);
            unidadesManagerP2[2] = Instantiate(modeloUnidadManager,posInicial , new Quaternion());
            if( nCaballeriaP2 == 0)
                unidadesManagerP2[2].GetComponent<UnidadManager>().muerto = true;
            unidadesManagerP2[2].GetComponent<UnidadManager>().CrearUnidades(modeloCaballeria,nCaballeriaP2,materialesEjercito[8],materialesEjercito[9],2,false,2);
        }

        unidadesManagerP1[0].GetComponent<UnidadManager>().elBatallaManager = this;
        unidadesManagerP1[1].GetComponent<UnidadManager>().elBatallaManager = this;
        unidadesManagerP1[2].GetComponent<UnidadManager>().elBatallaManager = this;

        unidadesManagerP2[0].GetComponent<UnidadManager>().elBatallaManager = this;
        unidadesManagerP2[1].GetComponent<UnidadManager>().elBatallaManager = this;
        unidadesManagerP2[2].GetComponent<UnidadManager>().elBatallaManager = this;

        laAICombateReal.unidadesManagerP1 = unidadesManagerP1;
        laAICombateReal.unidadesManagerP2 = unidadesManagerP2;
        laAICombateReal.player = "Player2";   //Siempre llevará al Player2
    }

    private void DestruirUnidades(){
        Debug.Log("Destruyendo todas las unidades.");
        foreach( GameObject laUnidadManager in unidadesManagerP1){
            laUnidadManager.GetComponent<UnidadManager>().EliminarTodasUnidades();
            laUnidadManager.GetComponent<UnidadManager>().muerto = true;
            Destroy(laUnidadManager);
        }
        foreach( GameObject laUnidadManager in unidadesManagerP2){
            laUnidadManager.GetComponent<UnidadManager>().EliminarTodasUnidades();
            laUnidadManager.GetComponent<UnidadManager>().muerto = true;
            Destroy(laUnidadManager);
        }
    }

    private int UnidadManagerDestruidas(int numPlayer){
        int contador = 0;
        if( numPlayer == 1){
            for(int i = 0 ; i < unidadesManagerP1.Length; i++){
                if (unidadesManagerP1[i] == null || unidadesManagerP1[i].gameObject.GetComponent<UnidadManager>().muerto)
                    contador++;
            }
        }
        if( numPlayer == 2){
            for(int i = 0 ; i < unidadesManagerP2.Length; i++){
                if (unidadesManagerP2[i] == null || unidadesManagerP2[i].gameObject.GetComponent<UnidadManager>().muerto)
                    contador++;
            }
        }
//        Debug.Log("El player: "+numPlayer+" tiene unidades destruidas: "+contador);
        return contador;
    }

    public int SiguienteUnidadManagerP1(){
        if(EsFinBatalla())
            return -1;
        unidadesManagerP1[unidadSeleccionadaP1].GetComponent<UnidadManager>().SeleccionarUnidad(false);
        unidadesManagerP1[unidadSeleccionadaP1].GetComponent<UnidadManager>().PararUnidades();
        unidadSeleccionadaP1++;
        if(unidadSeleccionadaP1 > 2)
            unidadSeleccionadaP1 = 0;
        while (unidadesManagerP1[unidadSeleccionadaP1].GetComponent<UnidadManager>().muerto){
            unidadSeleccionadaP1++;
            if(unidadSeleccionadaP1 > 2)
                unidadSeleccionadaP1 = 0;
        }
        unidadesManagerP1[unidadSeleccionadaP1].GetComponent<UnidadManager>().SeleccionarUnidad(true);

        return unidadSeleccionadaP1;
    }

    public int SiguienteUnidadManagerP2(){
        if(EsFinBatalla())
            return -1;
        unidadesManagerP2[unidadSeleccionadaP2].GetComponent<UnidadManager>().SeleccionarUnidad(false);
        unidadesManagerP2[unidadSeleccionadaP2].GetComponent<UnidadManager>().PararUnidades();
        unidadSeleccionadaP2++;
        if(unidadSeleccionadaP2 > 2)
            unidadSeleccionadaP2 = 0;
        while (unidadesManagerP2[unidadSeleccionadaP2].GetComponent<UnidadManager>().muerto){
            unidadSeleccionadaP2++;
            if(unidadSeleccionadaP2 > 2)
                unidadSeleccionadaP2 = 0;
        }
        unidadesManagerP2[unidadSeleccionadaP2].GetComponent<UnidadManager>().SeleccionarUnidad(true);

        return unidadSeleccionadaP2;
    }

    public bool EsFinBatalla(){
//        Debug.Log("BatallaManager.EsFinBatalla() comprobando si la batalla se ha acabado.");
        bool esFin = false;
        if(UnidadManagerDestruidas(2) == 3){
            esFin = true;
        }
        if(UnidadManagerDestruidas(1) == 3){
            esFin = true;
        }
        return esFin;
    }

    public bool HayCombateActivo(){
        return elTileManager.combateActivo;
    }

    public void FinalizarBatalla(){

        if(UnidadManagerDestruidas(2) == 3){
            Debug.Log("¡¡¡¡Victoria del player 1!!!! Hacer lo que toque");
            elTileManager.combateActivo = false;

            elEjercitoP1.GetComponent<Ejercito>().SetUnidades(unidadesManagerP1[0].GetComponent<UnidadManager>().numTotalUnidades,unidadesManagerP1[1].GetComponent<UnidadManager>().numTotalUnidades,unidadesManagerP1[2].GetComponent<UnidadManager>().numTotalUnidades);
            elTileManager.combateActivo = false;
            if(ejercitoAtacante == "Player1")
                elTileManager.vencedor = "atacante";
            else
                elTileManager.vencedor = "defensor";
        }

        if(UnidadManagerDestruidas(1) == 3){
            Debug.Log("¡¡¡¡Victoria del player 2!!!! Hacer lo que toque");
            elTileManager.combateActivo = false;

            elEjercitoP2.GetComponent<Ejercito>().SetUnidades(unidadesManagerP2[0].GetComponent<UnidadManager>().numTotalUnidades,unidadesManagerP2[1].GetComponent<UnidadManager>().numTotalUnidades,unidadesManagerP2[2].GetComponent<UnidadManager>().numTotalUnidades);
            elTileManager.combateActivo = false;
            if(ejercitoAtacante == "Player2")
                elTileManager.vencedor = "atacante";
            else
                elTileManager.vencedor = "defensor";
        }
    }
    public void DestruirCampoBatalla(){
        DestruirUnidades();
        elCampoBatallaTerrain.GetComponent<CampoBatallaTerrain>().DestroyTerrain();
        elTileManager.combateActivo = false;
    }
}