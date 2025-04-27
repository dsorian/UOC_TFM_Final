using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EstadoDefendiendo : IEstadoUnidad
{
            
    AI_CombateReal fsmAIController;
    private float tiempoActualDecision = 0.25f; //Para contar el tiempo que ha pasado desde la última decisión
    private float tiempoDecision = 0.5f;  //El agente tomará una decisión cada tiempoActualDecision segundos

    public EstadoDefendiendo(AI_CombateReal unidad){
        fsmAIController = unidad;
    }

    public void ActualizaEstado()
    {
        //Debug.Log("EstadoAndando: Actualizando Estadoooo....");
        tiempoActualDecision += Time.deltaTime;
        //Cada tiempoDecision comprueba si hay que hacer alguna otra acción (el destino se ha movido, o lo que sea)
        
        if( tiempoActualDecision > tiempoDecision ){
            tiempoActualDecision = 0;

            //El agente ha llegado a su destino, le damos un destino nuevo o lo paramos
            //Del abuelete: if( Vector3.Distance(fsmUnidad.transform.position, fsmUnidad.agent.destination )  < 2){
            if( UnityEngine.Random.Range(0,100) < 50 ){  //Probabilidad del 50% de moverse
                AEstadoEligiendo();
            }            
        }
    }

    public void AEstadoAndando(){
        Debug.Log("EstadoDefendiendo: A EstadoAndando...");
    }

    public void AEstadoEligiendo(){
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        Debug.Log("EstadoMuerto: A EstadoEligiendo...");
    }

    public void AEstadoAtacando(){
        Debug.Log("Unidad: ... de EstadoDefendiendo a aEstadoAtacando.");
    }

    public void AEstadoDefendiendo(){
        Debug.Log("Unidad: ... de EstadoDefendiendo a estadodefendiendo");
    }

    public void AEstadoDerrotado(){
        Debug.Log("EstadoDefendiendo: AEstadoDerrotado");
    }
}
