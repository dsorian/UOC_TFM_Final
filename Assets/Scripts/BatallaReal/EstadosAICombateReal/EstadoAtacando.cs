using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EstadoAtacando : IEstadoUnidad
{
        
    AI_CombateReal fsmAIController;
    private float tiempoActualDecision = 0.25f; //Para contar el tiempo que ha pasado desde la última decisión
    private float tiempoDecision = 0.5f;  //El agente tomará una decisión cada tiempoActualDecision segundos
    private float tiempoActualAtacando = 0.5f;  //Para contar el tiempo que lleva atacando y para en algún momento
    private float tiempoAtacando = 2.0f; //Atacará sin parar este tiempo
    private bool catapultaAtacando = false;  //Para saber si la catapulta está cargando
    public EstadoAtacando(AI_CombateReal laAI){
        fsmAIController = laAI;
    }

    public void ActualizaEstado()
    {
        tiempoActualDecision += Time.deltaTime;
        //Cada tiempoDecision comprueba si hay que hacer alguna otra acción (el destino se ha movido, o lo que sea)
        tiempoActualAtacando += Time.deltaTime;        
        if( tiempoActualDecision > tiempoDecision ){
            tiempoActualDecision = 0;
            Debug.Log("EstadoAtacando. Toca decidir tiempoActualAtacando: "+tiempoActualAtacando+" elBatallaManager.unidadSeleccionadaP2: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2+" catapultaAtacando: "+catapultaAtacando);
            
            if( fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 0){
                if( !catapultaAtacando ){
                    Debug.Log("EstadoAtacando: CATAPULTA: Inicio ataque catapulta.");
                    catapultaAtacando = true;
                }else{
                    float distanciaObjetivo = Vector3.Distance(fsmAIController.unidadesManagerP1[fsmAIController.elBatallaManager.unidadSeleccionadaP1].transform.position,fsmAIController.unidadesManagerP2[0].transform.position);
                    Debug.Log("EstadoAtacando: CATAPULTA La catapulta calcula la distancia al objetivo donde debe ir para disparar. distanciaObjetivo: "+distanciaObjetivo+" fuerzaCatapulta: "+fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().fuerzaCatapulta);
                    //El punto de impacto será el calculado +/- 4 unidades para que no sea siempre lo mismo
                    float variacion = (UnityEngine.Random.Range(0,2)*2-1) * (UnityEngine.Random.Range(0,4.0f));
                    if( distanciaObjetivo + variacion <= fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().fuerzaCatapulta){
                        Debug.Log("EstadoAtacando: Disparando catapulta!!!! y me voy a eligiendo distanciaObjetivo: "+distanciaObjetivo);
                        AEstadoEligiendo();
                    }
                }
            }else{
                Debug.Log("EstadoAtacando: tiempoActualAtacando - tiempoAtacando: "+tiempoActualAtacando+" - "+ tiempoAtacando+" unidadcontrolada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
                if( tiempoActualAtacando > tiempoAtacando){
                    Debug.Log("EstadoAtacando: Ya he atacado bastante, me voy a estado Eligiendo.");
                    AEstadoEligiendo();
                }else{
                    Debug.Log("EstadoAtacando:  No he atacado, iniciando ataque. unidadControlada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
                    fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().IniciarAtaque();
                }
            }
        }
    }

    public void AEstadoAndando(){
        Debug.Log("EstadoAtacando: A EstadoAndando...");
        tiempoActualAtacando = 0;
        catapultaAtacando = false;
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
                fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        if(fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 2)
            fsmAIController.elBatallaManager.elSoundManager.PlayRandomSound(fsmAIController.elBatallaManager.elSoundManager.sonidosCaballos,0.5f,"Batalla");
        fsmAIController.elBatallaManager.elSoundManager.PlayMusic(fsmAIController.elBatallaManager.elSoundManager.musicaUnidadSeleccionada[fsmAIController.elBatallaManager.unidadSeleccionadaP2],true,0.5f,"UnidadSeleccionadaP2Source");

        fsmAIController.estadoActual = fsmAIController.estadoAndando;
    }

    public void AEstadoEligiendo(){
        Debug.Log("EstadoAtacando: A EstadoEligiendo...");
        if( fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 0){
            Debug.Log("EstadoAtacando: La catapulta dispara y acaba.");
            catapultaAtacando = false;
            fsmAIController.unidadesManagerP2[0].GetComponent<UnidadManager>().DispararCatapulta();
            fsmAIController.unidadesManagerP2[0].GetComponent<UnidadManager>().FinalizarAtaque();
        }
        tiempoActualAtacando = 0;
        catapultaAtacando = false;
        fsmAIController.estadoActual = fsmAIController.estadoEligiendo;
    }

    public void AEstadoAtacando(){
        Debug.Log("Unidad: ... de EstadoAtacando a aEstadoAtacando.");
    }

    public void AEstadoDefendiendo(){
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        Debug.Log("Unidad: ... de EstadoAtacando a estadodefendiendo");
    }
    public void AEstadoDerrotado(){
        Debug.Log("EstadoAtacando: AEstadoDerrotado");
        tiempoActualAtacando = 0;
        catapultaAtacando = false;
        fsmAIController.estadoActual = fsmAIController.estadoDerrotado;
    }
}
