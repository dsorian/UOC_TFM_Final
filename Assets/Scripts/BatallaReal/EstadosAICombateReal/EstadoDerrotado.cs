using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//No hacemos nada en este estado salvo esperar el fin de la batalla para volver al mapa
public class EstadoDerrotado : IEstadoUnidad
{
         
    AI_CombateReal fsmAIController;
    private float tiempoActualDecision = 0.25f; //Para contar el tiempo que ha pasado desde la última decisión
    private float tiempoDecision = 0.5f;  //El agente tomará una decisión cada tiempoActualDecision segundos

    public EstadoDerrotado(AI_CombateReal unidad){
        fsmAIController = unidad;
    }

    public void ActualizaEstado()
    {
        //Debug.Log("IA Real: EstadoAndando: Actualizando Estadoooo....");
        tiempoActualDecision += Time.deltaTime;
        //Cada tiempoDecision comprueba si hay que hacer alguna otra acción (el destino se ha movido, o lo que sea)
        
        if( tiempoActualDecision > tiempoDecision ){
            tiempoActualDecision = 0;
            //No hacemos nada en este estado salvo esperar el fin de la batalla para volver al mapa
        }
    }

    public void AEstadoAndando(){
        Debug.Log("IA Real: EstadoDerrotado: A EstadoAndando...");
    }

    public void AEstadoEligiendo(){
        Debug.Log("IA Real: EstadoDerrotado: A EstadoEligiendo...");
    }

    public void AEstadoAtacando(){
        Debug.Log("IA Real: Unidad: ... de EstadoDerrotado a aEstadoAtacando.");
    }

    public void AEstadoDefendiendo(){
        Debug.Log("IA Real: Unidad: ... de EstadoDerrotado a estadodefendiendo");
    }

    public void AEstadoDerrotado(){
        Debug.Log("IA Real: EstadoDerrotado");
    }
}
