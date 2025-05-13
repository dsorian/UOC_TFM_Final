using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
    Esto es de soldado.cs del strategyGame01
*/

//La IA del combate real será una máquina de estados que analizará las posiciones y fuerzas enemigas y las propias
//para decidir la acción que tomar que podrá ser: 
// - Aproximarse a enemigo: Acercar sus unidades al enemigo. Irá alternando entre las distintas unidades para poder alcanzar al enemigo
//                En el caso de la catapulta deberá posicionarse para tener a un enemigo a tiro
// - Aproximarse a aliado: Acercar sus unidades a un aliado que está siendo atacado y está en inferioridad para protegerlo. 
//              Cuando esté cerca de el enemigo le atacará
// - Atacar: Tiene un enemigo al alcance, le atacaremo
// - Defender: La unidad se defenderá

public interface IEstadoUnidad{
    void ActualizaEstado();
    void AEstadoAndando();  //Antes AEstadoPaseando
    void AEstadoEligiendo();  //Antes AEstadoSentado
    void AEstadoAtacando();  //Antes AEstadoEnfadado
    void AEstadoDefendiendo();  //Nuevo
    void AEstadoDerrotado();   //Para no hacer nada salvo esperar el pase al mapa
}

public class AI_CombateReal : MonoBehaviour
{
    [HideInInspector] public IEstadoUnidad estadoActual;  //Antes IEstadoAbuelete
    [HideInInspector] public EstadoAndando estadoAndando; //Antes EstadoPaseando
    [HideInInspector] public EstadoEligiendo estadoEligiendo;   //Antes EstadoSentado
    [HideInInspector] public EstadoAtacando estadoAtacando; //Antes EstadoEnfadado
    [HideInInspector] public EstadoDefendiendo estadoDefendiendo; //Nuevo
    [HideInInspector] public EstadoDerrotado estadoDerrotado; //Nuevo

    public string player = "Player2"; //Guardar a qué player pertenece (Player1 o Player2)
    private Transform unitTarget; //Objeto de la UnidadManager que debe seguir
//    public GameObject objetivo = null; //Objetivo al que vamos a atacar
    public bool combateRealActivo = false;
    public GameObject[] unidadesManagerP1 = null, unidadesManagerP2 = null;
    public int numUnidadObjetivo;    //Número de la unidad objetivo a la que atacar
    public BatallaManager elBatallaManager;
    public string estadoDeUnidadSeleccionada = "EstadoEligiendo";

    // Start is called before the first frame update
    void Start()
    {
        //Creamos los estados de nuestra IA
        estadoAndando = new EstadoAndando(this);
        estadoEligiendo = new EstadoEligiendo(this);
        estadoAtacando = new EstadoAtacando(this);
        estadoDefendiendo = new EstadoDefendiendo(this);
        estadoActual = estadoEligiendo;
        elBatallaManager.unidadSeleccionadaP2 = UnityEngine.Random.Range(0,3);  //Empezaremos moviendo una unidad random
    }

    // Update is called once per frame
    void Update()
    {
        if( ! combateRealActivo || elBatallaManager.EsFinBatalla() )
            return;
        switch (estadoActual)
        {
            case EstadoEligiendo:
                estadoDeUnidadSeleccionada = "EstadoEligiendo";
                break;
            case EstadoAndando:
                estadoDeUnidadSeleccionada = "EstadoAndando";
                break;
            case EstadoAtacando:
                estadoDeUnidadSeleccionada = "EstadoAtacando";
                break;
            case EstadoDefendiendo:
                estadoDeUnidadSeleccionada = "EstadoDefendiendo";
                break;
            default:
                Debug.Log("Estado indeterminado, ERROR");
            break;
        }
        
        estadoActual.ActualizaEstado();
    }

    //Para comprobar que el resto de unidades están paradas y pueda hacer una acción. Si no, lo hace tan rápido que parece que mueve varias
    //unidades a la vez
    public bool RestoUnidadesEstanIdle(int unidadConsulta){
        bool primeraUnidadIdle = false;
        bool segundaUnidadIdle = false;

        if(unidadConsulta == 0){
            primeraUnidadIdle = unidadesManagerP2[1].GetComponent<UnidadManager>().TodosIdle();
            segundaUnidadIdle = unidadesManagerP2[2].GetComponent<UnidadManager>().TodosIdle();
        }
        if(unidadConsulta == 1){
            primeraUnidadIdle = unidadesManagerP2[0].GetComponent<UnidadManager>().TodosIdle();
            segundaUnidadIdle = unidadesManagerP2[2].GetComponent<UnidadManager>().TodosIdle();
        }
        if(unidadConsulta == 2){
            primeraUnidadIdle = unidadesManagerP2[0].GetComponent<UnidadManager>().TodosIdle();
            segundaUnidadIdle = unidadesManagerP2[1].GetComponent<UnidadManager>().TodosIdle();
        }
        return primeraUnidadIdle && segundaUnidadIdle;
    }

    //Comprobar si hay alguna unidad aliada que esté siendo atacada y que esté en peligro. Si es así, se defenderá
    public int UnidadAmenazada(){
        int unidadAmenazada=0;
        float distancia = 100000;
        float distAux;
        for(int i = 0; i<unidadesManagerP2.Length;i++){
            distAux = 10000;
            if( ! unidadesManagerP2[i].GetComponent<UnidadManager>().muerto){
                for(int j = 0; j<unidadesManagerP1.Length;j++){
                    if( ! unidadesManagerP1[j].GetComponent<UnidadManager>().muerto){
                        distAux = Vector3.Distance(unidadesManagerP2[i].transform.position,unidadesManagerP1[j].transform.position);
                        if(distAux < distancia){
                            unidadAmenazada = i;
                            distancia = distAux;
                        }
                    }
                }
            }
        }
        if( distancia > 35)
            unidadAmenazada = -1; //No hay ningún enemigo cerca
        
        return unidadAmenazada;
    }

    //Para que cuando comience el combate elija una unidad que no esté muerta
    public void EscogerUnidadObjetivo(){
        elBatallaManager.unidadSeleccionadaP2 = elBatallaManager.SiguienteUnidadManagerP2();
    }
}
